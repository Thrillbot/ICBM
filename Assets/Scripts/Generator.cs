using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : MonoBehaviour
{
    protected struct Chunk
	{
        public Mesh mesh;
        public GameObject gameObject;
        public Transform transform;
        public MeshCollider collider;
	}

    public bool generateOnStart;
    public Material terrainMaterial;
    public Material oceanMaterial;
    public int resolution = 4;
    public int chunkCount = 8;
    public float radius = 100;
    public float oceanRadius = 97;
    public float scale = 0.1f;
    public float oceanScale = 0.01f;
    public float amplitude = 5;
    public bool generateCrators = true;
    public float averageCratorRadius = 5;
    public float cratorVariance = 3;
    public int cratorAmount = 10;
    public int cratorAmountVariance = 5;

    public bool generate;

    [Header("Gameplay Stuff")]
    public AnimationCurve explosionFalloff;

    private float workerFloat;
    private List<Chunk> chunks = new List<Chunk>();
    private List<Chunk> oceanChunks = new List<Chunk>();
    private Mesh[] chunkMeshes;

    protected struct Explosion
	{
        public Vector3 coords;
        public float strength;
        public float radius;
	}
    private List<Explosion> explosions;

	private void Start()
	{
		if (generateOnStart)
        {
            StartCoroutine(GeneratePlanet());
        }
	}

#if UNITY_EDITOR
	void Update()
    {
        if (generate)
		{
            generate = false;
            StartCoroutine(GeneratePlanet());
        }
    }
#endif

    IEnumerator GeneratePlanet ()
    {
        foreach (Transform child in transform)
        {
            DestroyImmediate(child.gameObject);
            yield return null;
        }

        for (int i = chunks.Count-1; i >= 0; i--)
		{
            DestroyImmediate(chunks[i].gameObject);
		}
        chunks.Clear();

        chunkMeshes = IcoSphereCreator.Create(resolution, radius);

        for (int i = 0; i < chunkMeshes.Length; i++)
        {
            Chunk tempChunk = new Chunk();
            GameObject tempGo = new GameObject();
            tempChunk.gameObject = tempGo;
            tempChunk.gameObject.AddComponent<MeshFilter>();
            tempChunk.gameObject.GetComponent<MeshFilter>().sharedMesh = chunkMeshes[i];
            tempChunk.gameObject.AddComponent<MeshRenderer>();
            tempChunk.gameObject.GetComponent<MeshRenderer>().sharedMaterial = terrainMaterial;
            tempChunk.transform = tempChunk.gameObject.transform;
            tempChunk.transform.parent = transform;
            tempChunk.gameObject.name = "Chunk " + i;
            tempChunk.mesh = chunkMeshes[i];
            tempChunk.collider = tempChunk.gameObject.AddComponent<MeshCollider>();
            tempChunk.collider.sharedMesh = chunkMeshes[i];
            chunks.Add(tempChunk);
            yield return null;
        }

        StartCoroutine(GenerateHeight());
    }

    IEnumerator GenerateHeight ()
	{
        foreach (Chunk c in chunks)
        {
            if (c.mesh == null)
                continue;
            Vector3[] vertices = c.mesh.vertices;
            for (int i = 0; i < vertices.Length; i++)
            {
                workerFloat = Perlin.Noise(vertices[i] * scale) * Perlin.Noise(vertices[i] * oceanScale) * amplitude;
                vertices[i] = vertices[i].normalized * radius + vertices[i].normalized * workerFloat;
            }
            c.mesh.SetVertices(vertices);
            c.mesh.RecalculateNormals();
            c.mesh.MarkDynamic();
            yield return null;
        }

        StartCoroutine(GenerateOcean());

        if (generateCrators)
        {
            StartCoroutine(GenerateCrators());
        }
    }

    IEnumerator GenerateCrators()
    {
        Vector3 workerVec;
        int cratorCount = cratorAmount + (int)((Random.value * 2 - 1) * cratorAmountVariance);
        for (int i = 0; i < cratorCount; i++)
        {
            workerVec = Random.onUnitSphere;
            Explode(workerVec * radius + workerVec * amplitude / 2f, (averageCratorRadius + (int)((Random.value * 2f - 1f) * cratorVariance)) / 2f, averageCratorRadius + (int)((Random.value * 2 - 1) * cratorVariance));
            yield return null;
        }
    }

    IEnumerator GenerateOcean ()
	{
        for (int i = oceanChunks.Count - 1; i >= 0; i--)
        {
            DestroyImmediate(oceanChunks[i].gameObject);
        }
        oceanChunks.Clear();

        chunkMeshes = IcoSphereCreator.Create(resolution, oceanRadius);

        for (int i = 0; i < chunkMeshes.Length; i++)
        {
            Chunk tempChunk = new Chunk();
            GameObject tempGo = new GameObject();
            tempChunk.gameObject = tempGo;
            tempChunk.gameObject.AddComponent<MeshFilter>();
            tempChunk.gameObject.GetComponent<MeshFilter>().sharedMesh = chunkMeshes[i];
            tempChunk.gameObject.AddComponent<MeshRenderer>();
            tempChunk.gameObject.GetComponent<MeshRenderer>().sharedMaterial = oceanMaterial;
            tempChunk.transform = tempChunk.gameObject.transform;
            tempChunk.transform.parent = transform;
            tempChunk.gameObject.name = "OceanChunk " + i;
            tempChunk.mesh = chunkMeshes[i];
            oceanChunks.Add(tempChunk);
            yield return null;
        }

        Universe.paused = false;
        Universe.loading = false;
    }

    public void Explode (Vector3 inCoords, float inStrength, float inRadius)
	{
        if (explosions == null)
            explosions = new List<Explosion>();
        explosions.Add(new Explosion() { coords = inCoords, strength = inStrength, radius = inRadius});

        Vector3[] vertices;
        foreach (Chunk c in chunks) {
            vertices = c.mesh.vertices;
            for (int i = 0; i < vertices.Length; i++)
			{
                if ((vertices[i] - inCoords).magnitude / inRadius < 1)
				{
                    vertices[i] -= vertices[i].normalized * explosionFalloff.Evaluate((vertices[i] - inCoords).magnitude / inRadius) * inStrength;
                }
            }
            c.mesh.vertices = vertices;
            c.mesh.RecalculateNormals();
        }
	}

	private void OnDrawGizmos()
	{
		if (explosions != null)
		{
            Gizmos.color = Color.red;
            foreach (Explosion e in explosions)
			{
                Gizmos.DrawSphere(e.coords, e.radius);
			}
		}
	}
}
