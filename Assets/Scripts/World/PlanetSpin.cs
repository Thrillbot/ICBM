using UnityEngine;

public class PlanetSpin : MonoBehaviour
{
	public GameManager gameManager;
	public Vector3 axis;

	void LateUpdate()
	{
		if (gameManager)
			transform.localEulerAngles = gameManager.GameTime * 360f * axis.normalized;
		else
			transform.localEulerAngles += (Time.deltaTime / 3600) * 360f * axis.normalized;
	}
}
