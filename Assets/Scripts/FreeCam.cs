using UnityEngine;
using Rewired;

public class FreeCam : MonoBehaviour
{
	public Transform cameraTransform;
	public float sensitivity;
	public float shiftModifier = 4f;
	public float zoomSensitivity;
	public Vector3 axis;
	public Vector3 lowerLimits = new Vector3(-200, 0, 5);
	public Vector3 upperLimits = new Vector3(-106, 0, 45);

	public Vector3 buildLowerLimits = new Vector3(-106, 0, 2);
	public Vector3 buildUpperLimits = new Vector3(-100, 0, 7);

	public AnimationCurve zoomHeight;

	public Transform target;

	private Vector3 workerVec;
	private bool buildMode;

	private Player player;
	private void Awake()
	{
		// Get the Rewired Player object for this player and keep it for the duration of the character's lifetime
		player = ReInput.players.GetPlayer(0);
	}

	void Update()
	{
		if (Universe.paused || Universe.loading)
			return;

		transform.localEulerAngles += axis * -player.GetAxis("Horizontal") * Time.deltaTime * sensitivity * (player.GetButton("Sprint") ? shiftModifier : 1);

		workerVec = cameraTransform.localPosition;
		workerVec.z += player.GetAxis("Vertical") * Time.deltaTime * sensitivity;
		workerVec.y -= player.GetAxis("Zoom") * Time.deltaTime * zoomSensitivity;
		workerVec.z -= (player.GetAxis("Zoom") > 0 ? player.GetAxis("Zoom") : 0) * Time.deltaTime * zoomSensitivity;

		workerVec.x = 0;
		if (buildMode)
		{
			workerVec.y = Mathf.Clamp(workerVec.y, Mathf.Lerp(buildLowerLimits.x, buildUpperLimits.x, zoomHeight.Evaluate((workerVec.z - buildLowerLimits.z) / (buildUpperLimits.z - buildLowerLimits.z))), buildUpperLimits.x);
			workerVec.z = Mathf.Clamp(workerVec.z, buildLowerLimits.z, buildUpperLimits.z);
		}
		else
		{
			workerVec.y = Mathf.Clamp(workerVec.y, Mathf.Lerp(lowerLimits.x, upperLimits.x, zoomHeight.Evaluate((workerVec.z - lowerLimits.z) / (upperLimits.z - lowerLimits.z))), upperLimits.x);
			workerVec.z = Mathf.Clamp(workerVec.z, lowerLimits.z, upperLimits.z);
		}
		cameraTransform.localPosition = workerVec;

		if (target)
		{
			transform.LookAt(target);
			transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);

			workerVec = cameraTransform.localPosition;
			workerVec.z = target.position.magnitude + workerVec.y * 0.25f;
			cameraTransform.localPosition = workerVec;
		}
	}

	public bool BuildMode
	{
		get { return buildMode; }
		set { buildMode = value; }
	}

	public void Focus(Transform focusTarget, float distance = -1)
	{
		transform.LookAt(focusTarget);
		transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);

		if (distance != -1)
		{
			workerVec = cameraTransform.localPosition;
			workerVec.z = focusTarget.position.magnitude + distance;
			cameraTransform.localPosition = workerVec;
		}
	}
}
