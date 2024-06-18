using System.Collections.Generic;
using UnityEngine;
using static Universe;

public class Craft : MonoBehaviour
{
	private CraftMasterList craftList;
	public List<GameObject> hitby = new List<GameObject>();

	private MeshRenderer meshRend;
	public int radioPower = 1;
	public bool controlled;

	public bool connected;
	public float lastConnection;

	private bool radioPing;
	private int listIndex;
	private float connLerp;
	
	void Start () {
		craftList = FindObjectOfType<CraftMasterList>();
		if (craftList == null)
			return;

		craftList.ConnectCraft(this);

		meshRend = GetComponent<MeshRenderer>();

		for (int i = 0; i < craftList.MasterCraftList.Count; i++)
		{
			if (craftList.MasterCraftList[i] == this.gameObject)
				listIndex = i;
		}
	}

	
	public void RadioHit (GameObject _hitby) {
		hitby.Add(_hitby);
		
		if (radioPower > 0 && radioPing == false)
			RadioOut();
		
		craftList.MasterCraftList[listIndex].connected = true;
		craftList.MasterCraftList[listIndex].lastConnection = 0;
		//meshRend.enabled = true;
		connLerp = 1;
	}
	
	private void RadioOut () {
		radioPing = true;
		RaycastHit hit;

		for (int i=0;i<craftList.MasterCraftList.Count;i++) {
			if (hitby.Contains(craftList.MasterCraftList[i].gameObject)) {
				continue;
			}
		
			if (Physics.Linecast(transform.position, craftList.MasterCraftList[i].transform.position, out hit)) {
				if (hit.transform.gameObject == craftList.MasterCraftList[i].gameObject && DistanceCheck(transform.position, hit.transform.position, radioPower)) {
					craftList.MasterCraftList[i].GetComponent<Craft>().RadioHit(transform.gameObject);
					Debug.DrawLine(transform.position, craftList.MasterCraftList[i].transform.position, Color.red);
				}
			}
		}		
	}
	
	void FixedUpdate () {
		if (craftList == null)
		{
			craftList = FindObjectOfType<CraftMasterList>();

			if (craftList == null)
				return;

			craftList.ConnectCraft(this);

			meshRend = GetComponent<MeshRenderer>();

			for (int i = 0; i < craftList.MasterCraftList.Count; i++)
			{
				if (craftList.MasterCraftList[i] == this.gameObject)
					listIndex = i;
			}
		}

		if (connLerp > 0)
			connLerp -= Time.deltaTime / lostSignalVisualFadeTime;
		else
		{
			connLerp = 0;
		}
		meshRend.material.color = Color.Lerp(new Color(1f, 1f, 1f, 0f), new Color(1f, 1f, 1f, 1f), connLerp);
		radioPing = false;
		hitby.Clear();
		//meshRend.enabled = false;
	}
}