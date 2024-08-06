using Mirror;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayer : NetworkBehaviour
{
	[SyncVar(hook = "SetName")]
	public string playerName;

	[SyncVar(hook = "SetSteamID")]
	public CSteamID steamID;

	public TMP_Text nametag;

	[SyncVar(hook = "SetPlayerColor")]
	public Color playerColor;

	public Texture2D playerIcon;
	public RawImage playerIconImage;

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

	public void SetPlayerColor(Color oldValue, Color newValue)
	{
		playerColor = newValue;
		foreach (Renderer r in GetComponentsInChildren<Renderer>())
		{
			r.material.color = playerColor;
			r.material.SetColor("_EmissionColor", playerColor);
		}
	}

	[ClientRpc]
	public void RPCApplyIcon (int imageID, int width, int height)
	{
		byte[] image = new byte[width * height * 4];
		if (SteamUtils.GetImageRGBA(imageID, image, (int)(width * height * 4)))
		{
			playerIcon = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false, true);
			playerIcon.LoadRawTextureData(image);
			playerIcon.Apply();
			playerIconImage.texture = playerIcon;
		}
		
	}
}
