using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
	public string multiplayerSceneName;

	public void GoToMultiplayer ()
	{
		SceneManager.LoadScene(multiplayerSceneName);
	}
}
