using Rewired;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Decoupler : Part
{
    public float decoupleForce = 5f;

	private bool decoupling;

	void Update()
    {
        if (!decoupling && isArmed && player.GetButtonUp("Launch"))
		{
			decoupling = true;
			StartCoroutine(Decouple());
		}
    }

	IEnumerator Decouple ()
	{
		Rigidbody childRigidbody = transform.GetChild(0).GetChild(0).GetComponent<Rigidbody>();
		transform.root.GetComponent<Rigidbody>().AddForce(transform.forward * decoupleForce);
		transform.GetChild(0).GetChild(0).transform.parent = null;
		childRigidbody.isKinematic = false;
		childRigidbody.GetComponent<Part>().rootRigidbody = childRigidbody;
		foreach (Part p in childRigidbody.GetComponentsInChildren<Part>())
		{
			p.rootRigidbody = childRigidbody;
		}

		yield return null;

		childRigidbody.velocity = transform.root.GetComponent<Rigidbody>().velocity;
		childRigidbody.angularVelocity = transform.root.GetComponent<Rigidbody>().angularVelocity;
		childRigidbody.AddForce(-transform.forward * decoupleForce);

		Destroy(gameObject);
	}
}
