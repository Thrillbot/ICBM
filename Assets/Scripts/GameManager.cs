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

	public static Transform baseLocation;

	private void Start()
	{
		if (perlinNoiseSeed == 0)
			perlinNoiseSeed = Random.Range(int.MinValue, int.MaxValue);
		SetSeed(0, perlinNoiseSeed);
		Random.InitState(perlinNoiseSeed);
		noiseOffset = new Vector3(Random.Range(999, 99999), Random.Range(999, 99999), Random.Range(999, 99999));
		SetNoiseOffset(Vector2.zero, noiseOffset);
	}

	void Update()
    {
        if (!isServer)
            return;

		SetGameTime(gameTime, gameTime + Time.deltaTime / (Universe.dayLengthInMinutes * 60f));
	}

	void SetGameTime(float oldValue, float newValue)
	{
		if (!isServer)
			return;

		gameTime = newValue;
		if (gameTime >= 1f)
		{
			gameTime -= 1f;
		}
	}

	void SetSeed(int oldValue, int newValue)
	{
		if (!isServer)
			return;

		perlinNoiseSeed = newValue;
	}

	void SetNoiseOffset(Vector3 oldValue, Vector3 newValue)
	{
		if (!isServer)
			return;

		noiseOffset = newValue;
	}

	public void FocusBase ()
	{
		GameObject.FindWithTag("FreeCam").GetComponent<FreeCam>().Focus(baseLocation, 5);
	}

	public float GameTime
	{
		get { return gameTime; }
	}

	public Vector3 NoiseOffset
	{
		get { return noiseOffset; }
	}
}
