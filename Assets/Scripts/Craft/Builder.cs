using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Builder : MonoBehaviour {

	public GameObject[] parts; 

	private GameObject selPart;
	private GameObject curPart;
	private GameObject closest;
	private float closestDis;
	private bool canPlace;
	private int buildMode;
	private int partIndex;

	private Ray ray;
	private RaycastHit hit;

	int layer_mask;

	void Start() 
	{
		partIndex = 1;
		ChangePart();
	}

	void Update() 
	{
		//RESET
		closest = null;
		canPlace = false;
		curPart.transform.position = new Vector3(10000, 10000, 10000);

		/*
		Vector3 mousePos = Input.mousePosition;
		mousePos.z = Camera.main.nearClipPlane;
		mousePos = Camera.main.ScreenToWorldPoint(mousePos);
		*/

		Vector3 mousePos = Input.mousePosition;
		mousePos.z = Camera.main.nearClipPlane; 
		ray = Camera.main.ScreenPointToRay(mousePos);

		if (buildMode == 0 || buildMode == 2)	
		{
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
			print(selPart.transform.root);
			if (selPart != null && selPart.transform.parent != null && selPart.transform.parent != selPart.transform.root)
			{
				GameObject newSel = selPart.transform.parent.parent.gameObject;
				Destroy(selPart);
				selPart = newSel;
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
		CraftPart craftPart = hit.transform.GetComponent<CraftPart>();
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

			case 2: //Block Mover
				break;
		}
	}

	void ChangePart()
    {
		if (curPart != null)
			Destroy(curPart);
		curPart = Instantiate(parts[partIndex], new Vector3(0, 0, 0), Quaternion.identity);
		curPart.transform.GetChild(0).GetComponent<Renderer>().material.color = new Color(0f, 0f, 0f, 0.3f);
		layer_mask = LayerMask.GetMask("Parts");
	}

	void PlacePart()
    {
		GameObject newPart = Instantiate(curPart, new Vector3(0, 0, 0), Quaternion.identity);
		newPart.transform.GetChild(0).GetComponent<Renderer>().material.color = new Color(0f, 0f, 0f, 1f);
		newPart.transform.SetParent(closest.transform);
		newPart.transform.localPosition = new Vector3(0, 0, 0);
		newPart.transform.localEulerAngles = new Vector3(0, 0, 0);
		newPart.transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("Parts"); ;

		selPart = newPart;
	}

	void SelectPart()
    {
		if (Physics.Raycast(ray, out hit, 1000, layer_mask))
        {
			print(hit.transform.name);
        }
	}
}
