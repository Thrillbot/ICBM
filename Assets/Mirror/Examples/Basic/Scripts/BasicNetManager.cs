using TMPro;
using UnityEngine;

namespace Mirror.Examples.Basic
{
    [AddComponentMenu("")]
    public class BasicNetManager : NetworkManager
	{
		public TMP_InputField ipField;
		public TMP_InputField portField;

		public string worldSceneName;
		public GameObject basePrefab;

		/// <summary>
		/// Called on the server when a client adds a new player with NetworkClient.AddPlayer.
		/// <para>The default implementation for this function creates a new player object from the playerPrefab.</para>
		/// </summary>
		/// <param name="conn">Connection from client.</param>
		public override void OnServerAddPlayer(NetworkConnectionToClient conn)
		{
			//base.OnServerAddPlayer(conn);

			GameObject player = Instantiate(playerPrefab);
			//NetworkServer.Spawn(player, conn);


			// instantiating a "Player" prefab gives it the name "Player(clone)"
			// => appending the connectionId is WAY more useful for debugging!
			player.name = $"{playerPrefab.name} [connId={conn.connectionId}]";

			Debug.Log("Initializing Player: " + conn.connectionId);
			NetworkServer.AddPlayerForConnection(conn, player);

			Player.ResetPlayerNumbers();
        }

        /// <summary>
        /// Called on the server when a client disconnects.
        /// <para>This is called on the Server when a Client disconnects from the Server. Use an override to decide what should happen when a disconnection is detected.</para>
        /// </summary>
        /// <param name="conn">Connection from client.</param>
        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            base.OnServerDisconnect(conn);
            Player.ResetPlayerNumbers();
        }

		public override void OnServerSceneChanged(string sceneName)
		{
			base.OnServerSceneChanged(sceneName);

			if (sceneName != worldSceneName)
				return;

			foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values)
			{
				Transform startPos = GetStartPosition();
				GameObject player = Instantiate(basePrefab, startPos.position, startPos.rotation);
				//NetworkServer.Spawn(player, conn);


				// instantiating a "Player" prefab gives it the name "Player(clone)"
				// => appending the connectionId is WAY more useful for debugging!
				player.name = $"{basePrefab.name} [connId={conn.connectionId}]";

				Debug.Log("Initializing Player: " + conn.connectionId);
				NetworkServer.AddPlayerForConnection(conn, player);
				player.GetComponent<NetworkIdentity>().AssignClientAuthority(conn);
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

		public void IPOrPortChange ()
		{
			networkAddress = ipField.text;
			// only show a port field if we have a port transport
			// we can't have "IP:PORT" in the address field since this only
			// works for IPV4:PORT.
			// for IPV6:PORT it would be misleading since IPV6 contains ":":
			// 2001:0db8:0000:0000:0000:ff00:0042:8329
			if (Transport.active is PortTransport portTransport)
			{
				// use TryParse in case someone tries to enter non-numeric characters
				if (ushort.TryParse(portField.text, out ushort port))
					portTransport.Port = port;
			}
		}

		public void AddPlayerToList(System.Func<string> getPersonaName)
		{
			throw new System.NotImplementedException();
		}
	}
}
