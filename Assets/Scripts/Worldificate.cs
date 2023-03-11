using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class Worldificate : MonoBehaviour
{
    public struct Biome
    {
        public Texture2D heightMap;
        public float temperature;
        public float humidity;
    }

    public struct Feature
    {
        public Texture2D heightMap;
        public Vector2 location;
        public int scale;
    }

    public Texture2D basePlanet;
    public AnimationCurve temperatureCurve;
    public AnimationCurve humidityCurve;
    public float[] temperatureScales;
    public float[] humidityScales;
    [Range(0,1)]
    public float biomeBlending = 0.1f;
    public int tempSeed;
    public int humSeed;
    public float planetWrapOverlap = 500;
    public int minFeatures = 10;
    public int maxFeatures = 100;
    public float minFeatureSize = 100;
    public float maxFeatureSize = 3000;
    public float featureHeight = 0.2f;
    public Texture2D[] biomes;

    public bool generate;

    private int resolution;

    private Texture2D worldTex;
    private List<Biome> biomeList;
    private List<Feature> features;
    private int featureCount;

    private Color[] colors;
    private float height = 0;
    private float temperature = 0;
    private float humidity = 0;

    void Update()
    {
        if (generate)
        {
            resolution = basePlanet.width;

            biomeList = new List<Biome>();
            features = new List<Feature>();

            colors = basePlanet.GetPixels();

            height = 0;
            temperature = 0;
            humidity = 0;

            foreach (Texture2D b in biomes)
            {
                Biome tempBiome = new();
                tempBiome.heightMap = b;
                try
                {
                    tempBiome.temperature = float.Parse(b.name.Split('_')[1]) / 100f;
                    tempBiome.humidity = float.Parse(b.name.Split('_')[2]) / 100f;
                }
                catch
                {
                    tempBiome.temperature = 0;
                    tempBiome.humidity = 0;
                }
                biomeList.Add(tempBiome);
            }

            featureCount = Random.Range(minFeatures, maxFeatures);
            for (int i = 0; i < featureCount; i++)
            {
                Feature tempFeature = new();
                tempFeature.heightMap = biomes[Random.Range(0, biomes.Length)];
                tempFeature.location = new Vector2(Random.Range(-1, resolution - 1), Random.Range(-1, resolution - 1));
                tempFeature.scale = (int)Random.Range(minFeatureSize, maxFeatureSize);
                features.Add(tempFeature);
            }

            if (tempSeed == 0)
            {
                tempSeed = Random.Range(-999999, 999999);
            }
            if (humSeed == 0)
            {
                humSeed = Random.Range(-999999, 999999);
            }

            foreach (Feature f in features)
            {
                Color[] hColors = f.heightMap.GetPixels();
                int fResolution = f.heightMap.width;
                for (int x = 0; x < f.scale; x++)
                {
                    for (int y = 0; y < f.scale; y++)
                    {
                        if (((int)f.location.x + x) + ((int)f.location.y + y) * resolution < colors.Length && ((int)f.location.x + x) + ((int)f.location.y + y) * resolution >= 0)
                            colors[((int)f.location.x + x) + ((int)f.location.y + y) * resolution] += Color.white * ((hColors[(int)((float)x / f.scale * fResolution) + (int)((float)y / f.scale * fResolution) * f.heightMap.width].r * 2f - 1f) * featureHeight);
                    }
                }
            }

            for (int x = 0; x < resolution; x++)
            {
                for (int y = 0; y < resolution; y++)
                {
                    temperature = 0;
                    humidity = 0;
                    foreach (float f in temperatureScales)
                        temperature += Perlin.Noise((x * f) + tempSeed, (y * f) + tempSeed) * temperatureCurve.Evaluate((float)y / (float)resolution);
                    foreach (float f in humidityScales)
                        humidity += Perlin.Noise((x * f) + humSeed, (y * f) + humSeed) * temperatureCurve.Evaluate((float)y / (float)resolution);
                    height = temperature * humidity;

                    colors[x + y * resolution] += new Color(height, height, height, 1);
                    colors[x + y * resolution] = Color.Lerp(colors[x + y * resolution], colors[y * resolution], ((float)x - (float)resolution + planetWrapOverlap) / planetWrapOverlap);
                }
            }

            worldTex = new(resolution, resolution);
            worldTex.filterMode = FilterMode.Bilinear;
            worldTex.SetPixels(colors);
            worldTex.Apply();

            byte[] itemBGBytes = worldTex.EncodeToPNG();
            File.WriteAllBytes(Application.dataPath + "/Resources/Planet.png", itemBGBytes);

            DestroyImmediate(worldTex);

            System.GC.Collect();

            generate = false;
        }
    }
}
