using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Universe;

public class CraftMasterList : MonoBehaviour
{
	public int radioPower;

	public List<Craft> masterCraftList;

	void Update () {
		if (masterCraftList == null || masterCraftList.Count == 0)
			return;

		RaycastHit hit;

		for (int i=0;i<masterCraftList.Count;i++) {
			if (Physics.Linecast(transform.position, masterCraftList[i].transform.position, out hit))
			{
				if (hit.transform.gameObject == masterCraftList[i].gameObject && DistanceCheck(transform.position, masterCraftList[i].transform.position, radioPower))
				{
					masterCraftList[i].GetComponent<Craft>().RadioHit(transform.gameObject);					
					Debug.DrawLine(transform.position, masterCraftList[i].transform.position, Color.red);
					masterCraftList[i].connected = true;
					masterCraftList[i].lastConnection = 0;
				} else {
					masterCraftList[i].connected = false;
					masterCraftList[i].lastConnection += Time.deltaTime;
				}
			}
		}
	}

	public void ConnectCraft (Craft newCraft)
	{
		if (masterCraftList == null)
			masterCraftList = new List<Craft>();
		masterCraftList.Add(newCraft);
	}

	public List<Craft> MasterCraftList
	{
		get { return masterCraftList; }
	}
}

