using Lidgren.Network;
using UnityEngine;

namespace Landfall.Network.Sockets
{
	public class NetworkSocketServer
	{
		private static P2PPackageHandler m_PacketHandler;

		public NetServer Server { get; private set; }

		public NetworkSocketServer()
		{
			m_PacketHandler = P2PPackageHandler.Instance;
			NetPeerConfiguration netPeerConfiguration = new NetPeerConfiguration(NetworkSocketData.APP_NAME)
			{
				Port = 1337
			};
			netPeerConfiguration.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);
			netPeerConfiguration.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
			Server = new NetServer(netPeerConfiguration);
		}

		public void Init()
		{
			Server.Start();
			Debug.Log("Hosting socket server!" + Server.Status);
		}

		public void SendServerDiscoveryResponse(NetIncomingMessage inc)
		{
			NetOutgoingMessage netOutgoingMessage = Server.CreateMessage();
			netOutgoingMessage.Write("My server name");
			Server.SendDiscoveryResponse(netOutgoingMessage, inc.SenderEndPoint);
			Debug.Log("Sending Discovery Response! To: " + inc.SenderEndPoint.Address.ToString());
		}

		public void OnConnectedMessageRecieved(NetIncomingMessage message)
		{
			NetConnection senderConnection = message.SenderConnection;
			NetOutgoingMessage netOutgoingMessage = Server.CreateMessage("Hello new user: I am your host: " + Server.UniqueIdentifier);
			m_PacketHandler.SendSocketP2PPacketToUser(senderConnection, netOutgoingMessage.Data, P2PPackageHandler.MsgType.Ping);
		}
	}
}
