using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
	public PerlinLevel[] perlinLevels;
	public float globalBiomeScale = 25;
	public float maxHeight = 10;
	public AnimationCurve yValueCurve;
	public bool generate;

	private Vector3 workerVec;
	private Vector3 vertexWorldPos;

	private void Update()
	{
		if (generate)
		{
			foreach (GameObject c in chunks)
			{
				GenerateChunk(c);
			}

			generate = false;
		}
	}

	private void GenerateChunk (GameObject chunk)
	{
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
		}
		chunk.GetComponent<MeshFilter>().mesh.vertices = vertices;

		chunk.GetComponent<MeshFilter>().mesh.RecalculateBounds();
		chunk.GetComponent<MeshFilter>().mesh.RecalculateNormals();
		chunk.GetComponent<MeshFilter>().mesh.RecalculateTangents();

		if (chunk.GetComponent<MeshCollider>())
			chunk.GetComponent<MeshCollider>().sharedMesh = chunk.GetComponent<MeshFilter>().mesh;
	}
}
