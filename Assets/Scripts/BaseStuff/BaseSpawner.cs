using Mirror;
using Rewired;
using System.Collections;
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

	private Vector2 zoneAngles;
	private float workerFloat;

	public struct InitArgs
	{
		public int playerIndex;
		public Color playerColor;
	}

	void Awake()
	{
		player = ReInput.players.GetPlayer(playerId);

		transform.parent = Planet.transform;
	}

	public bool Visible
	{
		get { return visible; }
		set { visible = value; }
	}

	public void Initialize(InitArgs args)
	{
		InitializeRpc(args.playerIndex, args.playerColor, NetworkServer.connections.Count);
	}

	[ClientRpc]
	public void InitializeRpc(int playerIndex, Color playerColor, int connCount)
	{
		StartCoroutine(InitializeCoroutine(playerIndex, playerColor, connCount));
	}

	IEnumerator InitializeCoroutine (int playerIndex, Color playerColor, int connCount)
	{
		while (!GameManager.NoiseSyncd) yield return null;

		Planet.GetComponent<Worldificate>().GenerateWorld();

		zoneAngles = new Vector2(360f / (float)connCount * (float)playerIndex, 360f / (float)connCount * (float)(playerIndex + 1));

		if (isLocalPlayer)
		{
			if (debugText == null)
			{
				foreach (TMP_Text t in FindObjectsOfType<TMP_Text>())
				{
					if (t.name.Contains("Debug"))
					{
						debugText = t;
						break;
					}
				}
			}

			Transform cam = FindObjectOfType<FreeCam>().transform;

			cam.LookAt(Quaternion.Euler(0, 0, (zoneAngles.y - zoneAngles.x) / 2 + zoneAngles.x) * Vector3.up);
			cam.localEulerAngles = new Vector3(0, cam.localEulerAngles.y, 0);

			transform.parent = Planet.transform;
			initialized = true;

			GameObject.FindWithTag("LoadingScreen").SetActive(false);
		}
	}

	public void ApplyDamage (float damage)
	{
		health -= damage;
		if (health <= 0) BaseDeath();
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
		nameTag.text = playerName;
	}

	public void SetPlayerIcon(Texture2D oldValue, Texture2D newValue)
	{
		playerIcon = newValue;
		playerFlag.texture = newValue;
	}

	void SetHealth(float oldvalue, float newValue)
	{
		health = newValue;
		healthbar.fillAmount = health / 100f;
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

	private void Update()
	{
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

			workerFloat = Vector3.SignedAngle(workerVec.normalized, Vector3.up, Vector3.forward);
			if (workerFloat < 0) workerFloat += 360f;

			visible = false;
			if (workerFloat > zoneAngles.x && workerFloat < zoneAngles.y)
			{
				visible = true;
			}

			foreach (MeshRenderer m in GetComponentsInChildren<MeshRenderer>())
			{
				m.enabled = visible;
			}

			if (visible && player.GetButtonUp("Interact"))
			{
				debugText.text = "";
				placed = true;
			}
		}
	}

	public void FixedUpdate()
	{
		if (!placed)
		{
			foreach (MeshRenderer m in GetComponentsInChildren<MeshRenderer>())
			{
				m.enabled = visible;
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