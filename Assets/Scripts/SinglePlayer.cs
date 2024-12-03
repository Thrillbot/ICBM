using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SinglePlayer : MonoBehaviour
{
	[SerializeField]
	private string singlePlayerSceneName;
	[SerializeField]
	private GameObject basePrefab;
	[SerializeField]
	private GameObject mainMenuCanvas;

	public void StartSinglePlayer ()
	{
		mainMenuCanvas.SetActive(false);
		SceneManager.LoadScene(singlePlayerSceneName);
		StartCoroutine(WaitForAllLoaded());
	}

	IEnumerator WaitForAllLoaded()
	{

		yield return null;
		mainMenuCanvas.SetActive(false);
		yield return null;

		string playerName = "";
		Texture2D playerIcon = null;
		Color playerColor = Color.white;

		GameObject player = Instantiate(basePrefab);

		player.name = "Base - " + playerName;
		player.GetComponent<BaseSinglePlayer>().playerName = playerName;
		player.GetComponent<BaseSinglePlayer>().playerIcon = playerIcon;

		player.SendMessage("Initialize", new BaseSinglePlayer.InitArgs { playerIndex = 0, playerColor = playerColor });
	}
}
