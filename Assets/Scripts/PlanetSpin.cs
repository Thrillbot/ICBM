using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetSpin : MonoBehaviour
{
    public float dayLengthInMinutes;

    private float timer = 0;

    void Update()
    {
        if (Universe.paused || Universe.loading)
            return;
        timer += Time.deltaTime/(dayLengthInMinutes * 60f);
        transform.localEulerAngles = new Vector3(0, timer * 360f, 0);
        if (timer >= 1f)
		{
            timer -= 1f;
		}
    }
}
