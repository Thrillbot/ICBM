using Fusion;
using UnityEngine;
using UnityEngine.UIElements;

public class BaseSpawner : NetworkTransform {
	
	public GameObject planet;

	private Vector3 origin;
	private bool placed;

    private void Start()
	{
		origin = transform.position;
		planet = GameObject.FindWithTag("Terrain");
		transform.parent = planet.transform;
    }

    public override void FixedUpdateNetwork() {
		if (!HasInputAuthority)
		{
			return;
		}

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
				try
				{
					if (Physics.Linecast(ray, planet.transform.position, out hit))
					{
						RpcBasePosition(hit.point - new Vector3(0, 0, hit.point.z));

						if (Input.GetButton("Fire1"))
						{
							placed = true;
						}
					}
				}
				catch
				{

				}
			}
		}
	}

    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
    public void RpcBasePosition(Vector3 value)
    {
        transform.position = value;
        transform.LookAt(transform.position * 2f);
    }
}
