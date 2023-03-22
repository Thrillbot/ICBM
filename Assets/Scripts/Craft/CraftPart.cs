using Newtonsoft.Json.Bson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftPart : MonoBehaviour {

    public GameObject [] mounts;
    
    public bool notAttached;

    public float health = 100;
    public float mass = 1;
    public float GarbageCollectTimer = 1000;

    private int garbagetimer;
    public void TakeDamage(float dmg)
    {
        health -= dmg;
        if (health < 0)
            DetachChildren();
        Destroy(gameObject);
    }
    private void DetachChildren()
    {
        foreach (GameObject mount in mounts)
        {
            if (mount.transform.childCount == 0)
                continue;
            Transform mounted = mount.transform.GetChild(0);
            if (mounted.root.gameObject.name == "Root")   /////////////////////////////////will always call root, need to figure out how not do
                continue;
            mounted.GetComponent<CraftPart>().PostLaunchDetach();
        }
    }
    public void PostLaunchDetach()
    {
        if (garbagetimer > GarbageCollectTimer)
        {
            Destroy(gameObject);
        }
        else
        {
            garbagetimer++;
            PostLaunchDetach();
        }
    }
}
