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
		//Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		Vector3 ray = Input.mousePosition;
		ray.z = -10;
		ray = Camera.main.ScreenToWorldPoint(ray);
		print(ray);

		if (Physics.Linecast(ray, planet.transform.position, out hit)) {
			transform.position = hit.point;
			transform.LookAt(ray);
			Debug.DrawLine(ray, planet.transform.position, Color.green, 10.0f);
		} 
	}
}
