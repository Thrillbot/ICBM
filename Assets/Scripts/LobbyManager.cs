using Mirror;
using System.Collections.Generic;
using TMPro;

public class LobbyManager : NetworkBehaviour
{
	public TMP_Text playerListText;

	[SyncVar(hook = "AddPlayerName")]
	private List<string> playerList;

	public void AddPlayerToList(string name)
	{
		playerList ??= new();
		playerList.Add(name);

		playerListText.text = "";
		foreach (string s in playerList)
		{
			playerListText.text += s + '\n';
		}

		AddPlayerName(new(), playerList);
	}

	void AddPlayerName(List<string> oldList, List<string> newList)
	{
		playerList = newList;
	}
}
