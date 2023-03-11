using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Craft : MonoBehaviour {
	
	public CraftMasterList craftList;
	public List<GameObject> hitby = new List<GameObject>();
	
	public MeshRenderer meshRend;
	public int radioPower = 1;
	public bool controlled;
	
	private bool radioPing;
	private int listIndex;
	private float connLerp;
	
	void Start () {
		for (int i=0;i<craftList.crafts.Count;i++) {
			if (craftList.crafts[i].craft == this.gameObject)
				listIndex = i;
		}
	}

	
	public void RadioHit (GameObject _hitby) {
		hitby.Add(_hitby);
		
		if (radioPower > 0 && radioPing == false)
			RadioOut();
		
		craftList.crafts[listIndex].connected = true;
		craftList.crafts[listIndex].lastConnection = 0;
		//meshRend.enabled = true;
		connLerp = 1;
	}

	private bool DistanceCheck (GameObject _checker) {
		if (Vector3.Distance(_checker.transform.position, transform.position) < radioPower)
			return true;
		return false;
	}
	
	private void RadioOut () {
		radioPing = true;
		RaycastHit hit;

		for (int i=0;i<craftList.crafts.Count;i++) {
			if (hitby.Contains(craftList.crafts[i].craft)) {
				continue;
			}
		
			if (Physics.Linecast(transform.position, craftList.crafts[i].craft.transform.position, out hit)) {
				if (hit.transform.gameObject == craftList.crafts[i].craft && DistanceCheck(hit.transform.gameObject)) {
					craftList.crafts[i].craft.GetComponent<Craft>().RadioHit(transform.gameObject);
					Debug.DrawLine(transform.position, craftList.crafts[i].craft.transform.position, Color.red);
				}
			}
		}		
	}
	
	void FixedUpdate () {
		//meshRend.material.color = Color.Lerp(new Color(1f, 1f, 1f, 0f), new Color(1f, 1f, 1f, 1f), connLerp * Time.fixedTime);
		radioPing = false;
		hitby.Clear();
		//meshRend.enabled = false;
		connLerp = 0;
	}
}