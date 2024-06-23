using UnityEngine;
using Mirror;
using Steamworks;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class SteamLobby : MonoBehaviour
{
	public NetworkManager networkManager;
	public GameObject lobbyUI;
	public GameObject preGameUI;
	public string worldSceneName;
	public TMP_Text playerListText;

	protected Callback<LobbyCreated_t> lobbyCreated;
	protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
	protected Callback<LobbyEnter_t> gameLobbyEntered;

	private const string HostAddressKey = "HostAddress";

	private List<string> playerList;

	private void Start()
	{
		StartCoroutine(WhileNotInitialized());
	}

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
			lobbyUI.SetActive(true);
			preGameUI.SetActive(false);
			return;
		}

		networkManager.StartHost();
		SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey, SteamUser.GetSteamID().ToString());
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

		lobbyUI.SetActive(false);
		preGameUI.SetActive(true);
	}

	public void Host ()
	{
		if (!SteamManager.Initialized)
			return;

		lobbyUI.SetActive(false);
		preGameUI.SetActive(true);
		SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, networkManager.maxConnections);
	}

	public void StartGame ()
	{
		if (!SteamManager.Initialized)
			return;

		if (!NetworkServer.active)
			return;

		networkManager.ServerChangeScene(worldSceneName);
	}
}
