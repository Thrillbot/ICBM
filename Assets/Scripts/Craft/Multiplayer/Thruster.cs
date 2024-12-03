using UnityEngine;
using Mirror;

public class Thruster : Part
{
	[Header("Thruster")]
	public float thrust;
	public float fuelRate = 5;
	public ParticleSystem smoke;
	public AudioSource thrusterAudio;
	public AnimationCurve specificImpulse;

	[SyncVar]
	public bool fuelCheck;
	FuelTank[] tanks;

	[SyncVar(hook = "SetThrottle")]
	public float throttle;

	public override void FixedUpdate()
	{
		if (dead) return;

		base.FixedUpdate();

		if (fuelCheck)
		{
			ParticleSystem.MainModule temp = smoke.main;
			temp.startSizeMultiplier = throttle * 40f;
			thrusterAudio.volume = throttle;

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

		if (!hasAuthority) return;

		if (!transform.root.GetComponent<Rigidbody>() || transform.root.GetComponent<Rigidbody>().isKinematic) return;
		
		throttle = builder.Throttle;

		if (dead || gameObject == null || smoke == null || thrusterAudio == null) return;
		CommandSetThrottle(throttle, fuelCheck);

		if (fuelCheck)
		{
			airPressure = (Universe.KarmanLine - (transform.position.magnitude - Universe.GetPointOnPlanet(transform.position).magnitude)) / Universe.KarmanLine; // From seaLevel to 50f above seaLevel (Defining line of "space" for this game)
			airPressure = Mathf.Clamp(airPressure, 0.0f, 1.0f); // Clamp that so we can do a force

			transform.root.GetComponent<Rigidbody>().AddForceAtPosition(transform.forward * thrust * specificImpulse.Evaluate(airPressure) * throttle, transform.position);
		}
		else
		{

		}
	}

	private void Update()
	{
		if (!hasAuthority || dead) return;

		if (!isArmed)
		{
			fuelCheck = true;
			return;
		}

		tanks = transform.root.GetComponentsInChildren<FuelTank>();
		fuelCheck = false;
		foreach (FuelTank f in tanks)
		{
			f.fuel -= fuelRate * throttle * Time.deltaTime / tanks.Length;
			f.fuel = Mathf.Max(f.fuel, 0);

			if (f.fuel > 0)
				fuelCheck = true;
		}
	}

	[Command(requiresAuthority = false)]
	public void CommandSetThrottle (float throttleSet, bool fuelCheckSet)
	{
		if (dead || gameObject == null || smoke == null || thrusterAudio == null) return;
		throttle = throttleSet;
		fuelCheck = fuelCheckSet;
		SetThrottle(-1, throttleSet);
	}

	[ClientRpc]
	public void SetThrottle (float oldValue, float newValue)
	{
		if (dead || gameObject == null || smoke == null || thrusterAudio == null) return;

		if (thrusterAudio.enabled == false) return;

		if (fuelCheck)
		{
			ParticleSystem.MainModule temp = smoke.main;
			temp.startSizeMultiplier = throttle * 40f;
			thrusterAudio.volume = throttle;

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
}
