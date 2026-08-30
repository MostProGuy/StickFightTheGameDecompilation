using System.IO;
using Steamworks;
using UnityEngine;

public class SyncableObjectManager : MonoBehaviour
{
	public MultiplayerManager mMultiplayerManager;

	public const int mUpdateChannel = 10;

	public const int mEventChannel = 11;

	private void Start()
	{
	}

	private void Update()
	{
	}

	private void LateUpdate()
	{
		ListenForPackages(10);
		ListenForPackages(11);
	}

	private void ListenForPackages(int channel)
	{
		uint pcubMsgSize;
		while (SteamNetworking.IsP2PPacketAvailable(out pcubMsgSize, channel))
		{
			byte[] array = new byte[pcubMsgSize];
			uint pcubMsgSize2;
			CSteamID psteamIDRemote;
			if (!SteamNetworking.ReadP2PPacket(array, pcubMsgSize, out pcubMsgSize2, out psteamIDRemote, channel))
			{
				Debug.Log("Failed to read P2P Package!");
				continue;
			}
			using (MemoryStream input = new MemoryStream(array))
			{
				using (BinaryReader binaryReader = new BinaryReader(input))
				{
					uint num = binaryReader.ReadUInt32();
					P2PPackageHandler.MsgType msgType = (P2PPackageHandler.MsgType)binaryReader.ReadByte();
					if (num < MultiplayerManager.LastTimeStamp)
					{
						Debug.LogWarning("Packet Is obsolete!");
						continue;
					}
					byte[] data = binaryReader.ReadBytes((int)(pcubMsgSize - 1));
					ReceivedPackage(msgType, psteamIDRemote, data, channel == 10);
				}
			}
		}
	}

	private void ReceivedPackage(P2PPackageHandler.MsgType msgType, CSteamID steamId, byte[] data, bool updatePackage)
	{
		ushort networkIndex = ushort.MaxValue;
		using (MemoryStream input = new MemoryStream(data))
		{
			using (BinaryReader binaryReader = new BinaryReader(input))
			{
				networkIndex = binaryReader.ReadUInt16();
			}
		}
		NetworkSyncableObject syncedObject = GameManager.Instance.mMultiplayerManager.GetSyncedObject(networkIndex);
		if ((bool)syncedObject)
		{
			if (syncedObject.isActiveAndEnabled && syncedObject != mMultiplayerManager.DefaultSyncableObject)
			{
				bool flag = updatePackage && syncedObject.DontSyncForSeconds > 0f;
				if (syncedObject.ListeningForPackages && !flag)
				{
					syncedObject.ReceivedPackage(msgType, steamId, data);
				}
			}
		}
		else
		{
			Debug.LogWarning("Found no recipient for package!");
		}
	}
}
