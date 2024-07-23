using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
	public string multiplayerSceneName;

	public GameObject loadingScreen;
	public GameObject mainMenuObject;
	public GameObject settingsObject;

	public Transform cameraTransform;
	public Transform[] cameraWaypoints;
	public float cameraLerpRate;
	public AnimationCurve cameraLerpCurve;

	private Vector3 cameraStart;
	private Quaternion cameraStartRotation;
	private float timer;
	private int waypointIndex;

	private void Start()
	{
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
		if (timer < 1 - Time.deltaTime / cameraLerpRate)
			timer += Time.deltaTime / cameraLerpRate;
		else
			timer = 1;
		cameraTransform.position = Vector3.Lerp(cameraStart, cameraWaypoints[waypointIndex].position, cameraLerpCurve.Evaluate(timer));
		cameraTransform.rotation = Quaternion.Lerp(cameraStartRotation, cameraWaypoints[waypointIndex].rotation, cameraLerpCurve.Evaluate(timer));
	}

	public void GoToMultiplayer ()
	{
		SceneManager.LoadScene(multiplayerSceneName);
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
