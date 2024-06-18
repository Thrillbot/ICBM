using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunAim : MonoBehaviour
{

	public Transform debugObject;
	public GameObject crossHair;
	public ParticleSystem bullets;
	public float speed;

	public Vector3 workerVec;

	void Update() {
		//if (!HasStateAuthority)
		//	return;

		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

		if (Physics.Raycast(ray, out hit))
		{
			transform.up = Vector3.Lerp(transform.up, (hit.point - transform.position), speed * 0.0001f);
		}
		if (Input.GetMouseButtonDown(0))
		{
			RPC_Fire(true);
		}
		if (Input.GetMouseButtonUp(0))
		{
			RPC_Fire(false);
		}
	}

	//[Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
	public void RPC_Fire(bool firing)
	{
		if (firing)
		{
			bullets.Play();
		}
		else
		{
			bullets.Stop();
		}
	}
}
