using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class CraftBuilder : NetworkTransform
{
    public GameObject[] prefabs;

    private int currentIndex = -1;
    private int prevIndex = -1;
    private GameObject currentPrefab;

    void Update()
    {
        if (Input.GetAxis("Mouse Wheel") < 0)
        {
            currentIndex--;
        }
        else if (Input.GetAxis("Mouse Wheel") > 0)
        {
            currentIndex++;
        }

        if (currentIndex < -1)
            currentIndex = prefabs.Length - 1;
        else if (currentIndex > prefabs.Length - 1)
            currentIndex = -1;

        if (currentIndex < 0)
		{
            Destroy(currentPrefab);
		}
        else if (prevIndex != currentIndex)
		{
            Destroy(currentPrefab);
            //currentPrefab = 
        }

    }
}
