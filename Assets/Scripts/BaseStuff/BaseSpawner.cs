using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseSpawner : MonoBehaviour {
	
	public GameObject planet;
	
    void Update() {
		//if (!HasStateAuthority)
			//return;

		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

		if (Physics.Raycast(ray, out hit)) {
			RaycastHit _hit;
			if (Physics.Linecast(new Vector3(hit.point.x,0,hit.point.z), planet.transform.position, out _hit)) {
				transform.position = _hit.point;
				transform.LookAt(hit.transform.position);
			}
		}   
	}
}
