using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class UdpConnectedClient
{
	private readonly UdpClient connection;

	public UdpConnectedClient(IPAddress ip = null)
	{
		if (UdpSockets.instance.isServer)
		{
			connection = new UdpClient(1337);
		}
		else
		{
			connection = new UdpClient();
		}
		connection.BeginReceive(OnReceive, null);
	}

	public void Close()
	{
		connection.Close();
	}

	private void OnReceive(IAsyncResult ar)
	{
		try
		{
			IPEndPoint remoteEP = null;
			byte[] bytes = connection.EndReceive(ar, ref remoteEP);
			UdpSockets.AddClient(remoteEP);
			string text = Encoding.UTF8.GetString(bytes);
			Debug.Log("Recieved Data: " + text);
			if (UdpSockets.instance.isServer)
			{
				UdpSockets.BroadcastChatMessage(text);
			}
		}
		catch (SocketException)
		{
		}
		connection.BeginReceive(OnReceive, null);
	}

	internal void Send(string message, IPEndPoint ipEndpoint)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(message);
		connection.Send(bytes, bytes.Length, ipEndpoint);
	}
}
