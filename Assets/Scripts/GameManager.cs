using Mirror;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
	[SyncVar(hook = "SetGameTime")]
	private float gameTime;

	[SyncVar(hook = "SetSeed")]
	private int perlinNoiseSeed;

	[SyncVar(hook = "SetNoiseOffset")]
	private Vector3 noiseOffset;
	private static bool noiseSyncd = false;

	private void Start()
	{
		if (isServer)
		{
			if (perlinNoiseSeed == 0)
				perlinNoiseSeed = Random.Range(int.MinValue, int.MaxValue);
			Random.InitState(perlinNoiseSeed);
			noiseOffset = new Vector3(Random.Range(999, 99999), Random.Range(999, 99999), Random.Range(999, 99999));
		}
	}

	void Update()
    {
        if (!isServer)
            return;

		SetGameTime(gameTime, gameTime + Time.deltaTime / (Universe.dayLengthInMinutes * 60f));
	}

	void SetGameTime(float oldValue, float newValue)
	{
		gameTime = newValue;
		if (gameTime >= 1f)
		{
			gameTime -= 1f;
		}
	}

	void SetSeed(int oldValue, int newValue)
	{
		perlinNoiseSeed = newValue;
	}

	void SetNoiseOffset(Vector3 oldValue, Vector3 newValue)
	{
		noiseOffset = newValue;
		noiseSyncd = true;

		FindObjectOfType<Worldificate>().GenerateWorld();
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
