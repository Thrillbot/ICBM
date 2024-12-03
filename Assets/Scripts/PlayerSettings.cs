using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;
using static Universe;

public class PlayerSettings : MonoBehaviour
{
	public AnimationCurve volumeCurve;

	public Slider masterVolume;
	public Slider musicVolume;
	public Slider effectsVolume;
	public AudioMixer audioMixer;

	public AudioSource effectAudioTester;

	public Slider biomeScaleSlider;
	public TMP_Text biomeValue;
	public Slider maxHeightSlider;
	public TMP_Text heightValue;
	public TMP_InputField seedValue;

	void Start()
	{
		seed = Mathf.RoundToInt(Mathf.Lerp(float.MinValue, float.MaxValue, Random.value));
		seedValue.text = seed.ToString();

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

	public void UpdateBiomeScale()
	{
		if (globalBiomeScale == biomeScaleSlider.value) return;

		globalBiomeScale = biomeScaleSlider.value;
		biomeValue.text = Mathf.RoundToInt((biomeScaleSlider.value / 0.06f) * 100f).ToString();
		FindObjectOfType<Worldificate>().GenerateWorld(true);
	}

	public void UpdateMaxHeight()
	{
		if (maxHeight == maxHeightSlider.value) return;

		maxHeight = maxHeightSlider.value;
		heightValue.text = Mathf.RoundToInt(maxHeightSlider.value * 10f).ToString();
		FindObjectOfType<Worldificate>().GenerateWorld(true);
	}

	public void UpdateSeed()
	{
		if (seed == int.Parse(seedValue.text)) return;

		seed = int.Parse(seedValue.text);
		FindObjectOfType<Worldificate>().GenerateWorld(true);
	}

	public void TestEffectAudio ()
	{
		effectAudioTester.Play();
	}
}
