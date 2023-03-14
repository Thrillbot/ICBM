using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Builder : MonoBehaviour {

	public GameObject[] parts; 

	private GameObject curPart;
	private GameObject closest;
	private float closestDis;
	private bool canPlace;

	int layer_mask;

	void Start() {
		curPart = Instantiate(parts[1], new Vector3(0, 0, 0), Quaternion.identity);
		layer_mask = LayerMask.GetMask("Parts");
	}

	void Update() {
		/*
		Vector3 mousePos = Input.mousePosition;
		mousePos.z = Camera.main.nearClipPlane;
		mousePos = Camera.main.ScreenToWorldPoint(mousePos);

		curPart.transform.position = mousePos;
		*/

		RaycastHit hit;

		Vector3 mousePos = Input.mousePosition;
		mousePos.z = Camera.main.nearClipPlane; 
		Ray ray = Camera.main.ScreenPointToRay(mousePos);

		if (Physics.Raycast(ray, out hit, 1000, layer_mask)) {
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

			if (closest != null)
			{
				curPart.transform.position = closest.transform.position;
				curPart.transform.localEulerAngles = closest.transform.eulerAngles;
			}
		}

		if (Input.GetMouseButtonDown(0) && canPlace) {
			GameObject newPart = Instantiate(curPart, new Vector3(0, 0, 0), Quaternion.identity);
			newPart.transform.SetParent(closest.transform);
			newPart.transform.localPosition = new Vector3 (0,0,0);
			newPart.transform.localEulerAngles = new Vector3(0, 0, 0);
			newPart.transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("Parts"); ;
		}
		closest = null;
		canPlace = false;
	}
}
