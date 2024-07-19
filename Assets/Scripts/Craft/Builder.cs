using Mirror;
using Rewired;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Builder : NetworkBehaviour
{
	public TMP_Text feedbackText;
	public GameObject launchButton;
	public bool active;
	public BoxCollider buildVolume;
	public Transform launchPad;
	public LayerMask uiLayers;
	public GameObject tubeMask;

	public Material ghostMaterial;
	public GameObject craftHeadPrefab;
	public GameObject[] parts;
	public GameObject[] explosions;

	public Collider currentHitCollider;

	public bool debugMode;

	public GameObject craftHead;
	private List<GameObject> craft;  // To Do: Make this list a thing
	private int partIndex = 0;
	public GameObject ghostPart;
	private Ray ray;
	private RaycastHit[] hits;
	private bool hitHappened;
	private FreeCam cam;

	private int playerId = 0;
	private Player player;

	void Awake()
	{
		if (!isLocalPlayer)
			return;

		cam = GameObject.FindWithTag("FreeCam").GetComponent<FreeCam>();
		launchButton = GameObject.FindWithTag("LaunchButton");
		if (launchButton != null)
		{
			launchButton.GetComponent<Image>().color = Color.red;
			launchButton.GetComponentInChildren<TMP_Text>().color = Color.white;
			launchButton.GetComponent<Button>().onClick.AddListener(() => Launch());
			launchButton.SetActive(false);
		}
		SpawnGhostPart();

		// Get the Rewired Player object for this player and keep it for the duration of the character's lifetime
		player = ReInput.players.GetPlayer(playerId);
	}

	void LateUpdate()
	{
		if (!isLocalPlayer)
		{
			Debug.Log("Not Local Player");
			return;
		}

		if (player == null)
		{
			player = ReInput.players.GetPlayer(playerId);
		}

		if (player == null)
		{
			Debug.Log("No Rewired Player Found");
			return;
		}

        if (cam == null)
		{
			cam = GameObject.FindWithTag("FreeCam").GetComponent<FreeCam>();
		}

		if (launchButton == null)
		{
			launchButton = GameObject.FindWithTag("LaunchButton");
			if (launchButton != null)
			{
				launchButton.GetComponent<Image>().color = Color.red;
				launchButton.GetComponentInChildren<TMP_Text>().color = Color.white;
				launchButton.GetComponent<Button>().onClick.AddListener(() => Launch());
				launchButton.SetActive(false);
			}
			if (!ghostPart)
				SpawnGhostPart();
		}

		if (!debugMode)
		{
			if (launchButton == null || launchButton.GetComponent<MouseOverUI>().MousedOver)
				return;
		}

		ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		hits = Physics.RaycastAll(ray.origin, ray.direction, 1000);
		hitHappened = false;
		if (!ghostPart)
			SpawnGhostPart();
		ghostPart.SetActive(false);
		if (hits.Length > 0)
		{
			foreach (RaycastHit h in hits)
			{
				if (h.collider.gameObject.tag == "BuildVolume" || h.collider.gameObject.tag == "BeingBuilt")
				{
					if (!active && player.GetButtonUp("Interact"))
					{
						active = true;
						tubeMask.SetActive(true);
						cam.target = tubeMask.transform;
						cam.BuildMode = true;
						if (launchButton != null)
							launchButton.SetActive(true);

						if (craftHead == null)
						{
							CmdSpawnPart(-1, netIdentity);
							/*
							craftHead = Instantiate(craftHeadPrefab);
							NetworkServer.Spawn(craftHead, connectionToClient);
							//craftHead = Instantiate(craftHeadPrefab);

							craftHead.transform.parent = transform;
							craftHead.transform.localEulerAngles = Vector3.zero;
							craftHead.transform.localScale = Vector3.one * 10f;
							craftHead.transform.localPosition = new Vector3(0, 0, -1.5f);
							craftHead.GetComponent<ControlModule>().launchPad = launchPad;
							*/
						}

						return;
					}
					else if (!active)
						return;

					hitHappened = true;

					if (h.collider.gameObject.tag == "BeingBuilt")
					{
						ghostPart.SetActive(true);

						//ghostPart.transform.position = h.transform.TransformPoint(h.transform.localPosition) - FindPartOffset(h.transform.localPosition, ghostPart.GetComponent<Part>());
						//ghostPart.transform.position = h.transform.parent.position + h.transform.localPosition.normalized;
						ghostPart.transform.parent = h.collider.transform.parent;
						ghostPart.transform.localPosition = FindPartOffset(h.transform.localPosition, ghostPart.GetComponent<Part>(), h.transform);

						ghostPart.transform.localPosition = h.collider.transform.localPosition * 2f;

						ghostPart.transform.LookAt(ghostPart.transform.position + launchPad.up);

						if (player.GetButtonUp("Interact"))
						{
							currentHitCollider = h.collider;
							/*
							GameObject newPart = Instantiate(parts[partIndex]);

							newPart.transform.parent = h.collider.transform.parent;
							newPart.transform.SetAsFirstSibling();
							newPart.transform.localPosition = ghostPart.transform.localPosition;
							newPart.transform.localRotation = ghostPart.transform.localRotation;


							foreach (SphereCollider s in newPart.transform.GetComponentsInChildren<SphereCollider>())
							{
								if (Vector3.Dot(s.transform.localPosition.normalized, h.collider.transform.parent.position - s.transform.position) > 0.8f)
								{
									s.enabled = false;
									break;
								}
							}

							newPart.GetComponent<Part>().attachedCollider = h.collider;
							*/
							CmdSpawnPart(partIndex, netIdentity);//, ghostPart.transform.localPosition, ghostPart.transform.localRotation, h.collider.GetComponent<NetworkIdentity>());
							h.collider.enabled = false;
						}
					}
				}
				else if (h.collider.gameObject != craftHead && h.collider.gameObject.tag == "BeingBuild" && player.GetButtonUp("Interact"))
				{
					ghostPart.SetActive(false);
					ghostPart.transform.parent = launchPad;

					h.collider.gameObject.GetComponent<Part>().attachedCollider.enabled = true;

					CmdDestroyPart(h.collider.gameObject.GetComponent<NetworkIdentity>());
				}
			}
		}
		if (!hitHappened)
		{
			if (active && player.GetButtonUp("Interact"))
			{
				active = false;
				feedbackText.transform.parent.gameObject.SetActive(false);
				tubeMask.SetActive(false);
				cam.target = null;
				cam.BuildMode = false;
				if (launchButton != null)
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

		if (player.GetButtonUp("Launch"))
		{
			Launch();
		}
	}

	public void InitializePart (GameObject newPart)
	{
		try
		{
			if (newPart.GetComponent<ControlModule>())
			{
				newPart.transform.parent = transform;
				newPart.transform.localEulerAngles = Vector3.zero;
				newPart.transform.localScale = Vector3.one * 10f;
				newPart.transform.localPosition = new Vector3(0, 0, -1.5f);
				newPart.GetComponent<ControlModule>().launchPad = launchPad;
				craftHead = newPart;
			}
			else
			{
				newPart.transform.parent = currentHitCollider.transform.parent;
				newPart.transform.SetAsFirstSibling();
				newPart.transform.localPosition = ghostPart.transform.localPosition;
				newPart.transform.localRotation = ghostPart.transform.localRotation;

				foreach (SphereCollider s in newPart.transform.GetComponentsInChildren<SphereCollider>())
				{
					if (Vector3.Dot(s.transform.localPosition.normalized, currentHitCollider.transform.parent.position - s.transform.position) > 0.8f)
					{
						s.enabled = false;
						break;
					}
				}

				newPart.GetComponent<Part>().attachedCollider = currentHitCollider.GetComponent<Collider>();
			}
		}
		catch (Exception e)
		{
			Debug.LogError(e);
		}
	}

	[Command(requiresAuthority = false)]
	public void DestroyPart(GameObject part, bool armed, Vector3 pos, float explosionRadius, float explosionDamage, int explosionIndex)
	{
		if (part != null)
		{
			if (armed)
			{
				GameObject explosionGO = Instantiate(explosions[explosionIndex], pos, Quaternion.identity);
				NetworkServer.Spawn(explosionGO);
			}

			/*
			foreach (BaseSpawner b in FindObjectsOfType<BaseSpawner>())
			{
				if ((b.transform.position - transform.position).sqrMagnitude <= explosionRadius * explosionRadius)
				{
					b.RpcApplyDamage(explosionDamage);
				}
			}
			*/

			NetworkServer.Destroy(part.gameObject);
		}
	}

	[Command(requiresAuthority = false)]
	void CmdSpawnPart (int index, NetworkIdentity identity)//, Vector3 pos, Quaternion rot, NetworkIdentity col)
	{
		GameObject newPart;
		if (index == -1)
			newPart = Instantiate(craftHeadPrefab);
		else
			newPart = Instantiate(parts[index]);

		NetworkServer.Spawn(newPart, identity.connectionToClient);

		newPart.GetComponent<NetworkIdentity>().AssignClientAuthority(identity.connectionToClient);
	}

	[Command(requiresAuthority = false)]
	void CmdDestroyPart (NetworkIdentity netID)
	{
		NetworkServer.Destroy(netID.gameObject);
	}

	Vector3 FindPartOffset(Vector3 direction, Part part, Transform hit)
	{
		direction = direction.normalized;
		Vector3 offset;
		if (Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
		{
			offset = hit.parent.forward * part.dimensions.x * 0.5f * Mathf.Sign(direction.x);
		}
		else
		{
			offset = hit.parent.up * part.dimensions.y * 0.5f * Mathf.Sign(direction.z);
		}
		return offset * part.transform.localScale.x;
	}

	void SpawnGhostPart ()
	{
		if (ghostPart != null)
			Destroy(ghostPart);
		ghostPart = Instantiate(parts[partIndex]);
		Destroy(ghostPart.GetComponent<NetworkIdentity>());
		ghostPart.transform.parent = launchPad;

		for (int i = 0; i < ghostPart.GetComponentsInChildren<Renderer>().Length; i++)
		{
			if (!ghostPart.GetComponentsInChildren<Renderer>()[i].name.Contains("RenderVolume")) {
				ghostPart.GetComponentsInChildren<Renderer>()[i].material = ghostMaterial;
			}
		}
		for (int i = 0; i < ghostPart.GetComponentsInChildren<Collider>().Length; i++)
		{
			Destroy(ghostPart.GetComponentsInChildren<Collider>()[i]);
		}
	}

	public void Launch ()
	{
		feedbackText.text = "";
		string feedback = "";
		bool hasFuel = craftHead.transform.GetComponentsInChildren<FuelTank>().Length != 0;
		bool hasThrust = craftHead.transform.GetComponentsInChildren<Thruster>().Length != 0;

		if (!hasFuel)
		{
			feedback += "Craft has no fuel!\n";
		}
		if (!hasThrust)
		{
			feedback += "Craft has no thrusters!\n";
		}

        if (feedback != "")
		{
			feedbackText.transform.parent.gameObject.SetActive(true);
			feedbackText.text = feedback;
			return;
		}

		craftHead.transform.parent = null;
		craftHead.GetComponent<Rigidbody>().isKinematic = false;

		ghostPart.transform.parent = null;
		ghostPart.SetActive(false);

		cam.target = craftHead.transform;
		active = false;
		feedbackText.transform.parent.gameObject.SetActive(false);
		tubeMask.SetActive(false);
		cam.BuildMode = false;
		if (launchButton != null)
			launchButton.SetActive(false);
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
