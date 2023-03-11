using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class HeightToMesh : MonoBehaviour
{
    public Mesh baseMesh;
    public Texture2D heightMap;
    public float heightIntensity = 0.1f;
    public float minHeight = 0.5f;
    public bool applyHeightMap;

    private Mesh m;

    void Update()
    {
        if (applyHeightMap)
		{
            m = baseMesh;
            Vector3[] vertices = m.vertices;
            Vector2[] uvs = m.uv;
            int width = heightMap.width;
            int height = heightMap.height;
            Color[] colors = heightMap.GetPixels();
            for (int i = 0; i < vertices.Length && i < uvs.Length; i++)
			{
                try
                {
                    vertices[i] = (colors[(int)(uvs[i].x * width) + (int)(uvs[i].y * height) * width].r * heightIntensity) * vertices[i].normalized + vertices[i].normalized * minHeight;
                }
                catch
				{
                    continue;
				}
            }
            m.vertices = vertices;
            m.RecalculateNormals();
            m.RecalculateTangents();
            m.RecalculateBounds();

            GetComponent<MeshFilter>().mesh = m;
            if (GetComponent<MeshCollider>())
                GetComponent<MeshCollider>().sharedMesh = GetComponent<MeshFilter>().mesh;

            applyHeightMap = false;
        }
    }
}
