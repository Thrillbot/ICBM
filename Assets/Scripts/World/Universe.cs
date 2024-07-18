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
	private static float gravity = -0.1f;
	private static float seaLevel = 100f;
    private static float karmanLine = 25f;

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

    [Command]
    public static Vector3 GetPointOnPlanet (Vector3 pos)
	{
		Vector3 point = pos;
		point = planetTransform.InverseTransformPoint(point);
        float angle = Vector3.SignedAngle(point.normalized, Vector3.up, -Vector3.forward);
        if (angle < 0)
            angle += 360f;
        int vertexIndex = (int)(angle / planetResolution);
        int nextIndex = (int)((angle + planetResolution) / planetResolution);

        try
        {
            Vector3 pointOne = equator[vertexIndex];
            Vector3 pointTwo = equator[nextIndex];
            if (nextIndex >= equator.Count)
            {
                pointTwo = pointOne;
                pointOne = equator[0];
            }

            point = Vector3.Lerp(pointOne, pointTwo, (planetResolution * nextIndex - planetResolution * vertexIndex) / planetResolution);
            point = planetTransform.TransformPoint(point);

            return point;
        }
        catch
        {

        }

        return Vector3.zero;
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

    public static float KarmanLine
    {
        get { return karmanLine; }
    }
}
