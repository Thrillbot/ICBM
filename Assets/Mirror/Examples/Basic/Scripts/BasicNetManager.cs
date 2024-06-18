using UnityEngine;

namespace Mirror.Examples.Basic
{
    [AddComponentMenu("")]
    public class BasicNetManager : NetworkManager
    {
		/// <summary>
		/// Called on the server when a client adds a new player with NetworkClient.AddPlayer.
		/// <para>The default implementation for this function creates a new player object from the playerPrefab.</para>
		/// </summary>
		/// <param name="conn">Connection from client.</param>
		public override void OnServerAddPlayer(NetworkConnectionToClient conn)
		{
			//base.OnServerAddPlayer(conn);

			Transform startPos = GetStartPosition();
			GameObject player = Instantiate(playerPrefab, startPos.position, startPos.rotation);
			//NetworkServer.Spawn(player, conn);


			// instantiating a "Player" prefab gives it the name "Player(clone)"
			// => appending the connectionId is WAY more useful for debugging!
			player.name = $"{playerPrefab.name} [connId={conn.connectionId}]";

			Debug.Log("Initializing Player: " + conn.connectionId);
			NetworkServer.AddPlayerForConnection(conn, player);
			player.GetComponent<NetworkIdentity>().AssignClientAuthority(conn);
			player.SendMessage("Initialize", startPos.GetComponent<NetworkIdentity>().netId);

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
	}
}
