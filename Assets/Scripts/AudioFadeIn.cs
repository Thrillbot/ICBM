using UnityEngine;
using UnityEngine.Audio;

public class AudioFadeIn : MonoBehaviour
{
	public AudioMixer mixer;
	public AnimationCurve fadeRamp;
	public float fadeTime;

	private float timer = 0;

	private void Update()
	{
		if (timer < 1)
		{
			timer += Time.deltaTime / fadeTime;

			mixer.SetFloat("megaMasterVolume", (1f - fadeRamp.Evaluate(timer)) * -80f);
		}
	}
}
