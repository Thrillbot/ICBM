using UnityEngine;

public class ControlModuleSinglePlayer : PartSinglePlayer
{
	public float torque = 10f;

	void Update ()
	{
		GetComponent<Rigidbody>().AddTorque(transform.right * player.GetAxis("Horizontal") * torque);
	}
}
