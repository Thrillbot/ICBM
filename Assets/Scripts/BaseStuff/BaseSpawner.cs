using Mirror;
using Rewired;
using System.Collections;
using TMPro;
using UnityEngine;
using static Universe;

public class BaseSpawner : NetworkBehaviour
{

	public GameObject planet;
	public Transform debugPoint;
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
				debugText = FindObjectOfType<TMP_Text>();
			}

			Transform cam = FindObjectOfType<FreeCam>().transform;
			workerVec = cam.localEulerAngles;
			workerVec.z = pieSlice.transform.localEulerAngles.y;
			cam.localEulerAngles = workerVec;

			SetInitialized(false, true);
		}
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
        if (isLocalPlayer)
			debugText.text = "Initialized = " + initialized;

		if (!isLocalPlayer || !initialized)
			return;

		if (isLocalPlayer && !placed)
		{
			workerVec = GetMousePointOnPlanet(GetMousePointOnPlane());
			debugPoint.position = workerVec;

			transform.position = workerVec;
			transform.LookAt(transform.position * 2f);

			debugText.text = workerVec.ToString() + " | " + pieSlice.forward;

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