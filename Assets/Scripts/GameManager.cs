using Mirror;
using UnityEngine;
using static Universe;

public class GameManager : NetworkBehaviour
{
	[SyncVar(hook = "SetGameTime")]
	private float gameTime;

	[SyncVar(hook = "SetPerlinSeed")]
	private int perlinNoiseSeed;

	[SyncVar(hook = "SetNoiseOffset")]
	private Vector3 noiseOffset;
	[SyncVar(hook = "SetBiomeScale")]
	private float syncedBiomeScale;
	[SyncVar(hook = "SetMaxHeight")]
	private float syncedMaxHeight;
	[SyncVar(hook = "SetSeed")]
	private int syncedSeed;
	private static bool noiseSyncd = false;

	private void Start()
	{
		if (isServer)
		{
			if (perlinNoiseSeed == 0)
				perlinNoiseSeed = Random.Range(int.MinValue, int.MaxValue);
			Random.InitState(perlinNoiseSeed);
			syncedSeed = seed;
			syncedBiomeScale = globalBiomeScale;
			syncedMaxHeight = maxHeight;
			noiseOffset = Vector3.one * seed;
			//noiseOffset = new Vector3(Random.Range(999, 99999), Random.Range(999, 99999), Random.Range(999, 99999));
		}
	}

	void Update()
    {
        if (!isServer)
            return;

		SetGameTime(gameTime, gameTime + Time.deltaTime / (dayLengthInMinutes * 60f));
	}

	void SetGameTime(float oldValue, float newValue)
	{
		gameTime = newValue;
		if (gameTime >= 1f)
		{
			gameTime -= 1f;
		}
	}

	void SetPerlinSeed(int oldValue, int newValue)
	{
		perlinNoiseSeed = newValue;
	}

	void SetNoiseOffset(Vector3 oldValue, Vector3 newValue)
	{
		noiseOffset = newValue;
		noiseSyncd = true;

		FindObjectOfType<Worldificate>().GenerateWorld();
	}

	void SetBiomeScale(float oldValue, float newValue)
	{
		globalBiomeScale = newValue;
	}

	void SetMaxHeight(float oldValue, float newValue)
	{
		maxHeight = newValue;
	}

	void SetSeed(int oldValue, int newValue)
	{
		seed = newValue;
	}

	public float GameTime
	{
		get { return gameTime; }
	}

	public Vector3 NoiseOffset
	{
		get { return noiseOffset; }
	}

	public static bool NoiseSyncd
	{
		get { return noiseSyncd; }
	}
}
