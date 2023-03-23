using System.Collections.Generic;
using UnityEngine;
using static Universe;

public class Builder : MonoBehaviour
{
	public Camera cam;
	public GameObject[] parts;
	public GameObject rootPrefab;
	public Vector3 rootPos;
	public GameObject gizmoPrefab;

	public float ghostAlpha = 0.3f;

    private GameObject root;
	private Craft rootCraft;
	private Transform selPart;
	private GameObject curPart;
	private Vector3 curPartOffset;
    private GameObject closest;
	private float closestDis;
	private bool canPlace;
    private bool ghostPartSelected;
	private bool ghostPartHit;
	private int buildMode;
	private int partIndex;

	private Ray ray;
	private RaycastHit hit;

	int layer_mask;

	void Start()
	{
        cam = Camera.main;
        layer_mask = LayerMask.GetMask("Parts");

        SpawnNewCraft();
	}
	void SpawnNewCraft()
	{
		root = Instantiate(rootPrefab, new Vector3(0,0,0), Quaternion.identity);
		root.transform.localPosition = rootPos;
        root.transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("Parts");
        rootCraft = root.GetComponent<Craft>();
        rootCraft.controlled = true;
        gizmoPrefab = Instantiate(gizmoPrefab, new Vector3(0, 0, 0), Quaternion.identity);
		partIndex = 1;
		ChangePart();
	}

	void Update()
	{
        Vector3 mousePos = GetMousePointOnPlane();
        ray = cam.ScreenPointToRay(Input.mousePosition);

        //RESET
        closest = null;
		canPlace = false;
		ghostPartHit = false;
		curPart.transform.position = new Vector3(10000, 10000, 10000);

		if (buildMode == 0 || buildMode == 2)
		{
			curPart.transform.position = mousePos;
            if (Physics.Raycast(ray, out hit, 1000, layer_mask))
            {
                if (hit.transform.parent.GetComponent<CraftPart>().notAttached)
					ghostPartHit = true;
				DistanceChecker(hit);
                if (closest != null && OffsetPart(hit.transform) != new Vector3(0, 0, 0))
				{
					curPart.transform.position = closest.transform.position + OffsetPart(hit.transform);
					if (!CollisionChecker(hit.transform.root, curPart.transform))
						canPlace = true;
				}
			}
		}

		//change this shit to the input system///////////////////////////////////////////////////////////////////////////////////
		#region Inputs
		try
		{
			buildMode = int.Parse(Input.inputString)-1;
		}
		catch
		{

		}

		if (Input.GetMouseButtonDown(0))
		{
			Clicker();
		}
		if (Input.GetKeyDown("q"))
		{
			curPart.transform.rotation *= Quaternion.Euler(0, 0, -90);
		}
		if (Input.GetKeyDown("e"))
		{
			curPart.transform.rotation *= Quaternion.Euler(0, 0, 90);

		}
		if (Input.GetKeyDown("delete"))
		{
			if (selPart != null && selPart.parent != null && selPart != root && !ghostPartSelected)
			{
				Transform newSel = selPart.parent.parent;
				GhostChildren(selPart, true);
                rootCraft.craftParts.Remove(selPart.gameObject);
                Destroy(selPart.gameObject);
				MoveGizmoToSelection(newSel);
			}
			if (ghostPartSelected)
				ChangePart();
		}

		if (Input.GetAxis("Mouse ScrollWheel") > 0f && !ghostPartSelected)
		{
			if (partIndex < parts.Length - 1)
			{
				partIndex = partIndex + 1;
			}
			else
			{
				partIndex = 0;
			}
			ChangePart();
		}
		else if (Input.GetAxis("Mouse ScrollWheel") < 0f && !ghostPartSelected)
		{
			if (partIndex > 0)
			{
				partIndex = partIndex - 1;
			}
			else
			{
				partIndex = parts.Length - 1;
			}
			ChangePart();
		}
#endregion
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	}

	private void DistanceChecker(RaycastHit hit)
	{
		CraftPart craftPart = hit.transform.parent.GetComponent<CraftPart>();
		foreach (GameObject mount in craftPart.mounts)
		{
			float testDis = Vector3.Distance(hit.point, mount.transform.position);
			if (closest == null && mount.transform.childCount == 0)
			{
				closest = mount;
				closestDis = testDis;
				continue;
			}

			if (testDis < closestDis && mount.transform.childCount == 0)
			{
				closest = mount;
				closestDis = testDis;
			}
		}
	}

	private void Clicker()
	{
		if (ghostPartSelected && closest == null)
		{
            curPart.transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("Parts");
            curPart = null;
			ChangePart();
			buildMode = 1;
			return;
		}
		switch (buildMode)
		{
			case 0: //Block Placer
				if (canPlace)
					PlacePart();
				break;

			case 1: //Block Selector
				SelectPart();
				break;

			case 2:
				break;
		}
	}

