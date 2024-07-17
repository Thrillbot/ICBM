using UnityEngine;

public class Thruster : Part
{
	[Header("Thruster")]
	public float thrust;
	public float fuelRate = 5;
	public ParticleSystem smoke;
	public AudioSource thrusterAudio;
	public AnimationCurve specificImpulse;

	bool fuelCheck;
	FuelTank[] tanks;

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (!transform.root.GetComponent<Rigidbody>() || transform.root.GetComponent<Rigidbody>().isKinematic) return;

		if (fuelCheck)
		{
			airPressure = (Universe.KarmanLine - (transform.position.magnitude - Universe.GetPointOnPlanet(transform.position).magnitude)) / Universe.KarmanLine; // From seaLevel to 50f above seaLevel (Defining line of "space" for this game)
			airPressure = Mathf.Clamp(airPressure, 0.0f, 1.0f); // Clamp that so we can do a force

			transform.root.GetComponent<Rigidbody>().AddForceAtPosition(transform.forward * thrust * specificImpulse.Evaluate(airPressure), transform.position);

			if (!smoke.isPlaying)
			{
				thrusterAudio.Play();
				thrusterAudio.time = Random.value * thrusterAudio.clip.length;
				smoke.Play();
			}
		}
		else
		{
			if (smoke.isPlaying)
			{
				thrusterAudio.Stop();
				smoke.Stop();
			}
		}
	}

	private void Update()
	{
		if (!isArmed)
		{
			fuelCheck = true;
			return;
		}

		tanks = transform.root.GetComponentsInChildren<FuelTank>();
		fuelCheck = false;
		foreach (FuelTank f in tanks)
		{
			f.fuel -= fuelRate * Time.deltaTime / tanks.Length;
			f.fuel = Mathf.Max(f.fuel, 0);

			if (f.fuel > 0)
				fuelCheck = true;
		}
	}
}
