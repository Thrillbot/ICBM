using Mirror;
using Rewired;
using TMPro;
using UnityEngine;
using static Universe;

public class BaseSpawner : NetworkBehaviour
{
	public float health = 100;

	public GameObject planet;
	[SyncVar(hook = "SetPieSlice")]
	public Transform pieSlice;
	public int playerId = 0;

	private Vector3 workerVec;
	private Player player;
	[SyncVar(hook = "SetInitialized")]
	private bool initialized = false;

	[SyncVar(hook = "SetVisible")]
	private bool visible;
	[SyncVar(hook = "SetPlaced")]
	private bool placed;

	private TMP_Text debugText;

	void Awake()
	{
		// Get the Rewired Player object for this player and keep it for the duration of the character's lifetime
		player = ReInput.players.GetPlayer(playerId);

		GameManager.baseLocation = transform;
	}

	public bool Visible
	{
		get { return visible; }
		set { visible = value; SetVisible(!value, value); }
	}

	public void Initialize(uint spawnPointNetID)
	{
		InitializeRpc(spawnPointNetID);
	}

	[ClientRpc]
	public void InitializeRpc(uint spawnPointNetID)
	{
		if (isLocalPlayer)
		{
			foreach (NetworkIdentity i in FindObjectsOfType<NetworkIdentity>())
			{
				if (i.netId == spawnPointNetID)
				{
					SetPieSlice(transform, i.transform);
				}
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
			cam.LookAt(pieSlice.transform.forward);
			cam.localEulerAngles = new Vector3(0, cam.localEulerAngles.y - 22.5f, 0);

			SetInitialized(false, true);
		}
	}

	public void ApplyDamage (float damage)
	{
		health -= damage;
		if (health <= 0)
		{
			BaseDeath();
		}
	}

	[Command]
	void BaseDeath ()
	{
		NetworkServer.Destroy(gameObject);
	}

	void SetPieSlice(Transform oldValue, Transform newValue)
	{
		pieSlice = newValue;
		transform.parent = pieSlice;
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

	private void Update()
	{
        //if (isLocalPlayer)
		//	debugText.text = "Initialized = " + initialized;

		if (!isLocalPlayer || !initialized)
			return;

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
			//debugText.text = workerVec.ToString() + " | " + pieSlice.forward;

			if (Vector3.SignedAngle(workerVec.normalized, pieSlice.forward, Vector3.forward) > 1 && Vector3.SignedAngle(workerVec.normalized, pieSlice.forward, Vector3.forward) < 44)
			{
				Visible = true;
			}
			else
			{
				Visible = false;
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