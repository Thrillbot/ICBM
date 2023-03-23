using Fusion;
using Mono.Cecil.Cil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static System.Net.WebRequestMethods;
using static Universe;

public class Craft : MonoBehaviour
{
	private CraftMasterList craftList;
	private List<GameObject> hitby = new List<GameObject>();
    public List<GameObject> craftParts = new List<GameObject>();
    public List<StagingPart> stages = new List<StagingPart>();

	public int radioPower = 0;
	public bool controlled;

	public bool connected;
	public float lastConnection;

	private bool radioPing;
	private int listIndex;

    private float fuelTot;
    private int curStage;
	
	void Start () {
		craftList = FindObjectOfType<CraftMasterList>();
		if (craftList == null)
			return;

		craftList.ConnectCraft(this);

		for (int i = 0; i < craftList.MasterCraftList.Count; i++)
		{
			if (craftList.MasterCraftList[i] == this.gameObject)
				listIndex = i;
		}
	}

	
	public void RadioHit (GameObject _hitby) {
		if (radioPower == 0)
			return;

		hitby.Add(_hitby);
		
		if (radioPower > 0 && radioPing == false)
			RadioOut();
		
		craftList.MasterCraftList[listIndex].connected = true;
		craftList.MasterCraftList[listIndex].lastConnection = 0;
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

	private void Update()
	{
		if (!controlled)
			return;
		if (Input.GetKeyDown("space"))   ///////////////////////////////////input///////////////////////////////
		{
			if (curStage == 0)
			{
				Launch();
			} 
			else
			{
				Stage();
			}
			print("cur stage: " + curStage);
		}
	}
	void FixedUpdate () {
		if (craftList == null)
		{
			craftList = FindObjectOfType<CraftMasterList>();

			if (craftList == null)
				return;

			craftList.ConnectCraft(this);

			for (int i = 0; i < craftList.MasterCraftList.Count; i++)
			{
				if (craftList.MasterCraftList[i] == this.gameObject)
					listIndex = i;
			}
		}

		radioPing = false;
		hitby.Clear();
	}

	private void Launch()
	{
		foreach (GameObject cp in craftParts)
		{
            if (cp == null)
				continue;
			if (cp.transform.root != gameObject.transform)
				Destroy(cp.transform.root.gameObject);
			if (cp.GetComponent<FuelTank>() != null)
            {
				fuelTot += cp.GetComponent<FuelTank>().fuel;
            }
			if (cp.GetComponent<StagingPart>() != null)
			{
                cp.GetComponent<StagingPart>().OnLaunch();
				stages.Add(cp.GetComponent<StagingPart>());
            }			
			if (cp.GetComponent<NozzlePart>() != null)
                cp.GetComponent<NozzlePart>().OnLaunch();
        }
        craftParts.RemoveAll(s => s == null);
        curStage++;
    }
	
	private void Stage()
	{
		if (curStage <= 10)
		{
			foreach (StagingPart sp in stages)
				sp.OnStage(curStage);
			curStage++;
        }
	}
}