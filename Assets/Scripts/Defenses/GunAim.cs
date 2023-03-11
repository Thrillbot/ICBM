using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunAim : MonoBehaviour {

	public GameObject crossHair;
	public ParticleSystem bullets;
	public float speed;

	void Update() {
		Vector3 lookAt = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		transform.up = Vector2.Lerp(transform.up, (lookAt - transform.position), speed * 0.0001f);
		//crossHair.transform.localPosition = new Vector2(0,Vector2.Distance(lookAt,transform.position));
		
		if (Input.GetMouseButtonDown(0)) {
			bullets.Play();
		}
		if (Input.GetMouseButtonUp(0)) {
			bullets.Stop();
		}
	}
}
