using System.Collections.Generic;
using UnityEngine;
using static Universe;

public class Worldificate : MonoBehaviour
{
	[System.Serializable]
	public struct PerlinLevel
	{
		public float scale;
		public int seed;
		public float height;
		public float exponent;
		public AnimationCurve heightCurve;
	}

	public GameObject planetObject;

	public PerlinLevel[] perlinLevels;
	public float globalBiomeScale = 25;
	public float maxHeight = 10;
	public AnimationCurve yValueCurve;
	public bool generate;
	public bool fakePlanet;
	public bool pauseUniverse = true;

	private Vector3 workerVec;
	private Vector3 vertexWorldPos;
	private Vector3 noiseOffset;

	private bool worldGenerated = false;

	private void Start()
	{
		Planet = gameObject;
	}

	private void Update()
	{
		if (generate)
		{
			if (fakePlanet)
				noiseOffset = Vector3.one * Random.Range(-9999,9999);
			else
				noiseOffset = FindObjectOfType<GameManager>().NoiseOffset;

			SetLoading(false, true);
			if (!fakePlanet)
			{
				//Debug.Log("Not Fake Planet!  Creating Equator!");
				equator = new Dictionary<int, Vector3>();
			}
			planetResolution = 360f / planetResolution;

			GenerateChunk(planetObject.transform.GetChild(0).gameObject);
			GenerateChunk(planetObject.transform.GetChild(1).gameObject);

			planetTransform = transform.parent;

			generate = false;
			SetLoading(true, false);
		}

		SetPaused(!pauseUniverse, pauseUniverse);
	}

	public void GenerateWorld ()
	{
		if (worldGenerated) return;
		worldGenerated = true;
		fakePlanet = false;

		if (FindObjectOfType<GameManager>())
			noiseOffset = FindObjectOfType<GameManager>().NoiseOffset;

		SetLoading(false, true);
		equator = new Dictionary<int, Vector3>();
		planetResolution = 360f / planetResolution;

		GenerateChunk(planetObject.transform.GetChild(0).gameObject);
		GenerateChunk(planetObject.transform.GetChild(1).gameObject);

		planetTransform = transform.parent;

		generate = false;
		SetLoading(true, false);
	}

	private void GenerateChunk (GameObject chunk)
	{
		float angle;
		Vector3[] vertices = chunk.GetComponent<MeshFilter>().mesh.vertices;
		for (int i = 0; i < vertices.Length; i++)
		{
			float yValue = vertices[i].y;
			vertexWorldPos = chunk.transform.TransformPoint(vertices[i]);
			workerVec = vertexWorldPos.normalized;

			foreach (PerlinLevel p in perlinLevels)
			{
				workerVec += vertexWorldPos.normalized * Mathf.Pow(p.heightCurve.Evaluate(Perlin.Noise(vertexWorldPos.x * p.scale + p.seed + noiseOffset.x, vertexWorldPos.y * p.scale + p.seed + noiseOffset.y, vertexWorldPos.z * p.scale + p.seed + noiseOffset.z)), p.exponent) * p.height;
				workerVec -= vertexWorldPos.normalized;
			}
			vertices[i] += chunk.transform.InverseTransformPoint(workerVec * Perlin.Noise(vertexWorldPos.x * globalBiomeScale + noiseOffset.x, vertexWorldPos.y * globalBiomeScale + noiseOffset.y, vertexWorldPos.z * globalBiomeScale + noiseOffset.z)) * yValueCurve.Evaluate(vertices[i].normalized.y);
			vertices[i].y = yValue;
			if (Mathf.Abs(chunk.transform.TransformPoint(vertices[i]).z) < 0.2f)
			{
				angle = Vector3.SignedAngle(Vector3.up, chunk.transform.TransformPoint(vertices[i]).normalized, Vector3.forward);
				if (angle < 0)
					angle += 360;
				if (!fakePlanet)
				{
					equator.TryAdd((int)((angle / 360f) * planetResolution), chunk.transform.TransformPoint(vertices[i]));
				}
			}
		}
		chunk.GetComponent<MeshFilter>().mesh.vertices = vertices;

		chunk.GetComponent<MeshFilter>().mesh.RecalculateBounds();
		chunk.GetComponent<MeshFilter>().mesh.RecalculateNormals();
		chunk.GetComponent<MeshFilter>().mesh.RecalculateTangents();

		if (chunk.GetComponent<MeshCollider>())
			chunk.GetComponent<MeshCollider>().sharedMesh = chunk.GetComponent<MeshFilter>().mesh;
	}

	public float GetHeight ()
	{


		return 0;
	}

	private void OnDrawGizmos()
	{
		if (equator == null || fakePlanet)
			return;
		Gizmos.color = Color.red;
		for (int i = 0; i < planetResolution; i++)
		{
			Gizmos.DrawSphere(equator.GetValueOrDefault(i), 0.25f);
		}
	}
}