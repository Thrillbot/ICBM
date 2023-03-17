using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Builder : MonoBehaviour
{

	public Camera cam;
	public GameObject[] parts;
	public GameObject gizmoPrefab;

	private Transform selPart;
	private GameObject curPart;
	private GameObject closest;
	private float closestDis;
	private bool canPlace;
	private bool ghostPartSelected;
	private bool ghostPartHit;
	private int buildMode;
	private int partIndex;

	private Ray ray;
	private RaycastHit hit;

	int layer_mask;

	void Start()
	{
		gizmoPrefab = Instantiate(gizmoPrefab, new Vector3(0, 0, 0), Quaternion.identity);
		gizmoPrefab.transform.position = new Vector3(10000, 10000, 10000);
		partIndex = 1;
		ChangePart();
		layer_mask = LayerMask.GetMask("Parts");
	}

	void Update()
	{
		ray = cam.ScreenPointToRay(Input.mousePosition);
		Vector3 mousePos = Input.mousePosition;
		mousePos.z = cam.nearClipPlane + 15;

		//RESET
		closest = null;
		canPlace = false;
        ghostPartHit = false;
        curPart.transform.position = new Vector3(10000, 10000, 10000);

		if (buildMode == 0 || buildMode == 2)
		{
			curPart.transform.position = cam.ScreenToWorldPoint(mousePos);
			if (Physics.Raycast(ray, out hit, 1000, layer_mask))
			{
				if (hit.transform.parent.GetComponent<CraftPart>().notAttached)
					ghostPartHit = true;
                DistanceChecker(hit);
				if (closest != null)
				{
					curPart.transform.position = closest.transform.position;
					curPart.transform.localEulerAngles = closest.transform.eulerAngles;
				}
			}
		}

		//change this shit to the input system///////////////////////////////////////////////////////////////////////////////////
		switch (Input.inputString)
		{
			case "1":
				buildMode = 0;
				break;

			case "2":
				buildMode = 1;
				break;

			case "3":
				buildMode = 2;
				break;
		}

		if (Input.GetMouseButtonDown(0))
		{
			Clicker();
		}
		if (Input.GetKeyDown("delete"))
		{
			if (selPart != null && selPart.parent != null && selPart.parent != selPart.root && !ghostPartSelected)
			{
				Transform newSel = selPart.parent.parent;
				GhostChildren(selPart, true);
				Destroy(selPart.gameObject);
				MoveGizmoToSelection(newSel);
			}
			if (ghostPartSelected)
				ChangePart();
		}

		if (Input.GetAxis("Mouse ScrollWheel") > 0f && !ghostPartSelected)
		{
			if (partIndex < parts.Length - 1)
			{
				partIndex = partIndex + 1;
			}
			else
			{
				partIndex = 0;
			}
			ChangePart();
		}
		else if (Input.GetAxis("Mouse ScrollWheel") < 0f && !ghostPartSelected)
		{
			if (partIndex > 0)
			{
				partIndex = partIndex - 1;
			}
			else
			{
				partIndex = parts.Length - 1;
			}
			ChangePart();
		}
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	}

	void DistanceChecker(RaycastHit hit)
	{
		CraftPart craftPart = hit.transform.parent.GetComponent<CraftPart>();
		foreach (GameObject mount in craftPart.mounts)
		{
			float testDis = Vector3.Distance(hit.point, mount.transform.position);
			if (closest == null && mount.transform.childCount == 0)
			{
				closest = mount;
				closestDis = testDis;
				canPlace = true;
				continue;
			}

			if (testDis < closestDis && mount.transform.childCount == 0)
			{
				closest = mount;
				closestDis = testDis;
				canPlace = true;
			}
		}
	}

	void Clicker()
	{
		if (ghostPartSelected && closest == null)
		{
			curPart = null;
			ChangePart();
			return;
		}
		switch (buildMode)
		{
			case 0: //Block Placer
				if (canPlace)
					PlacePart();
				break;

			case 1: //Block Selector
				SelectPart();
				break;

			case 2:
				break;
		}
	}

	void ChangePart()
	{
		if (curPart != null)
			Destroy(curPart);
		curPart = Instantiate(parts[partIndex], new Vector3(0, 0, 0), Quaternion.identity);
		curPart.transform.GetChild(0).GetComponent<Renderer>().material.color = new Color(0f, 0f, 0f, 0.3f);
        ghostPartSelected = false;
	}

	void PlacePart()
	{
		GameObject newPart = Instantiate(curPart, new Vector3(0, 0, 0), Quaternion.identity);
		if (!ghostPartHit)
		{
			newPart.transform.GetChild(0).GetComponent<Renderer>().material.color = new Color(0f, 0f, 0f, 1f);
		} else {
			newPart.transform.GetComponent<CraftPart>().notAttached = true;
		}
        newPart.transform.SetParent(closest.transform);
		newPart.transform.localPosition = new Vector3(0, 0, 0);
		newPart.transform.localEulerAngles = new Vector3(0, 0, 0);
		newPart.transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("Parts"); ;
		if (ghostPartSelected)
		{
			UnGhostinate(newPart.transform);
			ChangePart();
		}
		MoveGizmoToSelection(newPart.transform);
	}

	void SelectPart()
	{
		if (Physics.Raycast(ray, out hit, 1000, layer_mask))
		{
			MoveGizmoToSelection(hit.transform.parent);
			if (hit.transform.parent.parent == null)
			{
				buildMode = 0;
                ghostPartSelected = true;
				gizmoPrefab.transform.position = new Vector3(10000, 10000, 10000);
				if (curPart != null)
					Destroy(curPart);
				curPart = hit.transform.parent.gameObject;
			}
		}
	}

	void MoveGizmoToSelection(Transform partSelection)
	{
		selPart = partSelection;
		gizmoPrefab.transform.position = selPart.position;
		gizmoPrefab.transform.rotation = selPart.rotation;
	}

	void GhostChildren(Transform part, bool unParent)
	{
		CraftPart craftPart = part.GetComponent<CraftPart>();
		part.GetChild(0).GetComponent<Renderer>().material.color = new Color(0f, 0f, 0f, 0.3f);
        craftPart.notAttached = true;
        foreach (GameObject mount in craftPart.mounts)
		{
			if (mount.transform.childCount == 0)
				break;
			Transform mounted = mount.transform.GetChild(0);
			if (unParent)
				mounted.SetParent(null);
			mounted.GetChild(0).GetComponent<Renderer>().material.color = new Color(0f, 0f, 0f, 0.3f);
			CraftPart mountPart = mounted.GetComponent<CraftPart>();
            mountPart.notAttached = true;
            foreach (GameObject subMount in mountPart.mounts)
			{
				if (subMount.transform.childCount == 0)
					break;
				GhostChildren(subMount.transform.GetChild(0).transform, false);
			}
		}
	}

	void UnGhostinate(Transform part)
	{
		CraftPart craftPart = part.GetComponent<CraftPart>();
		part.GetChild(0).GetComponent<Renderer>().material.color = new Color(0f, 0f, 0f, 1f);
        craftPart.notAttached = false;
        foreach (GameObject mount in craftPart.mounts)
		{
			if (mount.transform.childCount == 0)
				break;
			Transform mounted = mount.transform.GetChild(0);
			mounted.GetChild(0).GetComponent<Renderer>().material.color = new Color(0f, 0f, 0f, 1f);
			CraftPart mountPart = mounted.GetComponent<CraftPart>();
            mountPart.notAttached = false;
            foreach (GameObject subMount in mountPart.mounts)
			{
				if (subMount.transform.childCount == 0)
					break;
				UnGhostinate(subMount.transform.GetChild(0).transform);
			}
		}
	}
}
