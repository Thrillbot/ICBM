using Mirror;
using Steamworks;

public class LobbyPlayer : NetworkBehaviour
{
	[SyncVar(hook = "SetName")]
	public string playerName;

	public override void OnStartAuthority() {
		SetName("", SteamFriends.GetPersonaName());
	}

	void SetName(string oldValue, string newValue)
	{
		playerName = newValue;
		FindObjectOfType<LobbyManager>().AddSelfToLobby(this);
	}
}
