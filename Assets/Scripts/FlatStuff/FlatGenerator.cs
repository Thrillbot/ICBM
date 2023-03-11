using UnityEngine;

public class FlatGenerator : MonoBehaviour
{
    public float radius;
    public float magnitude;
    public float scale;

    void Start()
    {
        Vector3 workerVec;
        Vector3[] vertices = GetComponent<MeshFilter>().mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
		{
            workerVec = vertices[i].normalized * (Mathf.PerlinNoise(vertices[i].x * scale, vertices[i].y * scale) * magnitude + radius);
            workerVec.z = vertices[i].z * radius;
            vertices[i] = Vector3.Lerp(workerVec, vertices[i].normalized * radius, 1f - Mathf.Max(-vertices[i].z / GetComponent<MeshFilter>().mesh.bounds.size.z, 0));
        }
        GetComponent<MeshFilter>().mesh.SetVertices(vertices);
        GetComponent<MeshFilter>().mesh.RecalculateBounds();
        GetComponent<MeshFilter>().mesh.RecalculateNormals();
        GetComponent<MeshFilter>().mesh.RecalculateTangents();
    }
}