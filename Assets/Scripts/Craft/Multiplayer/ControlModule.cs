using UnityEngine;

public class ControlModule : Part
{
	public float torque = 10f;

	void Update ()
	{
		GetComponent<Rigidbody>().AddTorque(transform.right * player.GetAxis("Horizontal") * torque);
	}
}
