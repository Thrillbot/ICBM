using UnityEngine;

public class PlanetSpin : MonoBehaviour
{
	public GameManager gameManager;
	public Vector3 axis;

	void LateUpdate()
	{
		transform.localEulerAngles = gameManager.GameTime * 360f * axis.normalized;
	}
}
