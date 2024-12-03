using UnityEngine;

public class FuelTankSinglePlayer : PartSinglePlayer
{
	[Header("Fuel Tank")]
	public float fuel;

	public float maxFuel;

	private void Awake()
	{
		maxFuel = fuel;
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		_mass = mass + fuel * 0.1f;
	}
}
