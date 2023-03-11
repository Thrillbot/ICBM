using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SAMAim : MonoBehaviour {
	
	public ParticleSystem plume;
	public float turnSpeed;
	public float thrust;
	public float fuel;
	public float remainAfterGas;
	
	private Rigidbody rb;
	private int dethklok;
	
	void Start () {
		rb = transform.GetComponent<Rigidbody>();
	}

	void Update() {
		if (fuel > 0) {
			Vector3 lookAt = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			transform.up = Vector2.Lerp(transform.up, (lookAt - transform.position), turnSpeed * 0.0001f);
			
			rb.AddForce(transform.up * thrust);
			fuel--;
		} else {
			OuttaGas();
			if (plume.isPlaying)
				plume.Stop();
		}
	}
	
	void OuttaGas () {
		if (dethklok > remainAfterGas) {
			Destroy(this.gameObject);
		} else {
			dethklok++;
		}
	}
}
