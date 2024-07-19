using UnityEngine;

public class Explosion : MonoBehaviour
{
	public ParticleSystem explosion;
	public float maxIntensity = 5;
	public Light explosionLight;
	public AnimationCurve lightLevel;
	public float explosionRadius;
	public float explosionDamage;
	public AudioSource explosionSound;
	public AudioClip[] clips;

	private bool triggered;
	
	void Update()
	{
		if (!triggered)
		{
			triggered = true;
			explosionSound.clip = clips[Random.Range(0, clips.Length)];
			explosionSound.Play();
		}

		explosionLight.intensity = lightLevel.Evaluate(explosion.time / explosion.main.duration);
	}
}
