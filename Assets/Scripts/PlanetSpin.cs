using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetSpin : MonoBehaviour
{
    public float dayLengthInMinutes;
    public Vector3 axis;

    private float timer = 0;

	private void Start()
    {
        Universe.dayLengthInMinutes = dayLengthInMinutes;
    }

	void Update()
    {
        if (Universe.paused || Universe.loading)
            return;
        timer += Time.deltaTime/(Universe.dayLengthInMinutes * 60f);
        transform.localEulerAngles = timer * 360f * axis.normalized;
        if (timer >= 1f)
		{
            timer -= 1f;
		}
    }
}
