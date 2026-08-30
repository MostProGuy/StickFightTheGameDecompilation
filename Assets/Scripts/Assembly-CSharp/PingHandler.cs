using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PingHandler
{
	public struct PingMessage
	{
		public ulong User;

		public float TimeStamp;

		public int Identifier;

		public PingMessage(ulong user, float time, int identifier)
		{
			User = user;
			TimeStamp = time;
			Identifier = identifier;
		}
	}

	private static List<PingMessage> mPingMessages = new List<PingMessage>();

	private static MultiplayerManager mNetworkManager;

	public static void Init(MultiplayerManager manager)
	{
		Debug.Log("Heyoo");
		mNetworkManager = manager;
	}

	public static void AddPingMessage(ulong userID, byte[] data)
	{
		int identifier;
		using (MemoryStream input = new MemoryStream(data))
		{
			using (BinaryReader binaryReader = new BinaryReader(input))
			{
				identifier = binaryReader.ReadInt32();
			}
		}
		float unscaledTime = Time.unscaledTime;
		PingMessage item = new PingMessage(userID, unscaledTime, identifier);
		mPingMessages.Add(item);
	}

	public static void PingMessageRecieved(ulong userID, byte[] data)
	{
		int num;
		using (MemoryStream input = new MemoryStream(data))
		{
			using (BinaryReader binaryReader = new BinaryReader(input))
			{
				num = binaryReader.ReadInt32();
			}
		}
		float unscaledTime = Time.unscaledTime;
		int count = mPingMessages.Count;
		for (int num2 = count - 1; num2 >= 0; num2--)
		{
			PingMessage item = mPingMessages[num2];
			if (item.User == userID && item.Identifier == num)
			{
				float timeStamp = item.TimeStamp;
				float num3 = unscaledTime - timeStamp;
				mNetworkManager.UpdatePingForUser(userID, num3 * 1000f);
				mPingMessages.Remove(item);
				break;
			}
		}
	}
}
