using TMPro;
using UnityEngine;
using Mirror;
using Steamworks;
using System.Collections.Generic;

public class BasicNetManager : NetworkManager
{
	[Header("Bespoke Stuff")]
	public string worldSceneName;
	public GameObject basePrefab;
	public PlayerSeat[] playerSeatPositions;

	List<LobbyPlayer> players;
	List<PlayerConn> playerConns;
	protected Callback<PersonaStateChange_t> PersonaStateChangeCallback;

	public struct PlayerConn
	{
		public CSteamID steamID;
		public NetworkConnectionToClient conn;
		public string playerName;
	}

	[System.Serializable]
	public struct PlayerSeat
	{
		public Transform seat;
		public LobbyPlayer player;
	}

	public override void Update()
	{
		base.Update();

		SteamAPI.RunCallbacks();
	}

	private void OnPersonaStateChangeHandler(PersonaStateChange_t PersonaStateChange)
	{
		foreach (LobbyPlayer player in players)
		{
			Debug.Log("Checking Persona Callback for ID: " + player.steamID.m_SteamID);
			Debug.Log(PersonaStateChange.m_ulSteamID + " | " + player.steamID.m_SteamID);
			if (PersonaStateChange.m_ulSteamID == player.steamID.m_SteamID)
			{
				player.SetName("", SteamFriends.GetFriendPersonaName(player.steamID));
				Debug.Log("Steam Name: " + player.name);
				FindObjectOfType<SteamLobby>().AddNameToList(player.name);

				for (int i = 0; i < playerConns.Count; i++)
				{
					if (playerConns[i].steamID == player.steamID)
					{
						PlayerConn tempConn = playerConns[i];
						tempConn.playerName = player.name;
						playerConns[i] = tempConn;
					}
				}
			}
		}
	}

	/// <summary>
	/// Called on the server when a client adds a new player with NetworkClient.AddPlayer.
	/// <para>The default implementation for this function creates a new player object from the playerPrefab.</para>
	/// </summary>
	/// <param name="conn">Connection from client.</param>
	public override void OnServerAddPlayer(NetworkConnectionToClient conn)
	{
		players ??= new();
		//base.OnServerAddPlayer(conn);

		GameObject player = Instantiate(playerPrefab);
		player.name = "Joining...";
		LobbyPlayer lobbyPlayer = player.GetComponent<LobbyPlayer>();
		//NetworkServer.Spawn(player, conn);

		CSteamID steamId = SteamMatchmaking.GetLobbyMemberByIndex(
		new CSteamID(FindObjectOfType<SteamLobby>().currentLobbyID),
		players.Count
		);
		lobbyPlayer.SetSteamID(new(), steamId);

		PersonaStateChangeCallback = Callback<PersonaStateChange_t>.Create(OnPersonaStateChangeHandler);

		playerConns ??= new();
		PlayerConn tempPConn = new()
		{
			steamID = lobbyPlayer.steamID,
			conn = conn
		};

		bool needsToRetreiveInformationFromInternet = SteamFriends.RequestUserInformation(lobbyPlayer.steamID, true);
		if (!needsToRetreiveInformationFromInternet)
		{
			player.name = SteamFriends.GetFriendPersonaName(lobbyPlayer.steamID);
			lobbyPlayer.SetName("", player.name);
			Debug.Log("Steam Name: " + player.name);
			FindObjectOfType<SteamLobby>().AddNameToList(player.name);

			tempPConn.playerName = player.name;

		}
		playerConns.Add(tempPConn);

		//player.name = $"{SteamFriends.GetFriendPersonaName(steamId)}";
		for (int i = 0; i < playerSeatPositions.Length; i++)
		{
			if (playerSeatPositions[i].player == null)
			{
				player.transform.position = playerSeatPositions[i].seat.position;
				player.transform.rotation = playerSeatPositions[i].seat.rotation;
				playerSeatPositions[i].player = player.GetComponent<LobbyPlayer>();
				break;
			}
		}
		players.Add(lobbyPlayer);

		Debug.Log("Initializing Player: " + conn.connectionId);
		while (!conn.isReady)
		{

		}
		NetworkServer.AddPlayerForConnection(conn, player);

		//Player.ResetPlayerNumbers();
	}

    /// <summary>
    /// Called on the server when a client disconnects.
    /// <para>This is called on the Server when a Client disconnects from the Server. Use an override to decide what should happen when a disconnection is detected.</para>
    /// </summary>
    /// <param name="conn">Connection from client.</param>
    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnServerDisconnect(conn);
        //Player.ResetPlayerNumbers();
    }

	public override void OnServerSceneChanged(string sceneName)
	{
		base.OnServerSceneChanged(sceneName);

		if (sceneName != worldSceneName)
			return;

		foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values)
		{
			string playerName = "";

			foreach (PlayerConn p in playerConns)
			{
				if (p.conn == conn)
				{
					playerName = p.playerName;
				}
			}

			Transform startPos = GetStartPosition();
			GameObject player = Instantiate(basePrefab, startPos.position, startPos.rotation);
			//player.GetComponent<NetworkIdentity>().AssignClientAuthority(conn);
			NetworkServer.Spawn(player, conn);

			player.name = "Base - " + playerName;
			player.GetComponent<BaseSpawner>().playerName = playerName;

			// instantiating a "Player" prefab gives it the name "Player(clone)"
			// => appending the connectionId is WAY more useful for debugging!

			Debug.Log("Initializing Player: " + conn.connectionId);
			//while (!conn.isReady)
			//{
			//	yield return null;
			//}
			NetworkServer.AddPlayerForConnection(conn, player);
			//player.GetComponent<NetworkIdentity>().AssignClientAuthority(conn);
			player.SendMessage("Initialize", startPos.GetComponent<NetworkIdentity>().netId);
		}
	}

	public override Transform GetStartPosition()
	{
		// first remove any dead transforms
		startPositions.RemoveAll(t => t == null);

		if (startPositions.Count == 0)
			return null;

        int index = Random.Range(0, startPositions.Count);
		Transform startPos = startPositions[index];
        startPositions.RemoveAt(index);

		return startPos;
	}

	public void AddPlayerToList(System.Func<string> getPersonaName)
	{
		throw new System.NotImplementedException();
	}
}