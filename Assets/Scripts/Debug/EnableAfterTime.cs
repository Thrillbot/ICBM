using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableAfterTime : MonoBehaviour
{
    public float time = 1;

    public GameObject[] gameObjects;

    void Start()
    {
        if (gameObjects == null || gameObjects.Length == 0)
            return;

        StartCoroutine(Initialize());
    }

    IEnumerator Initialize ()
    {
        yield return new WaitForSeconds(time);
        foreach (GameObject go in gameObjects) {
            go.SetActive(true);
        }
    }
}
