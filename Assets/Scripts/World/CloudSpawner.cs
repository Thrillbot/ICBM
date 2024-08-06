using Cinemachine.Utility;
using System.Collections.Generic;
using UnityEngine;

public class CloudSpawner : MonoBehaviour
{
	public struct Cloud
	{
		public float life;
		public GameObject gameObject;
	}

	public GameObject cloudPrefab;

	public int initialSpawnAmount = 250;
	public float spawnRate = 1;
	public float maxCloudCount = 500;
	public float cloudLife = 30;
	public float cloudLayer = 108;
	public float cloudHeightVariance = 1;
	public float cloudSizeVariance = 0.1f;
	public AnimationCurve cloudLifeSize;
	public float driftSpeed = 1;

	private float timer = 0;
	private List<Cloud> clouds;
	private Cloud workerCloud;
	private Vector3 workerVec;

	private void Start()
	{
		clouds ??= new();
		for (int i = 0; i < initialSpawnAmount; i++)
		{
			SpawnCloud(true);
		}
	}

	private void Update()
	{
		transform.localEulerAngles += Vector3.forward * Time.deltaTime * driftSpeed;

		if (clouds.Count < maxCloudCount)
		{
			timer += Time.deltaTime / spawnRate;
			if (timer >= 1)
			{
				timer = 0;
				SpawnCloud();
			}
		}

		for (int i = clouds.Count - 1; i >= 0; i--)
		{
			workerCloud = clouds[i];
			workerCloud.life -= Time.deltaTime;
			workerCloud.gameObject.transform.GetChild(0).localScale = Vector3.one * cloudLifeSize.Evaluate(workerCloud.life / cloudLife);
			clouds[i] = workerCloud;

			if (clouds[i].life <= 0) clouds.RemoveAt(i);
		}
	}
	 
	void SpawnCloud (bool randomizeLife = false)
	{
		GameObject tempGo = Instantiate(cloudPrefab);
		tempGo.transform.GetChild(0).localEulerAngles = new Vector3(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));

		/*
		tempGo.transform.localEulerAngles = Vector3.forward * Random.Range(0, 360);
		tempGo.transform.position = tempGo.transform.up * (cloudLayer + ((Random.value * 2f - 1f) * cloudHeightVariance));
		*/
		workerVec = Random.onUnitSphere;
		workerVec.z = -Mathf.Abs(workerVec.z);
		tempGo.transform.position = workerVec * (cloudLayer + ((Random.value * 2f - 1f) * cloudHeightVariance));
		tempGo.transform.LookAt(Vector3.zero);

		tempGo.transform.localScale *= (1f + (cloudSizeVariance * (Random.value * 2f - 1f)));
		tempGo.transform.parent = transform;
		tempGo.layer = gameObject.layer;
		foreach (Renderer r in tempGo.transform.GetComponentsInChildren<Renderer>())
		{
			r.gameObject.layer = gameObject.layer;
		}

		workerCloud = new()
		{
			life = cloudLife * (randomizeLife ? Random.value : 1),
			gameObject = tempGo
		};

		clouds.Add(workerCloud);
	}
}
