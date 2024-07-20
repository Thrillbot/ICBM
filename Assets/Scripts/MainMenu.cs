using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
	public string multiplayerSceneName;

	public GameObject mainMenuObject;
	public GameObject settingsObject;

	public void GoToMultiplayer ()
	{
		SceneManager.LoadScene(multiplayerSceneName);
	}

	public void GoToMainMenu()
	{
		mainMenuObject.SetActive(true);
		settingsObject.SetActive(false);
	}

	public void GoToSettings()
	{
		mainMenuObject.SetActive(false);
		settingsObject.SetActive(true);
	}
}
