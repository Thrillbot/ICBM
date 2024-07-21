using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wobble : MonoBehaviour
{
	public float intensity;
	public float rateInSeconds;
	public AnimationCurve lerpCurve;

	private Vector3 startPositon;
	private Vector3 targetPosition;
	private Vector3 currentPosition;
	private float time;

	private void Start()
	{
		startPositon = transform.position;
		targetPosition = startPositon + new Vector3(GetRandom() * intensity, GetRandom() * intensity, GetRandom() * intensity);
		currentPosition = transform.position;
	}

	private void Update()
	{
		time += Time.deltaTime / rateInSeconds;
		transform.position = Vector3.Lerp(currentPosition, targetPosition, lerpCurve.Evaluate(time));
		if (time >= 1)
		{
			time = 0;
			targetPosition = startPositon + new Vector3(GetRandom() * intensity, GetRandom() * intensity, GetRandom() * intensity);
			currentPosition = transform.position;
		}
	}

	private float GetRandom ()
	{
		return (Random.value * 2f - 1f);
	}
}