	private void ChangePart()
	{
		if (curPart != null)
			Destroy(curPart);
		curPart = Instantiate(parts[partIndex], rootPos, Quaternion.identity);
		Color c = curPart.transform.GetChild(0).GetComponent<Renderer>().material.color;
		curPart.transform.GetChild(0).GetComponent<Renderer>().material.color = new Color(c.r, c.g, c.b, ghostAlpha);
		ghostPartSelected = false;
	}

	private void PlacePart()
	{
		GameObject newPart = Instantiate(curPart, new Vector3(0, 0, 0), Quaternion.identity);
		Color c = curPart.transform.GetChild(0).GetComponent<Renderer>().material.color;
		if (!ghostPartHit)
		{
			curPart.transform.GetChild(0).GetComponent<Renderer>().material.color = new Color(c.r, c.g, c.b, 1f);
		} else {
			newPart.transform.GetComponent<CraftPart>().notAttached = true;
			curPart.transform.GetChild(0).GetComponent<Renderer>().material.color = new Color(c.r, c.g, c.b, ghostAlpha);
		}
		newPart.transform.SetParent(closest.transform);
		newPart.transform.position = curPart.transform.position;
		newPart.transform.localEulerAngles = curPart.transform.localEulerAngles;
		newPart.transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("Parts");
		if (ghostPartSelected)
		{
			UnGhostinate(newPart.transform);
			ChangePart();
			buildMode = 1;
		}
		else
		{
            rootCraft.craftParts.Add (newPart);
        }
		MoveGizmoToSelection(newPart.transform);
    }

	private void SelectPart()
	{
		if (Physics.Raycast(ray, out hit, 1000, layer_mask))
		{
			MoveGizmoToSelection(hit.transform.parent);
			if (hit.transform.parent.parent == null)
			{
				buildMode = 0;
				ghostPartSelected = true;
				gizmoPrefab.transform.position = new Vector3(10000, 10000, 10000);
				if (curPart != null)
					Destroy(curPart);
				curPart = hit.transform.parent.gameObject;
                curPart.transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("Default");
            }
		}
	}

	private void MoveGizmoToSelection(Transform partSelection)
	{
		selPart = partSelection;
		gizmoPrefab.transform.position = selPart.position;
	}

	private void GhostChildren(Transform part, bool unParent)
	{
		CraftPart craftPart = part.GetComponent<CraftPart>();
		Color c = part.transform.GetChild(0).GetComponent<Renderer>().material.color;
        part.transform.GetChild(0).GetComponent<Renderer>().material.color = new Color(c.r, c.g, c.b, ghostAlpha);
		craftPart.notAttached = true;
		foreach (GameObject mount in craftPart.mounts)
		{
			if (mount.transform.childCount == 0)
				continue;
			Transform mounted = mount.transform.GetChild(0);
			if (unParent)
				mounted.SetParent(null);
            GhostChildren(mounted.transform, false);
		}
	}

	private void UnGhostinate(Transform part)
	{
		CraftPart craftPart = part.GetComponent<CraftPart>();
		Color c = part.transform.GetChild(0).GetComponent<Renderer>().material.color;
        part.transform.GetChild(0).GetComponent<Renderer>().material.color = new Color(c.r, c.g, c.b, 1f);
		craftPart.notAttached = false;
		foreach (GameObject mount in craftPart.mounts)
		{
			if (mount.transform.childCount == 0)
				continue;
			Transform mounted = mount.transform.GetChild(0);
            UnGhostinate(mounted.transform);
		}
	}
	private bool CollisionChecker(Transform hit, Transform part)
	{
		Collider col = hit.transform.GetChild(0).GetComponent<Collider>();
		if (col != null)
		{
		if  (SubCollisionCheck(col, part))
			return true;
		}

		CraftPart partPart = hit.GetComponent<CraftPart>();
		foreach (GameObject np in partPart.mounts)
		{
			if (np.transform.childCount == 0)
				continue;
			Transform nextHit = np.transform.GetChild(0);
			if (CollisionChecker(nextHit, part))
				return true;
		}

		return false;
	}	
	
	private bool SubCollisionCheck(Collider hit, Transform part)
	{
		Collider col = part.transform.GetChild(0).GetComponent<Collider>();
		if (col != null)
		{
			if (hit.bounds.Intersects(col.bounds))
				return true;
		}

		CraftPart partPart = part.GetComponent<CraftPart>();
		foreach (GameObject np in partPart.mounts)
		{
			if (np.transform.childCount == 0)
				continue;
			Transform nextPart = np.transform.GetChild(0);
			if (SubCollisionCheck(hit, nextPart))
				return true;
		}

		return false;
	}

	private Vector3 OffsetPart(Transform checkHit)
	{
		CraftPart craftPart = curPart.transform.GetComponent<CraftPart>();
		foreach (GameObject mount in craftPart.mounts)
		{
			if ((mount.transform.position - checkHit.transform.position).magnitude <= 0.5f)
				return -mount.transform.localPosition;
        }
        return new Vector3(0, 0, 0);
	}
}
