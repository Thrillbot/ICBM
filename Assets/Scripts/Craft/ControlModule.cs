using System.Collections;
using UnityEngine;

public class ControlModule : Part
{
	public Transform launchPad;
	public float torque = 10f;

	private Transform parent;
	private Vector3 localPosition;

	public override void Start()
	{

	}

	IEnumerator Initialize ()
	{
		yield return null;
		parent = transform.parent;
		localPosition = transform.localPosition;
	}

	void Update ()
	{
		GetComponent<Rigidbody>().AddTorque(transform.right * player.GetAxis("Horizontal") * torque);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();

		if (!isArmed)
		{
			// Use the parent and localPosition to lock the left/right/down movement while not armed.  Only up!
		}
	}
}
