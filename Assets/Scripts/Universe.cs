using Fusion;
using System.Collections.Generic;
using UnityEngine;

public sealed class Universe
{
	public static float gravity = -9.81f;
    public static float lostSignalVisualFadeTime = 1;
    public static float dayLengthInMinutes;

	public static bool loading = true;
	public static bool paused = true;

    public static Transform planetTransform;
    public static Dictionary<int, Vector3> equator;

    public static float resolution;

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

    public static Vector3 GetMousePointOnPlane ()
    {
        Vector3 point = Input.mousePosition;
        point.z = Camera.main.transform.position.z;
        point = Camera.main.ScreenToWorldPoint(point);
        return point;
    }

    public static Vector3 GetMousePointOnPlanet ()
	{
        Vector3 point = GetMousePointOnPlane();
        point = planetTransform.InverseTransformPoint(point);
        float angle = Vector3.SignedAngle(point.normalized, Vector3.up, -Vector3.forward);
        if (angle < 0)
            angle += 360f;
        int vertexIndex = (int)(angle / resolution);
        int nextIndex = (int)((angle + resolution) / resolution);

        Vector3 pointOne = equator[vertexIndex];
        Vector3 pointTwo = equator[nextIndex];
        if (nextIndex >= equator.Count)
		{
            pointTwo = pointOne;
            pointOne = equator[0];
        }

        point = Vector3.Lerp(pointOne, pointTwo, (resolution * nextIndex - resolution * vertexIndex) / resolution);
        point = planetTransform.TransformPoint(point);

        return point;
    }

    public static bool DistanceCheck(Vector3 from, Vector3 to, float maxDistance)
    {
        if ((from - to).sqrMagnitude < maxDistance * maxDistance)
            return true;
        return false;
    }
}
