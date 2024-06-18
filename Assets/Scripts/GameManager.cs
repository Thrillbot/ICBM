using Mirror;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    [SyncVar(hook = "SetGameTime")]
    private float gameTime;

	public float dayLengthInMinutes = 60;

	void Update()
    {
        if (!isServer)
            return;

		SetGameTime(gameTime, gameTime + Time.deltaTime / (dayLengthInMinutes * 60f));
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

	public float GameTime
	{
		get { return gameTime; }
	}
}
