using System.Net;
using Landfall.Network.Sockets;
using Lidgren.Network;
using UnityEngine;

public class MatchMakingHandlerSockets : MonoBehaviour
{
	private const string APP_NAME = "StickFight 1.0";

	private bool m_Active;

	private static bool m_IsServer;

	private NetConnection m_NetConnection;

	private NetworkSocketServer m_Server;

	private NetClient m_Client;

	private static P2PPackageHandler m_PacketHandler;

	public static MatchMakingHandlerSockets Instance;

	public static bool IsServer
	{
		get
		{
			return m_IsServer;
		}
	}

	public NetworkSocketServer Server
	{
		get
		{
			return m_Server;
		}
	}

	private void Awake()
	{
		Instance = this;
		m_IsServer = false;
	}

	private void Start()
	{
		m_PacketHandler = P2PPackageHandler.Instance;
	}

	private void Update()
	{
	}

	private void CheckForConnections()
	{
	}

	public bool HostServer()
	{
		m_Active = true;
		m_IsServer = true;
		m_Server = new NetworkSocketServer();
		m_Server.Init();
		return m_Server.Server.Status == NetPeerStatus.Running;
	}

	public void JoinServer()
	{
		m_Active = true;
		m_IsServer = false;
		NetPeerConfiguration netPeerConfiguration = new NetPeerConfiguration("StickFight 1.0");
		netPeerConfiguration.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
		NetClient netClient = new NetClient(netPeerConfiguration);
		netClient.Start();
		m_Client = netClient;
		m_Client.DiscoverLocalPeers(1337);
		Debug.Log("Discovering local peers!");
		NetOutgoingMessage netOutgoingMessage = m_Client.CreateMessage(1);
		netOutgoingMessage.Write((byte)1);
		m_NetConnection = m_Client.Connect("127.0.0.1", 1337, netOutgoingMessage);
	}

	public void JoinServerAt(IPEndPoint point)
	{
		m_NetConnection = m_Client.Connect(point);
		Debug.Log("Joining SOcket server after response from host!");
	}

	public NetIncomingMessage ReadMessage()
	{
		if (!m_Active)
		{
			return null;
		}
		NetIncomingMessage result;
		if (m_IsServer)
		{
			if ((result = m_Server.Server.ReadMessage()) != null)
			{
				return result;
			}
		}
		else if ((result = m_Client.ReadMessage()) != null)
		{
			return result;
		}
		return null;
	}

	public void SendServerDiscoveryResponse(NetIncomingMessage message)
	{
		m_Server.SendServerDiscoveryResponse(message);
	}

	public void OnConnectedMessageRecieved(NetIncomingMessage message)
	{
		if (m_IsServer)
		{
			m_Server.OnConnectedMessageRecieved(message);
			return;
		}
		Debug.Log("Onconnected Called by client!");
		Object.FindObjectOfType<MultiplayerManager>().OnSocketServerJoined();
	}

	public void SendSocketMessage(NetConnection user, byte[] data, int channel)
	{
		NetOutgoingMessage netOutgoingMessage = ((!m_IsServer) ? m_Client.CreateMessage(data.Length) : m_Server.Server.CreateMessage(data.Length));
		netOutgoingMessage.Write(data);
		Debug.Log("Sending SOcket message: Result: " + ((!m_IsServer) ? m_Client.SendMessage(netOutgoingMessage, user, NetDeliveryMethod.ReliableOrdered, channel) : m_Server.Server.SendMessage(netOutgoingMessage, user, NetDeliveryMethod.ReliableOrdered, channel)));
	}
}
