using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class UdpSockets : MonoBehaviour
{
	public static UdpSockets instance;

	public bool isServer;

	public IPAddress serverIp;

	private List<IPEndPoint> clientList = new List<IPEndPoint>();

	private UdpConnectedClient connection;

	public void Awake()
	{
		instance = this;
	}

	public void StartServer()
	{
		isServer = true;
		connection = new UdpConnectedClient();
	}

	public void StartClient(string ipAddress)
	{
		IPAddress iPAddress = IPAddress.Parse(ipAddress);
		serverIp = iPAddress;
		isServer = false;
		connection = new UdpConnectedClient(serverIp);
		AddClient(new IPEndPoint(serverIp, 1337));
	}

	internal static void AddClient(IPEndPoint ipEndpoint)
	{
		if (!instance.clientList.Contains(ipEndpoint))
		{
			MonoBehaviour.print("Connected TO: " + ipEndpoint);
			instance.clientList.Add(ipEndpoint);
		}
	}

	internal static void RemoveClient(IPEndPoint ipEndpoint)
	{
		instance.clientList.Remove(ipEndpoint);
	}

	private void OnApplicationQuit()
	{
		connection.Close();
	}

	public void Send(string message)
	{
		if (isServer)
		{
		}
		BroadcastChatMessage(message);
	}

	internal static void BroadcastChatMessage(string message)
	{
		foreach (IPEndPoint client in instance.clientList)
		{
			instance.connection.Send(message, client);
		}
	}
}
