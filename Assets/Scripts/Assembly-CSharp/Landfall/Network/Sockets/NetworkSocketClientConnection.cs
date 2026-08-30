using System;
using Lidgren.Network;
using UnityEngine;

namespace Landfall.Network.Sockets
{
	public class NetworkSocketClientConnection
	{
		private NetClient m_NetworkClient;

		public bool Init()
		{
			SocketPlayerLoginInformation ob = new SocketPlayerLoginInformation("Philip");
			NetPeerConfiguration netPeerConfiguration = new NetPeerConfiguration(NetworkSocketData.APP_NAME);
			netPeerConfiguration.Port = 1337;
			netPeerConfiguration.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
			m_NetworkClient = new NetClient(netPeerConfiguration);
			m_NetworkClient.Start();
			m_NetworkClient.DiscoverLocalPeers(1337);
			Debug.Log("Discovering local peers!");
			NetOutgoingMessage netOutgoingMessage = m_NetworkClient.CreateMessage();
			netOutgoingMessage.Write((byte)1);
			netOutgoingMessage.WriteAllProperties(ob);
			m_NetworkClient.Connect("localhost", 1337, netOutgoingMessage);
			return ValidateConnectionRequest();
		}

		private bool ValidateConnectionRequest()
		{
			DateTime now = DateTime.Now;
			NetIncomingMessage netIncomingMessage;
			while (true)
			{
				if (DateTime.Now.Subtract(now).Seconds > 5)
				{
					Debug.Log("Time Out!");
					return false;
				}
				if ((netIncomingMessage = m_NetworkClient.ReadMessage()) != null)
				{
					NetIncomingMessageType messageType = netIncomingMessage.MessageType;
					if (messageType == NetIncomingMessageType.Data)
					{
						break;
					}
				}
			}
			P2PPackageHandler.MsgType msgType = (P2PPackageHandler.MsgType)netIncomingMessage.ReadByte();
			if (msgType == P2PPackageHandler.MsgType.ClientAccepted)
			{
				return true;
			}
			return false;
		}
	}
}
