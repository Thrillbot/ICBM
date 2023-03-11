using System.Collections.Generic;
using UnityEngine;

public class FirstComputerShaderTest : MonoBehaviour
{
    public struct Vertex
    {
        public Vector3 position;
    }

    public ComputeShader computeShader;
    public MeshFilter meshFilter;
    public int seed = 0;

	private void Start()
	{
        if (seed == 0)
		{
            seed = Random.Range(-999999, 999999);
        }
        //CreateVertices();
        OnRandomizeGPU();

    }

    /*
	void CreateVertices ()
    {
        data = new Vertex[count * count];

        for (int x = 0; x < count; x++)
		{
            for (int y = 0; y < count; y++)
            {
                CreateVertice(x, y);
            }
        }
    }
    */

    void CreateVertice(int x, int y)
	{
        /*
        GameObject vertex = new GameObject("Vertex " + (x * count + y), typeof(MeshFilter), typeof(MeshRenderer));
        vertex.GetComponent<MeshFilter>().mesh = mesh;

        vertex.transform.parent = transform;
        vertex.transform.localPosition = new Vector3(x, y, Random.Range(-0.1f, 0.1f));

        Vertex vertexData = new Vertex();
        vertexData.position = vertex.transform.localPosition;
        data[x * count + y] = vertexData;
        */
	}

    public void OnRandomizeGPU()
    {
        int vector3Size = sizeof(float) * 3;
        int totalSize = vector3Size;
        Vector3[] vertices = meshFilter.mesh.vertices;
        ComputeBuffer vertexBuffer = new ComputeBuffer(vertices.Length, totalSize);
        vertexBuffer.SetData(vertices);
        computeShader.SetBuffer(0, "vertices", vertexBuffer);
        computeShader.SetFloat("resolution", vertices.Length);
        computeShader.SetFloat("seed", seed);
        computeShader.Dispatch(0, vertices.Length / 8, 1, 1);

        vertexBuffer.GetData(vertices);

        /*
        Vector3[] vertices = meshFilter.mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
		{
            Vertex vertex = data[i];
            vertices[i] = vertex.position;
		}
        */

        meshFilter.mesh.SetVertices(vertices);
        meshFilter.mesh.RecalculateBounds();
        meshFilter.mesh.RecalculateNormals();
        meshFilter.mesh.RecalculateTangents();

        vertexBuffer.Dispose();

        Universe.paused = false;
        Universe.loading = false;
	}
}
