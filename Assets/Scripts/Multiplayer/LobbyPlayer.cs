using Mirror;
using Steamworks;

public class LobbyPlayer : NetworkBehaviour
{
	void OnEnable() {
		LobbyManager lobbyManager = FindObjectOfType<LobbyManager>();
		lobbyManager.AddPlayerToList(SteamFriends.GetPersonaName());
	}
}
