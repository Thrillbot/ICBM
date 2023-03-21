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

	public GameObject[] chunks;
	public GameObject[] innerBits;
	public PerlinLevel[] perlinLevels;
	public float globalBiomeScale = 25;
	public float maxHeight = 10;
	public AnimationCurve yValueCurve;
	public float planetResolution = 2048;
	public bool generate;
	public bool pauseUniverse = true;

	private Vector3 workerVec;
	private Vector3 vertexWorldPos;

	private void Update()
	{
		if (generate)
		{
			resolution = planetResolution;
			loading = true;
			equator = new Dictionary<int, Vector3>();
			resolution = 360f / resolution;

			for (int i = 0; i < chunks.Length; i++)
			{
				GenerateChunk(chunks[i]);
			}
			foreach (GameObject c in innerBits)
			{
				GenerateChunk(c);
			}
			planetTransform = transform.parent;

			generate = false;
			loading = false;
		}

		paused = pauseUniverse;
	}

	private void GenerateChunk (GameObject chunk)
	{
		float angle;
		Vector3[] vertices = chunk.GetComponent<MeshFilter>().mesh.vertices;
		for (int i = 0; i < vertices.Length; i++)
		{
			vertexWorldPos = chunk.transform.TransformPoint(vertices[i]);
			workerVec = vertexWorldPos.normalized;

			foreach (PerlinLevel p in perlinLevels)
			{
				workerVec += vertexWorldPos.normalized * Mathf.Pow(p.heightCurve.Evaluate(Perlin.Noise(vertexWorldPos.x * p.scale + p.seed, vertexWorldPos.y * p.scale + p.seed, vertexWorldPos.z * p.scale + p.seed)), p.exponent) * p.height;
				workerVec -= vertexWorldPos.normalized;
			}
			vertices[i] += chunk.transform.InverseTransformPoint(workerVec * Perlin.Noise(vertexWorldPos.x * globalBiomeScale, vertexWorldPos.y * globalBiomeScale, vertexWorldPos.z * globalBiomeScale)) * yValueCurve.Evaluate(vertices[i].normalized.y);
			if (Mathf.Abs(chunk.transform.TransformPoint(vertices[i]).z) < 0.2f)
			{
				angle = Vector3.SignedAngle(Vector3.up, chunk.transform.TransformPoint(vertices[i]).normalized, Vector3.forward);
				if (angle < 0)
					angle += 360;
				equator.TryAdd((int)(angle / resolution), chunk.transform.TransformPoint(vertices[i]));
			}
		}
		chunk.GetComponent<MeshFilter>().mesh.vertices = vertices;

		chunk.GetComponent<MeshFilter>().mesh.RecalculateBounds();
		chunk.GetComponent<MeshFilter>().mesh.RecalculateNormals();
		chunk.GetComponent<MeshFilter>().mesh.RecalculateTangents();

		if (chunk.GetComponent<MeshCollider>())
			chunk.GetComponent<MeshCollider>().sharedMesh = chunk.GetComponent<MeshFilter>().mesh;
	}

	private void OnDrawGizmos()
	{
		if (equator == null)
			return;
		Gizmos.color = Color.red;
		for (int i = 0; i < planetResolution; i++)
		{
			Gizmos.DrawSphere(equator.GetValueOrDefault(i), 0.25f);
		}
	}
}