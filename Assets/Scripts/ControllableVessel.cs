using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ControllableVessel : MonoBehaviour
{
    public float thrust = 10f;
    public float rcsThrust = 0.1f;
    public float explosiveForce = 10f;
    public float explosiveRadius = 10f;
    public float detonationSpeed = 1f;

	private new Rigidbody rigidbody;

	private void Start()
	{
		rigidbody = GetComponent<Rigidbody>();
	}

	void Update()
	{
		rigidbody.AddForce(transform.forward * thrust * Input.GetAxis("Vertical"));
		rigidbody.AddTorque(transform.up * rcsThrust * Input.GetAxis("Horizontal"));
	}

	private void OnCollisionEnter ()
	{
		if (rigidbody == null)
			rigidbody = GetComponent<Rigidbody>();
		if (rigidbody.velocity.magnitude >= detonationSpeed)
		{
			GameObject.FindWithTag("Terrain").GetComponent<Generator>().Explode(GameObject.FindWithTag("Terrain").transform.InverseTransformPoint(transform.position), explosiveForce, explosiveRadius);
			Destroy(gameObject);
		}
	}
}
