using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SAMControl : MonoBehaviour {

	public GameObject missilePrefab;
	public ParticleSystem launch;

	void Update() {
		if (Input.GetMouseButtonDown(0)) {
			Instantiate(missilePrefab, transform.position + new Vector3(0, 5, 0), Quaternion.AngleAxis(90, Vector3.up));
			launch.Play();
		} 
	}
}
