using Mirror;
using Steamworks;
using TMPro;

public class LobbyPlayer : NetworkBehaviour
{
	[SyncVar(hook = "SetName")]
	public string playerName;

	[SyncVar(hook = "SetSteamID")]
	public CSteamID steamID;

	public TMP_Text nametag;

	public void SetName(string oldValue, string newValue)
	{
		playerName = newValue;
		gameObject.name = playerName;
		nametag.text = playerName;
	}

	public void SetSteamID(CSteamID oldValue, CSteamID newValue)
	{
		steamID = newValue;
	}
}
