using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftMasterList : MonoBehaviour {

	[System.Serializable]
	public class Crafts {
		public GameObject craft;
		public bool connected;
		public float lastConnection;
	}
	
	public List<Crafts> crafts = new List<Crafts>();
	
	public int radioPower;

	void Update () {
		RaycastHit hit;

		for (int i=0;i<crafts.Count;i++) {
			if (Physics.Linecast(transform.position, crafts[i].craft.transform.position, out hit)) {
				if (hit.transform.gameObject == crafts[i].craft && DistanceCheck(hit.transform.gameObject)) {
					crafts[i].craft.GetComponent<Craft>().RadioHit(transform.gameObject);					
					Debug.DrawLine(transform.position, crafts[i].craft.transform.position, Color.red);
					crafts[i].connected = true;
					crafts[i].lastConnection = 0;
				} else {
					crafts[i].connected = false;
					crafts[i].lastConnection++;
				}
			}
		}
	}
	
	private bool DistanceCheck (GameObject _checker) {
		if (Vector3.Distance(_checker.transform.position, transform.position) < radioPower)
			return true;
		return false;
	}	
}

