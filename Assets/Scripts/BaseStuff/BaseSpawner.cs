using Fusion;
using UnityEngine;

public class BaseSpawner : NetworkTransform {
	
	public GameObject planet;

	private Vector3 origin;
	private bool placed;

	private void Start()
	{
		origin = transform.position;
		planet = GameObject.FindWithTag("Terrain");
	}

	void Update() {
		if (!HasStateAuthority)
			return;

		if (!placed)
		{
			RaycastHit hit;
			Vector3 ray = Input.mousePosition;
			ray.y = 1000;
			ray.z = Camera.main.transform.position.z * 1.5f;
			ray = Camera.main.ScreenToWorldPoint(ray);
			ray.z = -1;

			print(Vector3.Angle(ray.normalized, origin.normalized));
			if (Vector3.Angle(ray.normalized, origin.normalized) < 22)
			{
				if (Physics.Linecast(ray, planet.transform.position, out hit))
				{
					transform.position = hit.point - new Vector3(0, 0, hit.point.z);
					transform.LookAt(transform.position * 2f);

					if (Input.GetButton("Fire1"))
					{
						placed = true;
					}
				}
			}
		}
	}
}
