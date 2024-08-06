using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
	public string menuSceneName;
	public GameObject mainMenuCanvas;
	public GameObject networkCanvas;

	public GameObject loadingScreen;
	public GameObject mainMenuObject;
	public GameObject settingsObject;

	public Transform cameraTransform;
	public Transform[] cameraWaypoints;

	public Transform mainCameraTransform;
	public Transform[] mainCameraWaypoints;

	public float cameraLerpRate;
	public AnimationCurve cameraLerpCurve;

	public float mainCameraLerpRate;
	public AnimationCurve mainCameraLerpCurve;

	private Vector3 cameraStart;
	private Quaternion cameraStartRotation;

	private Vector3 mainCameraStart;
	private Quaternion mainCameraStartRotation;

	private float timer;
	private float mainTimer;

	private int waypointIndex;
	private int mainCameraWaypointIndex;

	private void Start()
	{
		cameraStart = cameraTransform.position;
		cameraStartRotation = cameraTransform.rotation;

		mainCameraStart = mainCameraTransform.position;
		mainCameraStartRotation = mainCameraTransform.rotation;

		GoToMainMenu();

		StartCoroutine(Initialize());
	}

	IEnumerator Initialize ()
	{
		yield return null;

		loadingScreen.SetActive(false);
	}

	private void Update()
	{
		if (SceneManager.GetActiveScene().name != menuSceneName) return;

		if (timer < 1 - Time.deltaTime / cameraLerpRate)
			timer += Time.deltaTime / cameraLerpRate;
		else
			timer = 1;

		if (mainTimer < 1 - Time.deltaTime / mainCameraLerpRate)
			mainTimer += Time.deltaTime / mainCameraLerpRate;
		else
			mainTimer = 1;

		mainCameraTransform.position = Vector3.Lerp(mainCameraStart, mainCameraWaypoints[mainCameraWaypointIndex].position, mainCameraLerpCurve.Evaluate(mainTimer));
		mainCameraTransform.rotation = Quaternion.Lerp(mainCameraStartRotation, mainCameraWaypoints[mainCameraWaypointIndex].rotation, mainCameraLerpCurve.Evaluate(mainTimer));
		
		cameraTransform.position = Vector3.Lerp(cameraStart, cameraWaypoints[waypointIndex].position, cameraLerpCurve.Evaluate(timer));
		cameraTransform.rotation = Quaternion.Lerp(cameraStartRotation, cameraWaypoints[waypointIndex].rotation, cameraLerpCurve.Evaluate(timer));
		
		if (mainCameraWaypointIndex == 1 && mainTimer == 1)
		{
			networkCanvas.SetActive(true);
		}
	}

	public void GoToMultiplayer ()
	{
		mainCameraStart = mainCameraTransform.position;
		mainCameraStartRotation = mainCameraTransform.rotation;
		mainCameraWaypointIndex = 1;
		mainTimer = 0;

		mainMenuCanvas.SetActive(false);
	}

	public void LeaveMultiplayer ()
	{
		mainCameraStart = mainCameraTransform.position;
		mainCameraStartRotation = mainCameraTransform.rotation;
		mainCameraWaypointIndex = 0;
		mainTimer = 0;

		mainMenuCanvas.SetActive(false);
		networkCanvas.SetActive(true);
	}

	public void GoToMainMenu()
	{
		mainMenuObject.SetActive(true);
		settingsObject.SetActive(false);

		cameraStart = cameraTransform.position;
		cameraStartRotation = cameraTransform.rotation;
		waypointIndex = 0;
		timer = 0;
	}

	public void GoToSettings()
	{
		mainMenuObject.SetActive(false);
		settingsObject.SetActive(true);

		cameraStart = cameraTransform.position;
		cameraStartRotation = cameraTransform.rotation;
		waypointIndex = 1;
		timer = 0;
	}

	public void Exit ()
	{
		Application.Quit();
	}
}
