using UnityEngine;

public class ControlModule : Part
{
	public Transform launchPad;
	public float torque = 10f;

	void Update ()
	{
		GetComponent<Rigidbody>().AddTorque(transform.right * player.GetAxis("Horizontal") * torque);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();

		if (!isArmed)
		{
			//transform.LookAt(transform.position + launchPad.position.normalized);
		}
	}
}
