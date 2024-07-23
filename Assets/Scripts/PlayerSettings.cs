using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class PlayerSettings : MonoBehaviour
{
	public AnimationCurve volumeCurve;

	public Slider masterVolume;
	public Slider musicVolume;
	public Slider effectsVolume;
	public AudioMixer audioMixer;

	public AudioSource effectAudioTester;

	void Start()
	{
		masterVolume.value = PlayerPrefs.GetFloat("MasterVolume", 1) * 10;
		musicVolume.value = PlayerPrefs.GetFloat("MusicVolume", 0.5f) * 10;
		effectsVolume.value = PlayerPrefs.GetFloat("EffectsVolume", 0.8f) * 10;

		audioMixer.SetFloat("masterVolume", Mathf.Lerp(-80, 0, volumeCurve.Evaluate(masterVolume.value / 10f)));
		audioMixer.SetFloat("musicVolume", Mathf.Lerp(-80, 0, volumeCurve.Evaluate(musicVolume.value / 10f)));
		audioMixer.SetFloat("effectsVolume", Mathf.Lerp(-80, 0, volumeCurve.Evaluate(effectsVolume.value / 10f)));
	}

	public void UpdateMaster()
	{
		audioMixer.SetFloat("masterVolume", Mathf.Lerp(-80, 0, volumeCurve.Evaluate(masterVolume.value / 10f)));
		PlayerPrefs.SetFloat("MasterVolume", masterVolume.value / 10f);
		PlayerPrefs.Save();
	}

	public void UpdateMusic()
	{
		audioMixer.SetFloat("musicVolume", Mathf.Lerp(-80, 0, volumeCurve.Evaluate(musicVolume.value / 10f)));
		PlayerPrefs.SetFloat("MusicVolume", musicVolume.value / 10f);
		PlayerPrefs.Save();
	}

	public void UpdateEffects()
	{
		audioMixer.SetFloat("effectsVolume", Mathf.Lerp(-80, 0, volumeCurve.Evaluate(effectsVolume.value / 10f)));
		PlayerPrefs.SetFloat("EffectsVolume", effectsVolume.value / 10f);
		PlayerPrefs.Save();
	}

	public void TestEffectAudio ()
	{
		effectAudioTester.Play();
	}
}
