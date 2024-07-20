using Mirror;
using Steamworks;

public class LobbyPlayer : NetworkBehaviour
{
	[SyncVar(hook = "SetName")]
	public string playerName;

	[SyncVar(hook = "SetSteamID")]
	public CSteamID steamID;

	public void SetName(string oldValue, string newValue)
	{
		playerName = newValue;
		gameObject.name = playerName;
	}

	public void SetSteamID(CSteamID oldValue, CSteamID newValue)
	{
		steamID = newValue;
	}
}
