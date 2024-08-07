using UnityEngine;
using Mirror;
using Steamworks;
using System.Collections.Generic;
using System.Collections;

public class BasicNetManager : NetworkManager
{
	[Header("Bespoke Stuff")]
	public string worldSceneName;
	public GameObject basePrefab;
	public PlayerSeat[] playerSeatPositions;
	public List<Color> playerColors;

	List<LobbyPlayer> players;
	List<PlayerConn> playerConns;
	protected Callback<PersonaStateChange_t> PersonaStateChangeCallback;

	public struct PlayerConn
	{
		public CSteamID steamID;
		public NetworkConnectionToClient conn;
		public string playerName;
		public Color playerColor;
		public Texture2D playerIcon;
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
			if (player == null)
			{
				continue;
			}
			//Debug.Log("Checking Persona Callback for ID: " + player.steamID.m_SteamID);
			//Debug.Log(PersonaStateChange.m_ulSteamID + " | " + player.steamID.m_SteamID);
			if (PersonaStateChange.m_ulSteamID == player.steamID.m_SteamID)
			{
				player.SetName("", SteamFriends.GetFriendPersonaName(player.steamID));

				Texture2D playerIcon = null;
				if (SteamUtils.GetImageSize(SteamFriends.GetLargeFriendAvatar(player.steamID), out uint width, out uint height))
				{
					byte[] image = new byte[width * height * 4];
					if (SteamUtils.GetImageRGBA(SteamFriends.GetLargeFriendAvatar(player.steamID), image, (int)(width * height * 4)))
					{
						playerIcon = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false, true);
						playerIcon.LoadRawTextureData(image);
						playerIcon.Apply();
					}
					player.RPCApplyIcon(SteamFriends.GetLargeFriendAvatar(player.steamID), (int)width, (int)height);
				}

				//Debug.Log("Steam Name: " + player.name);
				FindObjectOfType<SteamLobby>().AddNameToList(player.name);

				for (int i = 0; i < playerConns.Count; i++)
				{
					if (playerConns[i].steamID == player.steamID)
					{
						PlayerConn tempConn = playerConns[i];
						tempConn.playerName = player.name;
						tempConn.playerIcon = playerIcon;
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
		playerConns ??= new();

		foreach (PlayerConn p in playerConns)
		{
			if (p.conn == conn) return;
		}

		Color chosenColor = Color.white;
		if (playerColors.Count > 0)
		{
			chosenColor = playerColors[0];
			playerColors.RemoveAt(0);
		}

		GameObject player = Instantiate(playerPrefab);
		player.name = "Joining...";
		LobbyPlayer lobbyPlayer = player.GetComponent<LobbyPlayer>();
		NetworkServer.Spawn(player, conn);

		CSteamID steamId = SteamMatchmaking.GetLobbyMemberByIndex(
		new CSteamID(FindObjectOfType<SteamLobby>().currentLobbyID),
		players.Count
		);
		lobbyPlayer.SetSteamID(new(), steamId);
		lobbyPlayer.playerColor = chosenColor;

		PersonaStateChangeCallback = Callback<PersonaStateChange_t>.Create(OnPersonaStateChangeHandler);

		PlayerConn tempPConn = new()
		{
			steamID = lobbyPlayer.steamID,
			conn = conn,
			playerColor = chosenColor
		};

		bool needsToRetreiveInformationFromInternet = SteamFriends.RequestUserInformation(lobbyPlayer.steamID, true);
		if (!needsToRetreiveInformationFromInternet)
		{
			player.name = SteamFriends.GetFriendPersonaName(lobbyPlayer.steamID);
			lobbyPlayer.SetName("", player.name);
			FindObjectOfType<SteamLobby>().AddNameToList(player.name);

			if (SteamUtils.GetImageSize(SteamFriends.GetLargeFriendAvatar(lobbyPlayer.steamID), out uint width, out uint height))
			{
				lobbyPlayer.RPCApplyIcon(SteamFriends.GetLargeFriendAvatar(lobbyPlayer.steamID), (int)width, (int)height);
			}

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

		//Debug.Log("Initializing Player: " + conn.connectionId);
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
		for (int i = 0; i < playerConns.Count; i++)
		{
			if (playerConns[i].conn == conn)
			{
				playerColors.Add(playerConns[i].playerColor); break;
			}
		}

		base.OnServerDisconnect(conn);
    }

	public override void OnServerSceneChanged(string sceneName)
	{
		base.OnServerSceneChanged(sceneName);

		if (sceneName != worldSceneName)
			return;

		StartCoroutine(WaitForAllLoaded());
	}

	IEnumerator WaitForAllLoaded ()
	{
		foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values)
		{
			if (!conn.isReady) yield return null;
		}

		yield return null;
		yield return null;

		foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values)
		{
			string playerName = "";
			Texture2D playerIcon = null;
			Color playerColor = Color.white;
			int playerIndex = -1;

			for (int i = 0; i < playerConns.Count; i++)
			{
				if (playerConns[i].conn == conn)
				{
					playerName = playerConns[i].playerName;
					playerIcon = playerConns[i].playerIcon;
					playerColor = playerConns[i].playerColor;
					playerIndex = i;
					break;
				}
			}

			if (playerIndex == -1)
				continue;

			GameObject player = Instantiate(basePrefab);
			NetworkServer.Spawn(player, conn);

			player.name = "Base - " + playerName;
			player.GetComponent<BaseSpawner>().playerName = playerName;
			player.GetComponent<BaseSpawner>().playerIcon = playerIcon;

			// instantiating a "Player" prefab gives it the name "Player(clone)"
			// => appending the connectionId is WAY more useful for debugging!

			//Debug.Log("Initializing Player: " + conn.connectionId);
			//while (!conn.isReady)
			//{
			//	yield return null;
			//}
			NetworkServer.AddPlayerForConnection(conn, player);
			//player.GetComponent<NetworkIdentity>().AssignClientAuthority(conn);

			player.SendMessage("Initialize", new BaseSpawner.InitArgs { playerIndex = playerIndex, playerColor = playerColor });
		}
	}

	public void AddPlayerToList(System.Func<string> getPersonaName)
	{
		throw new System.NotImplementedException();
	}
}