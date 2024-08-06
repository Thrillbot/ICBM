using Mirror;
using Rewired;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Builder : NetworkBehaviour
{
	public TMP_Text feedbackText;
	public bool active;
	public BoxCollider buildVolume;
	public Transform launchPad;
	public LayerMask uiLayers;
	public GameObject tubeMask;
	public GameObject launchButton;
	public GameObject flightControls;
	public Image throttleVisual;
	public Image fuelVisual;
	public GameObject controlButtons;

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

	public float throttle;
	private float throttleRampTime = 0.5f;
	private float fuelLevel = 0;
	private float maxFuel = 0;

	void Awake()
	{
		cam = GameObject.FindWithTag("FreeCam").GetComponent<FreeCam>();

		SpawnGhostPart();

		// Get the Rewired Player object for this player and keep it for the duration of the character's lifetime
		player = ReInput.players.GetPlayer(playerId);
	}

	void LateUpdate()
	{
		if (!isOwned)
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

		if (!ghostPart)
			SpawnGhostPart();

		if (craftHead == null)
		{
			flightControls.SetActive(false);
		}

		if (launchButton.activeInHierarchy && launchButton.GetComponent<MouseOverUI>().MousedOver)
			return;
		
		if (craftHead == null || !craftHead.GetComponent<Rigidbody>().isKinematic) {
			throttle += player.GetAxis("Vertical") * (Time.deltaTime / throttleRampTime);
			throttle = Mathf.Clamp01(throttle);
			throttleVisual.fillAmount = throttle;

			if (craftHead != null)
			{
				fuelLevel = 0;
				maxFuel = 0;
				foreach (FuelTank f in craftHead.GetComponentsInChildren<FuelTank>())
				{
					fuelLevel += f.fuel;
					maxFuel += f.maxFuel;
				}
				fuelVisual.fillAmount = fuelLevel / maxFuel;
			}
		}
		else
		{
			throttle = 0;
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
						launchButton.SetActive(true);
						active = true;
						tubeMask.SetActive(true);
						cam.target = tubeMask.transform;
						cam.BuildMode = true;
						flightControls.SetActive(false);

						if (craftHead == null)
						{
							CmdSpawnPart(-1, netIdentity);
						}

						return;
					}
					else if (!active)
						return;

					hitHappened = true;

					if (h.collider.gameObject.tag == "BeingBuilt")
					{
						ghostPart.SetActive(true);

						ghostPart.transform.parent = h.collider.transform.parent;
						ghostPart.transform.localPosition = h.collider.transform.localPosition + FindPartOffset(h.collider.transform.localPosition, ghostPart.GetComponent<Part>(), h.collider.transform);

						ghostPart.transform.localEulerAngles = Vector3.zero;

						if (player.GetButtonUp("Interact"))
						{
							currentHitCollider = h.collider;
							CmdSpawnPart(partIndex, netIdentity);
							h.collider.enabled = false;
						}
					}
				}
				else if (h.collider.gameObject != craftHead && h.collider.gameObject.tag == "BeingBuild" && player.GetButtonUp("Interact"))
				{
					ghostPart.SetActive(false);
					ghostPart.transform.parent = launchPad;

					h.collider.transform.parent.gameObject.GetComponent<Part>().attachedCollider.enabled = true;

					CmdDestroyPart(h.collider.transform.parent.gameObject.GetComponent<NetworkIdentity>());
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
				launchButton.SetActive(false);
				flightControls.SetActive(false);
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

			NetworkServer.Destroy(part);
		}
	}

	[Command(requiresAuthority = false)]
	public void Stage(GameObject part)
	{
		if (part != null)
			NetworkServer.Destroy(part.gameObject);
	}

	[Command(requiresAuthority = false)]
	void CmdSpawnPart (int index, NetworkIdentity identity)
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
		if (netID != null)
			NetworkServer.Destroy(netID.gameObject);
	}

	Vector3 FindPartOffset(Vector3 direction, Part part, Transform hit)
	{
		if (hit.parent.GetComponent<Part>() == null)
		{
			Debug.Log(hit.name);
			Debug.Log("Attachment Point doesn't belong to a part!");
			return Vector3.zero;
		}

		direction = direction.normalized;
		Vector3 offset;

		if (Mathf.Abs(direction.y) > Mathf.Abs(direction.z))
		{
			offset = Vector3.up * (part.dimensions.x / 2f) * Mathf.Sign(direction.y);
		}
		else
		{
			offset = Vector3.forward * (part.dimensions.y / 2f) * Mathf.Sign(direction.z);
		}
		Debug.Log("Offset: " + offset);
		return offset;
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
		throttle = 1;

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
		launchButton.SetActive(false);
		flightControls.SetActive(true);
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

	public float Throttle
	{
		get { return throttle; }
	}

	public override void OnStartAuthority()
	{
		base.OnStartAuthority();
		controlButtons.SetActive(true);
	}
}
