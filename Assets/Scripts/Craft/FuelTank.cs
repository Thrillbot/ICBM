using UnityEngine;

public class FuelTank : Part
{
	[Header("Fuel Tank")]
	public float fuel;

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		_mass = mass + fuel * 0.1f;
	}
}
