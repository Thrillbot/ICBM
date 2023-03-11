using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TestOrbit : MonoBehaviour
{
    public Vector3 initialVelocity;

	private new Rigidbody rigidbody;

	void Awake()
	{
		if (rigidbody == null)
			rigidbody = GetComponent<Rigidbody>();
		rigidbody.AddForce(initialVelocity);
	}

	void FixedUpdate()
    {
		if (Universe.paused || Universe.loading)
		{
			if (!Universe.loading)
				initialVelocity = rigidbody.velocity;
			rigidbody.isKinematic = true;
		}
		else
		{
			if (rigidbody.isKinematic)
			{
				rigidbody.isKinematic = false;
				rigidbody.AddForce(initialVelocity);
			}
			rigidbody.AddForce(transform.position.normalized * Universe.gravity);
		}
    }

	void OnDrawGizmos()
	{
		if (rigidbody == null)
			rigidbody = GetComponent<Rigidbody>();
		Gizmos.color = Color.white;
		Gizmos.DrawLine(transform.position, transform.position + (Application.isPlaying ? rigidbody.velocity : initialVelocity));
	}
}
