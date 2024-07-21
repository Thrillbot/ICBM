using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
	public string multiplayerSceneName;

	public GameObject mainMenuObject;
	public GameObject settingsObject;

	public Transform cameraTransform;
	public Transform[] cameraWaypoints;
	public float cameraLerpRate;
	public AnimationCurve cameraLerpCurve;

	private Vector3 cameraStart;
	private Quaternion cameraStartRotation;
	private float timer;
	private IEnumerator lerpRoutine;

	public void GoToMultiplayer ()
	{
		SceneManager.LoadScene(multiplayerSceneName);
	}

	public void GoToMainMenu()
	{
		mainMenuObject.SetActive(true);
		settingsObject.SetActive(false);

		if (lerpRoutine != null) StopCoroutine(lerpRoutine);
		lerpRoutine = LerpCamera(0);
		StartCoroutine(lerpRoutine);
	}

	public void GoToSettings()
	{
		mainMenuObject.SetActive(false);
		settingsObject.SetActive(true);

		if (lerpRoutine != null) StopCoroutine(lerpRoutine);
		lerpRoutine = LerpCamera(1);
		StartCoroutine(lerpRoutine);
	}

	IEnumerator LerpCamera (int index)
	{
		//cameraWaypoints
		timer = 0;
		cameraStart = cameraTransform.position;
		cameraStartRotation = cameraTransform.rotation;
		while (timer < 1)
		{
			timer += Time.deltaTime / cameraLerpRate;
			cameraTransform.position = Vector3.Lerp(cameraStart, cameraWaypoints[index].position, cameraLerpCurve.Evaluate(timer));
			cameraTransform.rotation = Quaternion.Lerp(cameraStartRotation, cameraWaypoints[index].rotation, cameraLerpCurve.Evaluate(timer));
			yield return null;
		}

		cameraTransform.position = cameraWaypoints[index].position;
		cameraTransform.rotation = cameraWaypoints[index].rotation;
	}

	public void Exit ()
	{
		Application.Quit();
	}
}
