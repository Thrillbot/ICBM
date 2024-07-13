using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Gravity : MonoBehaviour
{
	private new Rigidbody rigidbody;
	private void Start()
	{
		rigidbody = GetComponent<Rigidbody>();
	}

	void FixedUpdate()
    {
		rigidbody.AddForce(transform.position.normalized * Universe.Gravity);
	}
}
