using Mirror;
using System.Collections.Generic;
using UnityEngine;

public sealed class Universe
{
    public static float lostSignalVisualFadeTime = 1;

    [SyncVar(hook = "SetLoading")]
	public static bool loading = true;
	[SyncVar(hook = "SetPaused")]
	public static bool paused = true;

    public static Transform planetTransform;
    public static Dictionary<int, Vector3> equator;

	public static float planetResolution = 2048;

	public static float dayLengthInMinutes = 60;

    public static float killAltitude = 80;

	private static GameObject planet;
	private static float gravity = 0.000000667f;
	private static float seaLevel = 100f;
    private static float planetMass = 1499250374.81f;
    private static float karmanLine = 40f;

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

    public static Vector3 GetPointOnPlanet (Vector3 pos)
	{
		Vector3 point = pos;
		point = planetTransform.InverseTransformPoint(point);
        float angle = Vector3.SignedAngle(point.normalized, Vector3.up, -Vector3.forward);
        if (angle < 0)
            angle += 360f;

        angle = angle / 360f * planetResolution;

        try
        {
			point = equator[(int)angle];
            return planetTransform.TransformPoint(point);
        }
        catch
        {
            return Vector3.zero;
        }
    }

    public static bool DistanceCheck(Vector3 from, Vector3 to, float maxDistance)
    {
        if ((from - to).sqrMagnitude < maxDistance * maxDistance)
            return true;
        return false;
	}

	public static void SetLoading(bool oldValue, bool newValue)
	{
		loading = newValue;
	}

	public static void SetPaused(bool oldValue, bool newValue)
	{
		paused = newValue;
	}

	public static GameObject Planet
	{
		set { planet = value; }
		get { return planet; }
	}

	public static float Gravity
	{
		get { return gravity; }
	}

	public static float SeaLevel
	{
		get { return seaLevel; }
	}

	public static float PlanetMass
	{
		get { return planetMass; }
	}

	public static float KarmanLine
    {
        get { return karmanLine; }
    }

    public static float CalculateGravity (float mass)
    {
        return Gravity * PlanetMass / Mathf.Pow(SeaLevel, 2);

	}
}
