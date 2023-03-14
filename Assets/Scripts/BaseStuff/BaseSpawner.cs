using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseSpawner : NetworkTransform {
	
	public GameObject planet;
	public LineRenderer line;

	private bool placed;

	private void Start()
	{
		planet = GameObject.FindWithTag("Terrain");
	}

	void Update() {
		if (!HasStateAuthority)
			return;

		if (!placed)
		{
			RaycastHit hit;
			Vector3 ray = Input.mousePosition;
			ray.z = 10;
			ray = Camera.main.ScreenToWorldPoint(ray);
			ray.z = -1;
			line.SetPosition(0, ray);

			if (Physics.Linecast(ray, planet.transform.position, out hit))
			{
				transform.position = hit.point - new Vector3(0, 0, hit.point.z);
				transform.LookAt(transform.position * 2f);
				line.SetPosition(0, hit.point - new Vector3(0, 0, hit.point.z));

				if (Input.GetButton("Fire1"))
				{
					placed = true;
				}
			}
		}
	}
}
