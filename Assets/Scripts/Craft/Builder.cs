using Mirror;
using Rewired;
using Rewired.Demos;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Builder : NetworkBehaviour
{
	public GameObject launchButton;
	public bool active;
	public BoxCollider buildVolume;
	public Transform launchPad;
	public LayerMask uiLayers;

	public Material ghostMaterial;
	public GameObject[] parts;

	private List<GameObject> craft;
	private int partIndex = 0;
	private GameObject ghostPart;
	private List<Material> selectedPartMaterials;
	private Ray ray;
	private RaycastHit hit;
	private bool hitHappened;
	private Vector3 workerVec;

	private bool craftHead;

	public int playerId = 0;
	private Player player;

	void Awake()
	{
		if (!isLocalPlayer)
			return;

		launchButton = GameObject.FindWithTag("LaunchButton");
		launchButton.GetComponent<Image>().color = Color.red;
		launchButton.GetComponentInChildren<TMP_Text>().color = Color.white;
		launchButton.GetComponent<Button>().onClick.AddListener(() => Launch());
		launchButton.SetActive(false);
		SpawnGhostPart();

		// Get the Rewired Player object for this player and keep it for the duration of the character's lifetime
		player = ReInput.players.GetPlayer(playerId);
	}

	void Update()
	{
		if (!isLocalPlayer)
			return;

		if (player == null)
		{
			player = ReInput.players.GetPlayer(playerId);
		}

		if (player == null)
			return;

		if (launchButton == null)
		{
			launchButton = GameObject.FindWithTag("LaunchButton");
			launchButton.GetComponent<Image>().color = Color.red;
			launchButton.GetComponentInChildren<TMP_Text>().color = Color.white;
			launchButton.GetComponent<Button>().onClick.AddListener(() => Launch());
			launchButton.SetActive(false);
			SpawnGhostPart();
		}

		if (launchButton == null || launchButton.GetComponent<MouseOverUI>().MousedOver)
			return;

		ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		hitHappened = Physics.Raycast(ray.origin, ray.direction, out hit, 1000);
		if (hitHappened && hit.collider.gameObject.tag == "BuildVolume")
		{
			if (!active && player.GetButtonUp("Interact"))
			{
				active = true;
				launchButton.SetActive(true);
				return;
			}
		}
		else if (!hitHappened || hit.collider.gameObject.tag == "CurrentlyBuilt")
		{
			if (active && player.GetButtonUp("Interact"))
			{
				active = false;
				launchButton.SetActive(false);
				return;
			}
		}

		if (!active)
			return;

		if (player.GetButtonUp("NextPart"))
		{
			partIndex++;
			partIndex %= parts.Length;
			SpawnGhostPart();
		}

		ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		if (Physics.Raycast(ray.origin, ray.direction, out hit, 1000))
		{
			if (craftHead == false && hit.collider.gameObject.tag == "BuildVolume" && partIndex == 0)
			{
				ghostPart.SetActive(true);
				workerVec = hit.point;
				workerVec.z = 0;
				ghostPart.transform.position = workerVec;
				ghostPart.transform.LookAt(ghostPart.transform.position * 2f);

				if (player.GetButtonUp("Interact"))
				{
					craftHead = true;
					CmdSpawnPart(netId, partIndex, ghostPart.transform.position, ghostPart.transform.rotation, netIdentity.connectionToClient);
				}
			}
			else if (craftHead && hit.collider.name.Contains("attachmentPoint"))
			{
				ghostPart.SetActive(true);
				workerVec = launchPad.InverseTransformPoint(hit.point);
				workerVec.z = 0;
				workerVec = hit.collider.transform.parent.parent.localPosition - FindPartOffset(hit.collider.transform.parent.parent.gameObject, ghostPart, workerVec - hit.collider.transform.parent.parent.localPosition);
				workerVec.z = 0;
				ghostPart.transform.localPosition = workerVec;
				ghostPart.transform.LookAt(ghostPart.transform.position * 2f);

				if (player.GetButtonUp("Interact"))
				{
					CmdSpawnPart(netId, partIndex, ghostPart.transform.position, ghostPart.transform.rotation, netIdentity.connectionToClient);
				}
			}
			else
			{
				ghostPart.SetActive(false);
			}
		}
		else
		{
			ghostPart.SetActive(false);
		}
	}

	Vector3 FindPartOffset(GameObject targetedPart, GameObject ghostPart, Vector3 direction)
	{
		Vector3 offset = Vector3.zero;
		if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
		{
			offset = Vector3.right * targetedPart.GetComponent<BoxCollider>().bounds.size.x * 0.5f + Vector3.right * ghostPart.GetComponent<BoxCollider>().bounds.size.x * 0.5f;
			offset *= Mathf.Sign(direction.x);
		}
		else
		{
			offset = Vector3.up * targetedPart.GetComponent<BoxCollider>().bounds.size.y * Mathf.Sign(direction.y) * 0.5f + Vector3.right * ghostPart.GetComponent<BoxCollider>().bounds.size.y * Mathf.Sign(direction.y) * 0.5f;
		}

		return offset;
	}

	[Command]
	void CmdSpawnPart(uint padID, int index, Vector3 pos, Quaternion rot, NetworkConnectionToClient conn = null)
	{
		if (conn == null)
			return;
		GameObject newPart = Instantiate(parts[index], pos, rot);
		NetworkServer.Spawn(newPart, conn);

		ParentPart(padID, newPart);
	}

	[ClientRpc]
	void ParentPart (uint padID, GameObject newPart)
	{
		foreach (NetworkIdentity n in FindObjectsOfType<NetworkIdentity>())
		{
			if (n.netId == padID)
				newPart.transform.parent = n.transform;
		}
	}

	void SpawnGhostPart ()
	{
		if (ghostPart != null)
			Destroy(ghostPart);
		ghostPart = Instantiate(parts[partIndex]);
		ghostPart.transform.parent = launchPad;

		selectedPartMaterials ??= new();
		selectedPartMaterials.Clear();
		for (int i = 0; i < ghostPart.GetComponentsInChildren<Renderer>().Length; i++)
		{
			selectedPartMaterials.Add(ghostPart.GetComponentsInChildren<Renderer>()[i].material);
			ghostPart.GetComponentsInChildren<Renderer>()[i].material = ghostMaterial;
		}
	}

	public void Launch ()
	{
		Debug.Log("LAUNCH DETECTED!");
	}

	public void DeleteCraft ()
	{
		foreach (GameObject g in craft)
		{
			if (g != null)
			{
				Destroy(g);
			}
		}
		craft.Clear();
	}
}
