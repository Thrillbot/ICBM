using Mirror;
using Rewired;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Universe;

public class BaseSpawner : NetworkBehaviour
{
	[SyncVar(hook = "SetHealth")]
	public float health = 100;
	public Image healthbar;

	public Transform[] pieSlices;
	public int playerId = 0;

	[SyncVar(hook = "SetName")]
	public string playerName;
	public TMP_Text nameTag;
	[SyncVar(hook = "SetPlayerIcon")]
	public Texture2D playerIcon;
	public RawImage playerFlag;

	private Vector3 workerVec;
	private Player player;
	[SyncVar(hook = "SetInitialized")]
	private bool initialized = false;

	[SyncVar(hook = "SetVisible")]
	private bool visible;
	[SyncVar(hook = "SetPlaced")]
	private bool placed;

	private TMP_Text debugText;
	private List<Explosion> explosions;

	void Awake()
	{
		// Get the Rewired Player object for this player and keep it for the duration of the character's lifetime
		player = ReInput.players.GetPlayer(playerId);
	}

	public bool Visible
	{
		get { return visible; }
		set { visible = value; SetVisible(!value, value); }
	}

	public void Initialize(int playerIndex)
	{
		InitializeRpc(playerIndex);
	}

	[ClientRpc]
	public void InitializeRpc(int playerIndex)
	{
		if (isLocalPlayer)
		{
			pieSlices = new Transform[840 / NetworkServer.connections.Count];
			int index = 0;
			for (int i = 840 / NetworkServer.connections.Count * playerIndex; i < 840 / (NetworkServer.connections.Count * playerIndex + 1); i++)
			{
				pieSlices[index] = Worldificate.chunkList[i].transform;
				index++;
			}

			if (debugText == null)
			{
				foreach (TMP_Text t in FindObjectsOfType<TMP_Text>()) {
					if (t.name.Contains("Debug"))
					{
						debugText = t;
						break;
					}
				}
			}

			Transform cam = FindObjectOfType<FreeCam>().transform;
			cam.LookAt(pieSlices[pieSlices.Length/2].forward);
			cam.localEulerAngles = new Vector3(0, cam.localEulerAngles.y - 22.5f, 0);

			transform.parent = Planet.transform;
			SetInitialized(false, true);
		}
	}

	public void ApplyDamage (float damage)
	{
		Debug.Log("Damaging by " + damage + " of total " + health);
		health -= damage;
		SetHealth(100, health);
		if (health <= 0)
		{
			Debug.Log("Killing Base");
			BaseDeath();
		}
	}

	[Command]
	void BaseDeath ()
	{
		NetworkServer.Destroy(gameObject);
	}

	void SetVisible(bool oldValue, bool newValue)
	{
		visible = newValue;
	}

	void SetPlaced(bool oldValue, bool newValue)
	{
		placed = newValue;
	}

	void SetInitialized(bool oldValue, bool newValue)
	{
		initialized = newValue;
	}

	public void SetName(string oldValue, string newValue)
	{
		playerName = newValue;
		Debug.Log("Setting Player Name Tag: " + playerName);
		nameTag.text = playerName;
	}

	public void SetPlayerIcon(Texture2D oldValue, Texture2D newValue)
	{
		playerIcon = newValue;
		playerFlag.texture = newValue;
	}

	public void FocusBase ()
	{
		GameObject.FindWithTag("FreeCam").GetComponent<FreeCam>().Focus(transform, 5);
	}

	public void AbortFlight ()
	{
		if (GameObject.FindWithTag("FreeCam").GetComponent<FreeCam>().target.GetComponent<Part>())
			GameObject.FindWithTag("FreeCam").GetComponent<FreeCam>().target.GetComponent<Part>().Abort();
	}

	void SetHealth (float oldvalue, float newValue)
	{
		health = newValue;
		healthbar.fillAmount = health / 100f;
	}

	private void Update()
	{
		//if (isLocalPlayer)
		//	debugText.text = "Initialized = " + initialized;

		if (!isLocalPlayer || !initialized)
			return;

		if (placed)
		{
			explosions ??= new();
			foreach (Explosion e in FindObjectsOfType<Explosion>())
			{
				if (explosions.Contains(e)) continue;
				explosions.Add(e);

				if ((e.transform.position - transform.position).sqrMagnitude <= e.explosionRadius * e.explosionRadius)
				{
					Debug.Log("Applying Damage");
					ApplyDamage(e.explosionDamage);
				}
			}
		}

		if (isLocalPlayer && !placed)
		{
			try
			{
				workerVec = GetPointOnPlanet(GetMousePointOnPlane());
			}
			catch
			{
				return;
			}

			transform.position = workerVec;
			transform.LookAt(transform.position * 2f);
			workerVec = transform.localEulerAngles;
			workerVec.z = 90;
			transform.localEulerAngles = workerVec;

			workerVec = GetPointOnPlanet(GetMousePointOnPlane());

			Visible = false;

			foreach (Transform t in pieSlices)
			{
				if (Vector3.SignedAngle(workerVec.normalized, t.GetChild(0).forward, Vector3.forward) > 0 && Vector3.SignedAngle(workerVec.normalized, t.GetChild(0).forward, Vector3.forward) < 1)
				{
					Visible = true;
					break;
				}
			}

			foreach (MeshRenderer m in GetComponentsInChildren<MeshRenderer>())
			{
				m.enabled = Visible;
			}

			if (Visible && player.GetButtonUp("Interact"))
			{
				debugText.text = "";
				placed = true;
				SetPlaced(false, true);
			}
		}
	}

	public void FixedUpdate()
	{
		//if (!isLocalPlayer)
		//	return;

		if (!placed)
		{
			foreach (MeshRenderer m in GetComponentsInChildren<MeshRenderer>())
			{
				m.enabled = Visible;
			}
		}
		else
		{
			foreach (MeshRenderer m in GetComponentsInChildren<MeshRenderer>())
			{
				m.enabled = true;
			}
		}
	}
}