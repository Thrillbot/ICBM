using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Builder : MonoBehaviour {

	public Camera cam;
	public GameObject[] parts;
    public GameObject gizmoPrefab;

    private Transform selPart;
	private GameObject curPart;
	private GameObject closest;
	private float closestDis;
	private bool canPlace;
	private bool oldPart;
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
		curPart.transform.position = new Vector3(10000, 10000, 10000);

		if (buildMode == 0 || buildMode == 2) 
		{
            curPart.transform.position = cam.ScreenToWorldPoint(mousePos);
            if (Physics.Raycast(ray, out hit, 1000, layer_mask))
			{
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
			if (selPart != null && selPart.parent != null && selPart.parent != selPart.root)
			{
				Transform newSel = selPart.parent.parent;
				GhostChildren();
				Destroy(selPart.gameObject);
                MoveGizmoToSelection(newSel);
			}
		}

		if (Input.GetAxis("Mouse ScrollWheel") > 0f)
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
		else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
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
	}

	void PlacePart()
    {
		GameObject newPart = Instantiate(curPart, new Vector3(0, 0, 0), Quaternion.identity);
		newPart.transform.GetChild(0).GetComponent<Renderer>().material.color = new Color(0f, 0f, 0f, 1f);
		newPart.transform.SetParent(closest.transform);
		newPart.transform.localPosition = new Vector3(0, 0, 0);
		newPart.transform.localEulerAngles = new Vector3(0, 0, 0);
		newPart.transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("Parts"); ;
		if (oldPart)
		{
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
				oldPart = true;
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

	void GhostChildren()
	{
        CraftPart craftPart = selPart.GetComponent<CraftPart>();
        foreach (GameObject mount in craftPart.mounts)
        {
			if (mount.transform.childCount == 0)
				break;
			mount.transform.GetChild(0).SetParent(null);
		}
	}
}
