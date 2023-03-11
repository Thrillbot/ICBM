using UnityEngine;

public sealed class Universe : MonoBehaviour
{
	public static float gravity = -9.81f;

	public static bool loading = true;
	public static bool paused = true;

    private static Universe instance = null;

    private Universe()
    {
    }

    public static Universe Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new Universe();
            }
            return instance;
        }
    }

    public void Print(string message)
	{
        print(message);
	}
}
