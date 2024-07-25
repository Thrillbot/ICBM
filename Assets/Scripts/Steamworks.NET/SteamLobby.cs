using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using Steamworks;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class SteamLobby : MonoBehaviour
{
	//public string multiplayerSceneName = "Lobby";
	public NetworkManager networkManager;
	public GameObject lobbyUI;
	public GameObject preGameUI;
	public GameObject launchGameButton;
	public string worldSceneName;
	public GameObject networkCanvas;
	public ulong currentLobbyID;

	protected Callback<LobbyCreated_t> lobbyCreated;
	protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
	protected Callback<LobbyEnter_t> gameLobbyEntered;

	private const string HostAddressKey = "HostAddress";
	private static SteamLobby _instance;
	private List<string> playerList;

	public static SteamLobby Instance
	{
		get { return _instance; }
	}

	private void Start()
	{
		if (_instance == null)
			_instance = this;

		SceneManager.sceneLoaded += OnSceneLoaded;
		StartCoroutine(WhileNotInitialized());
	}

	/*
	void Update ()
	{
		playerListText.text = "";
		foreach (LobbyPlayer p in FindObjectsOfType<LobbyPlayer>())
		{
			Debug.Log("Adding Player Name: " + p.playerName);
			playerListText.text += p.playerName + '\n';
		}
	}
	*/

	IEnumerator WhileNotInitialized ()
	{
		while (!SteamManager.Initialized)
		{
			yield return null;
		}

		lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
		gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
		gameLobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
	}

	private void OnLobbyCreated(LobbyCreated_t callback)
	{
		if (callback.m_eResult != EResult.k_EResultOK)
		{
			networkCanvas.SetActive(true);
			lobbyUI.SetActive(true);
			preGameUI.SetActive(false);
			return;
		}

		CSteamID lobbyId = new CSteamID(callback.m_ulSteamIDLobby);
		currentLobbyID = lobbyId.m_SteamID;

		networkManager.StartHost();
		SteamMatchmaking.SetLobbyData(lobbyId, HostAddressKey, SteamUser.GetSteamID().ToString());
	}

	private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
	{
		SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
	}

	private void OnLobbyEntered(LobbyEnter_t callback)
	{
		string hostAddress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey);

		networkManager.networkAddress = hostAddress;
		networkManager.StartClient();

		//SceneManager.LoadScene(multiplayerSceneName);
		networkCanvas.SetActive(true);
		lobbyUI.SetActive(false);
		preGameUI.SetActive(true);
	}

	public void Host ()
	{
		if (!SteamManager.Initialized)
			return;

		lobbyUI.SetActive(false);
		preGameUI.SetActive(true);
		launchGameButton.SetActive(true);
		SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, networkManager.maxConnections);
	}

	public void StartGame ()
	{
		if (!SteamManager.Initialized)
			return;

		if (!NetworkServer.active)
			return;

		lobbyUI.SetActive(false);
		preGameUI.SetActive(false);

		networkManager.ServerChangeScene(worldSceneName);
	}

	void OnSceneLoaded (Scene scene, LoadSceneMode mode)
	{
		/*
		if (scene.name == multiplayerSceneName)
		{
			networkCanvas.SetActive(true);
		}
		*/
		if (scene.name == worldSceneName)
		{
			lobbyUI.SetActive(false);
			preGameUI.SetActive(false);
		}
	}

	public void AddNameToList (string name)
	{
		Debug.Log("Adding Player To List");
		playerList ??= new();

		if (playerList.Contains(name)) return;
		Debug.Log("Confirmed, Unique Player");
		playerList.Add(name);

		/*
		Debug.Log("Updating Player List Text");
		playerListText.text = "";
		foreach (string s in playerList)
		{
			Debug.Log("Adding Player Name: " + s);
			playerListText.text += s + '\n';
		}
		*/
	}
}