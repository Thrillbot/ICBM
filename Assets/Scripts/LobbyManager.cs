using Mirror;
using System.Collections.Generic;
using TMPro;
using Steamworks;
using UnityEngine;

public class LobbyManager : NetworkBehaviour
{
	public TMP_Text playerListText;

	[SyncVar(hook = "UpdatePlayerList")]
	public List<LobbyPlayer> playerList;

	[SyncVar(hook = "AddToPlayerNames")]
	public List<string> playerNames;

	[SyncVar(hook = "SteamIDify")]
	public List<ulong> steamIDs;

	public void SteamNicknameGetter(CSteamID steamID)
	{
		bool needsToRetreiveInformationFromInternet = SteamFriends.RequestUserInformation(steamID, true);
		if (!needsToRetreiveInformationFromInternet)
		{
			List<string> tempNames = playerNames;
			if (tempNames.Contains(SteamFriends.GetFriendPersonaName(steamID))) return;
			tempNames.Add(SteamFriends.GetFriendPersonaName(steamID));
			AddToPlayerNames(null, tempNames);
		}
	}

	private void Update()
	{
		playerNames ??= new();
		for (int i = 0; i < playerList.Count; i++)
		{
			Debug.Log(SteamMatchmaking.GetLobbyMemberByIndex((CSteamID)SteamLobby.Instance.currentLobbyID, i).GetAccountID().m_AccountID);
			SteamNicknameGetter(SteamMatchmaking.GetLobbyMemberByIndex((CSteamID)SteamLobby.Instance.currentLobbyID, i));
		}

		playerListText.text = "";
		foreach (string s in playerNames)
		{
			playerListText.text += s + '\n';
		}

		/*
		playerList ??= new();

		playerListText.text = "";
		foreach (LobbyPlayer p in playerList)
		{
			playerListText.text += p.playerName + '\n';
		}
		*/
	}

	public void AddSelfToLobby (LobbyPlayer player)
	{
		playerList ??= new();

		if (!playerList.Contains(player))
		{
			playerList.Add(player);
			UpdatePlayerList(null, playerList);
		}
	}

	void UpdatePlayerList(List<LobbyPlayer> oldValue, List<LobbyPlayer> newValue)
	{
		playerList = newValue;
	}

	void AddToPlayerNames(List<string> oldValue, List<string> newValue)
	{
		playerNames = newValue;
	}

	void SteamIDify(List<ulong> oldValue, List<ulong> newValue)
	{
		steamIDs = newValue;
	}
}
