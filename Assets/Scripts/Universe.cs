using UnityEngine;

public sealed class Universe : MonoBehaviour
{
	public static float gravity = -9.81f;
    public static float lostSignalVisualFadeTime = 1;
    public static float dayLengthInMinutes;

	public static bool loading = true;
	public static bool paused = true;

    private static Universe instance = null;

    private static string currentCraft;

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

    public static Vector3 GetMousePointOnPlane()
    {
        Vector3 point = Input.mousePosition;
        point.z = Camera.main.transform.position.z;
        point = Camera.main.ScreenToWorldPoint(point);
        return point;
    }

    public static bool DistanceCheck(Vector3 from, Vector3 to, float maxDistance)
    {
        if ((from - to).sqrMagnitude < maxDistance * maxDistance)
            return true;
        return false;
    }

    public void Print(string message)
	{
        print(message);
	}
}
