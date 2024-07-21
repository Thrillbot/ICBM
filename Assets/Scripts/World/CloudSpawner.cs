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

	private void Start()
	{
		clouds ??= new();
		for (int i = 0; i < initialSpawnAmount; i++)
		{
			GameObject tempGo = Instantiate(cloudPrefab);
			tempGo.transform.GetChild(0).localEulerAngles = Vector3.up * Random.Range(0, 360);
			tempGo.transform.localEulerAngles = Vector3.forward * Random.Range(0, 360);
			tempGo.transform.localScale *= (1f + (cloudSizeVariance * (Random.value * 2f - 1f)));
			tempGo.transform.position = tempGo.transform.up * (cloudLayer + ((Random.value * 2f - 1f) * cloudHeightVariance));
			tempGo.transform.parent = transform;

			workerCloud = new()
			{
				life = cloudLife * Random.value,
				gameObject = tempGo
			};

			clouds.Add(workerCloud);
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
				GameObject tempGo = Instantiate(cloudPrefab);
				tempGo.transform.GetChild(0).localEulerAngles = Vector3.up * Random.Range(0, 360);
				tempGo.transform.localEulerAngles = Vector3.forward * Random.Range(0, 360);
				tempGo.transform.localScale *= (1f - (cloudSizeVariance * (Random.value * 2f - 1f)));
				tempGo.transform.position = tempGo.transform.up * (cloudLayer + ((Random.value * 2f - 1f) * cloudHeightVariance));
				tempGo.transform.parent = transform;

				workerCloud = new()
				{
					life = cloudLife,
					gameObject = tempGo
				};

				clouds.Add(workerCloud);
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
}
