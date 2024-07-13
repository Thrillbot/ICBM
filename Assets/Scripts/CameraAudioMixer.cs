using UnityEngine;
using UnityEngine.Audio;

public class CameraAudioMixer : MonoBehaviour
{
    public AudioMixer mixer;
	public AnimationCurve pressureCurve;

	private AudioMixerSnapshot inAtmoSnapshot;
	private AudioMixerSnapshot inSpaceSnapshot;
	private float airPressure;

	private void Start()
	{
		inAtmoSnapshot = mixer.FindSnapshot("InAtmo");
		inSpaceSnapshot = mixer.FindSnapshot("InSpace");
	}

	void Update()
    {
		airPressure = (Universe.KarmanLine - (transform.position.magnitude - Universe.SeaLevel)) / Universe.KarmanLine;
		airPressure = Mathf.Clamp(airPressure, 0.0f, 1.0f); // Clamp that so we can do a force
		airPressure = pressureCurve.Evaluate(airPressure);

		mixer.TransitionToSnapshots(new AudioMixerSnapshot[2] {inAtmoSnapshot, inSpaceSnapshot}, new float[2] { airPressure , 1f - airPressure }, 0);
	}
}