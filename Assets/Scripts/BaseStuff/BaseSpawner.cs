using Fusion;
using UnityEngine;
using static Universe;

public class BaseSpawner : NetworkTransform {
	
	public GameObject planet;
	public Transform debugPoint;

	private Transform pieSlice;
	private bool placed;
	private Vector3 workerVec;

	private bool visible;

	[Networked]
	public bool Visible
	{
		get { return visible; }
		set { visible = value; }
	}

    private void Start()
	{
		planet = GameObject.FindWithTag("Terrain");
		transform.parent = planet.transform;
		pieSlice = transform.parent;
	}

	private void Update()
	{
		if (!HasInputAuthority)
		{
			return;
		}

		if (!placed)
		{
			workerVec = GetMousePointOnPlanet();
			debugPoint.position = workerVec;

			transform.position = workerVec;
			transform.LookAt(transform.position * 2f);

			print(Vector3.SignedAngle(workerVec.normalized, pieSlice.forward, Vector3.forward));

			if (Vector3.SignedAngle(workerVec.normalized, pieSlice.forward, Vector3.forward) > 1 && Vector3.SignedAngle(workerVec.normalized, pieSlice.forward, Vector3.forward) < 44)
			{
				WritePosition(transform.position);
				WriteRotation(transform.rotation);
				Visible = true;
			}
			else
			{
				Visible = false;
			}
			foreach (MeshRenderer m in GetComponentsInChildren<MeshRenderer>())
			{
				m.enabled = Visible;
			}
		}
	}

	public override void FixedUpdateNetwork() {
		if (!placed)
		{
			foreach (MeshRenderer m in GetComponentsInChildren<MeshRenderer>())
			{
				m.enabled = Visible;
			}
		}
	}
}
