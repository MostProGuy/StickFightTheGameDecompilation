using System;
using System.IO;
using Lidgren.Network;
using Steamworks;
using UnityEngine;

public class P2PPackageHandler : MonoBehaviour
{
	public enum MsgType : byte
	{
		Ping = 0,
		PingResponse = 1,
		ClientJoined = 2,
		ClientRequestingAccepting = 3,
		ClientAccepted = 4,
		ClientInit = 5,
		ClientRequestingIndex = 6,
		ClientRequestingToSpawn = 7,
		ClientSpawned = 8,
		ClientReadyUp = 9,
		PlayerUpdate = 10,
		PlayerTookDamage = 11,
		PlayerTalked = 12,
		PlayerForceAdded = 13,
		PlayerForceAddedAndBlock = 14,
		PlayerLavaForceAdded = 15,
		PlayerFallOut = 16,
		PlayerWonWithRicochet = 17,
		MapChange = 18,
		WeaponSpawned = 19,
		WeaponThrown = 20,
		RequestingWeaponThrow = 21,
		ClientRequestWeaponDrop = 22,
		WeaponDropped = 23,
		WeaponWasPickedUp = 24,
		ClientRequestingWeaponPickUp = 25,
		ObjectUpdate = 26,
		ObjectSpawned = 27,
		ObjectSimpleDestruction = 28,
		ObjectInvokeDestructionEvent = 29,
		ObjectDestructionCollision = 30,
		GroundWeaponsInit = 31,
		MapInfo = 32,
		MapInfoSync = 33,
		WorkshopMapsLoaded = 34,
		StartMatch = 35,
		ObjectHello = 36,
		OptionsChanged = 37,
		KickPlayer = 38
	}

	protected Callback<P2PSessionRequest_t> m_P2PSessionRequest;

	protected Callback<P2PSessionConnectFail_t> m_P2PSessionConnectFail;

	protected Callback<SocketStatusCallback_t> m_SocketStatusCallback_t;

	private bool mHasHandler;

	private MultiplayerManager mNetworkHandler;

	[SerializeField]
	private uint m_SimulatedMS;

	private float m_MsInSeconds;

	private static P2PPackageHandler _instance;

	private bool mPauseTraffic;

	public static P2PPackageHandler Instance
	{
		get
		{
			return _instance;
		}
	}

	private void Awake()
	{
		if (_instance != null)
		{
			UnityEngine.Object.Destroy(this);
			return;
		}
		_instance = this;
		m_MsInSeconds = (float)m_SimulatedMS / 1000f;
	}

	public void Init()
	{
		mNetworkHandler = UnityEngine.Object.FindObjectOfType<MultiplayerManager>();
		if (mNetworkHandler != null)
		{
			mHasHandler = true;
		}
	}

	private void OnEnable()
	{
		if (SteamManager.Initialized)
		{
			m_P2PSessionRequest = Callback<P2PSessionRequest_t>.Create(OnP2PSessionRequest);
			m_P2PSessionConnectFail = Callback<P2PSessionConnectFail_t>.Create(OnP2PSessionConnectFail);
			m_SocketStatusCallback_t = Callback<SocketStatusCallback_t>.Create(OnSocketStatusCallback);
		}
	}

	private void Update()
	{
		if (SteamManager.Initialized && !mPauseTraffic)
		{
			m_MsInSeconds = (float)m_SimulatedMS / 1000f;
			if (!MatchmakingHandler.Instance.IsInsideLobby || !mHasHandler)
			{
				CheckForPackagesOnChannelInMainMenu();
				CheckForPackagesOnChannelInMainMenu(1);
			}
			else
			{
				CheckForPackagesOnChannel(1);
				CheckForPackagesOnChannel();
			}
		}
	}

	private void CheckForPackagesOnChannelInMainMenu(int channel = 0)
	{
		CheckForPackagesOnChannel(channel, false);
	}

	private void ReadSocketMessages()
	{
		NetIncomingMessage netIncomingMessage;
		while ((netIncomingMessage = MatchMakingHandlerSockets.Instance.ReadMessage()) != null)
		{
			switch (netIncomingMessage.MessageType)
			{
			case NetIncomingMessageType.Data:
			{
				byte[] data = netIncomingMessage.Data;
				ReadMessageBuffer(data, new CSteamID(1uL));
				break;
			}
			case NetIncomingMessageType.StatusChanged:
				switch (netIncomingMessage.SenderConnection.Status)
				{
				case NetConnectionStatus.Connected:
					MatchMakingHandlerSockets.Instance.OnConnectedMessageRecieved(netIncomingMessage);
					break;
				}
				Debug.Log("Status Changed: " + netIncomingMessage.SenderConnection.Status);
				break;
			case NetIncomingMessageType.DebugMessage:
				Debug.Log(netIncomingMessage.ReadString());
				break;
			case NetIncomingMessageType.DiscoveryRequest:
				MatchMakingHandlerSockets.Instance.SendServerDiscoveryResponse(netIncomingMessage);
				break;
			case NetIncomingMessageType.DiscoveryResponse:
				Debug.Log(string.Concat("Found server at ", netIncomingMessage.SenderEndPoint, " name: ", netIncomingMessage.ReadString()));
				MatchMakingHandlerSockets.Instance.JoinServerAt(netIncomingMessage.SenderEndPoint);
				break;
			default:
				Debug.Log("unhandled message with type: " + netIncomingMessage.MessageType);
				break;
			}
		}
	}

	private void CheckForPackagesOnChannel(int channel = 0, bool checkHandler = true)
	{
		if (checkHandler && !mHasHandler)
		{
			Debug.LogError("Checking messages with no handler: Returning...");
			return;
		}
		if (mPauseTraffic)
		{
			Debug.Log("Traffic Is Paused, returning");
			return;
		}
		if (MatchmakingHandler.RunningOnSockets)
		{
			ReadSocketMessages();
			return;
		}
		uint pcubMsgSize;
		while (SteamNetworking.IsP2PPacketAvailable(out pcubMsgSize, channel))
		{
			byte[] array = new byte[pcubMsgSize];
			uint pcubMsgSize2;
			CSteamID psteamIDRemote;
			if (!SteamNetworking.ReadP2PPacket(array, pcubMsgSize, out pcubMsgSize2, out psteamIDRemote, channel))
			{
				Debug.Log("Failed to read P2P Package!");
				break;
			}
			ReadMessageBuffer(array, psteamIDRemote);
		}
	}

	private void ReadMessageBuffer(byte[] rawData, CSteamID SteamIdRemote)
	{
		using (MemoryStream input = new MemoryStream(rawData))
		{
			using (BinaryReader binaryReader = new BinaryReader(input))
			{
				uint lastTimeStamp = MultiplayerManager.LastTimeStamp;
				uint num = binaryReader.ReadUInt32();
				MsgType msgType = (MsgType)binaryReader.ReadByte();
				if (!mNetworkHandler.HasBeenInitializedFromServer && msgType != MsgType.ClientInit && msgType != MsgType.ClientAccepted)
				{
					Debug.Log(string.Concat("Stopping packet: ", msgType, " Has not been inited by server yet!"));
					return;
				}
				if (num < lastTimeStamp)
				{
					Debug.LogWarning("Packet is obsolete!");
				}
				byte[] data = binaryReader.ReadBytes(rawData.Length - 1);
				CheckMessageType(data, msgType, SteamIdRemote);
			}
		}
	}

	private void CheckMessageType(byte[] data, MsgType type, CSteamID steamIdRemote)
	{
		switch (type)
		{
		case MsgType.Ping:
			if (data.Length > 0)
			{
				SendP2PPacketToUser(steamIdRemote, data, MsgType.PingResponse);
			}
			break;
		case MsgType.PingResponse:
			if (steamIdRemote == SteamUser.GetSteamID())
			{
				int num = 0;
			}
			PingHandler.PingMessageRecieved(steamIdRemote.m_SteamID, data);
			break;
		case MsgType.ClientJoined:
			mNetworkHandler.OnClientJoined(data);
			break;
		case MsgType.ClientInit:
			mNetworkHandler.OnInitFromServer(data);
			break;
		case MsgType.ClientRequestingAccepting:
			SendP2PPacketToUser(steamIdRemote, new byte[0], MsgType.ClientAccepted);
			break;
		case MsgType.ClientAccepted:
			mNetworkHandler.OnClientAcceptedByServer();
			break;
		case MsgType.ClientRequestingIndex:
			mNetworkHandler.OnPlayerRequestingIndex(data);
			break;
		case MsgType.ClientRequestingToSpawn:
			mNetworkHandler.OnPlayerRequestingToSpawn(data);
			break;
		case MsgType.ClientSpawned:
			mNetworkHandler.OnPlayerSpawned(data);
			break;
		case MsgType.MapChange:
			mNetworkHandler.OnMapChanged(data);
			break;
		case MsgType.WeaponSpawned:
			mNetworkHandler.OnWeaponSpawned(data);
			break;
		case MsgType.WeaponWasPickedUp:
			mNetworkHandler.OnWeaponWasPickedUp(data);
			break;
		case MsgType.ClientRequestingWeaponPickUp:
			mNetworkHandler.OnPlayerRequestingWeaponPickUp(data);
			break;
		case MsgType.ClientRequestWeaponDrop:
			mNetworkHandler.OnPlayerRequestingWeaponDrop(data);
			break;
		case MsgType.WeaponDropped:
			mNetworkHandler.OnWeaponDropped(data);
			break;
		case MsgType.ObjectSpawned:
			mNetworkHandler.OnObjectSpawned(data);
			break;
		case MsgType.GroundWeaponsInit:
			mNetworkHandler.OnGroundWeaponsInit(data);
			break;
		case MsgType.MapInfo:
			mNetworkHandler.OnMapInfoRecieved(data);
			break;
		case MsgType.MapInfoSync:
			mNetworkHandler.OnMapDataRecieved(data);
			break;
		case MsgType.WorkshopMapsLoaded:
			mNetworkHandler.OnNewWorkshopMapsRecieved(data);
			break;
		case MsgType.OptionsChanged:
			OptionsHolder.NetworkOptionsChanged(data);
			break;
		case MsgType.KickPlayer:
			mNetworkHandler.OnKicked(data);
			break;
		case MsgType.ClientReadyUp:
			mNetworkHandler.OnClientReadyUp(data);
			break;
		case MsgType.StartMatch:
			mNetworkHandler.OnMatchStart(data);
			break;
		default:
			throw new Exception(string.Concat("Messagetype: ", type, " Is not setup!"));
		}
	}

	public void SendP2PPacketToServer(byte[] data, MsgType messageType, EP2PSend sendMethod = EP2PSend.k_EP2PSendReliable, int channel = -1)
	{
		CSteamID lobbyOwner = MatchmakingHandler.Instance.LobbyOwner;
		int channel2 = ((channel != -1) ? channel : GetChannelForMsgType(messageType));
		SendP2PPacketToUser(lobbyOwner, data, messageType, sendMethod, channel2);
	}

	private int GetChannelForMsgType(MsgType messageType)
	{
		switch (messageType)
		{
		case MsgType.Ping:
			return 0;
		case MsgType.PingResponse:
			return 0;
		case MsgType.ClientInit:
			return 0;
		case MsgType.ClientJoined:
			return 0;
		case MsgType.ClientRequestingIndex:
			return 0;
		case MsgType.ClientRequestingToSpawn:
			return 0;
		case MsgType.ClientSpawned:
			return 0;
		case MsgType.MapChange:
			return 1;
		case MsgType.WeaponSpawned:
			return 1;
		case MsgType.WeaponWasPickedUp:
			return 1;
		case MsgType.ClientRequestingWeaponPickUp:
			return 1;
		case MsgType.ClientRequestWeaponDrop:
			return 1;
		case MsgType.WeaponDropped:
			return 1;
		case MsgType.ObjectSpawned:
			return 1;
		case MsgType.ObjectSimpleDestruction:
			return 1;
		case MsgType.ObjectInvokeDestructionEvent:
			return 1;
		case MsgType.GroundWeaponsInit:
			return 1;
		case MsgType.MapInfo:
			return 1;
		case MsgType.MapInfoSync:
			return 0;
		case MsgType.PlayerForceAddedAndBlock:
			return 0;
		case MsgType.PlayerFallOut:
			return 1;
		case MsgType.OptionsChanged:
			return 1;
		case MsgType.KickPlayer:
			return 1;
		case MsgType.ClientAccepted:
			return 1;
		case MsgType.ClientRequestingAccepting:
			return 1;
		case MsgType.ClientReadyUp:
			return 1;
		case MsgType.StartMatch:
			return 1;
		case MsgType.WorkshopMapsLoaded:
			return 1;
		default:
			throw new Exception("Message Type is not setup: " + messageType);
		}
	}

	public void SendSocketP2PPacketToUser(NetConnection user, byte[] data, MsgType messageType, NetDeliveryMethod sendMethod = NetDeliveryMethod.ReliableOrdered, int channel = 0)
	{
		uint value = uint.MaxValue;
		byte[] array = new byte[data.Length + 4 + 1];
		using (MemoryStream output = new MemoryStream(array))
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(output))
			{
				binaryWriter.Write(value);
				binaryWriter.Write((byte)messageType);
				binaryWriter.Write(data);
			}
		}
		SendSocketMessage(user, array, channel);
	}

	private byte[] WriteMessageBuffer(byte[] data, MsgType messageType)
	{
		uint serverRealTime = SteamUtils.GetServerRealTime();
		byte[] array = new byte[data.Length + 4 + 1];
		using (MemoryStream output = new MemoryStream(array))
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(output))
			{
				binaryWriter.Write(serverRealTime);
				binaryWriter.Write((byte)messageType);
				binaryWriter.Write(data);
				return array;
			}
		}
	}

	public void SendP2PPacketToUser(NetConnection user, byte[] data, MsgType messageType, EP2PSend sendMethod = EP2PSend.k_EP2PSendReliable, int channel = 0)
	{
		byte[] data2 = WriteMessageBuffer(data, messageType);
		SendSocketMessage(user, data2, channel);
	}

	public void SendP2PPacketToUser(CSteamID clientID, byte[] data, MsgType messageType, EP2PSend sendMethod = EP2PSend.k_EP2PSendReliable, int channel = 0)
	{
		byte[] array = WriteMessageBuffer(data, messageType);
		uint num = (uint)array.Length;
		if (!SteamNetworking.SendP2PPacket(clientID, array, num, sendMethod, channel))
		{
			Debug.Log("FAILED send package to User: " + clientID.m_SteamID);
			return;
		}
		P2PStatistics.BytesWasSent(num, (MsgType)array[4]);
		if (messageType == MsgType.Ping && data.Length > 0)
		{
			PingHandler.AddPingMessage(clientID.m_SteamID, data);
		}
	}

	private void SendSocketMessage(CSteamID user, byte[] data, int channel)
	{
	}

	private void SendSocketMessage(NetConnection user, byte[] data, int channel)
	{
		MatchMakingHandlerSockets.Instance.SendSocketMessage(user, data, channel);
	}

	public void ResumeNetworkTraffic()
	{
		mPauseTraffic = false;
		Debug.Log("Resumed Network Traffic On time: " + Time.time);
	}

	public void PauseNetworkTraffic()
	{
		mPauseTraffic = true;
		Debug.Log("Paused Network Traffic On time: " + Time.time);
	}

	private void OnP2PSessionRequest(P2PSessionRequest_t pCallback)
	{
		Debug.Log("[" + 1202 + " - P2PSessionRequest] - " + pCallback.m_steamIDRemote);
		if (!MatchmakingHandler.Instance.IsInsideLobby)
		{
			Debug.LogError("Got a P2P request when not in lobby, denying request!");
			return;
		}
		CSteamID steamIDRemote = pCallback.m_steamIDRemote;
		if (!MatchmakingHandler.Instance.IsUserInsideMyLobby(steamIDRemote))
		{
			Debug.LogError(string.Concat("Requestee user: ", steamIDRemote, " : ", SteamFriends.GetFriendPersonaName(steamIDRemote), " Wants to establish a p2p connection but is not in lobby, denying..."));
			return;
		}
		bool flag = SteamNetworking.AcceptP2PSessionWithUser(steamIDRemote);
		MonoBehaviour.print(string.Concat("SteamNetworking.AcceptP2PSessionWithUser(", steamIDRemote, ") - ", flag));
		if (SteamFriends.RequestUserInformation(steamIDRemote, false))
		{
		}
		if (flag)
		{
			SendP2PPacketToUser(steamIDRemote, new byte[0], MsgType.Ping);
		}
	}

	private void OnPersoneStateChanged(PersonaStateChange_t pCallback)
	{
		Debug.Log("OnPersoneStateChanged: " + pCallback.m_nChangeFlags);
		int smallFriendAvatar = SteamFriends.GetSmallFriendAvatar(new CSteamID(pCallback.m_ulSteamID));
		if (smallFriendAvatar == 0)
		{
			Debug.LogError("No Image is set for remote user!");
		}
		int smallFriendAvatar2 = SteamFriends.GetSmallFriendAvatar(SteamUser.GetSteamID());
		if (smallFriendAvatar2 == 0)
		{
			Debug.LogError("No Image is set for user!");
		}
		uint num = 8192u;
		byte[] array = new byte[num];
		byte[] pubDest = new byte[num];
		if (!SteamUtils.GetImageRGBA(smallFriendAvatar, array, array.Length))
		{
			Debug.LogError("Image was not stored correctly!");
		}
		if (!SteamUtils.GetImageRGBA(smallFriendAvatar2, pubDest, array.Length))
		{
			Debug.LogError("Image was not stored correctly!");
		}
	}

	private void OnSocketStatusCallback(SocketStatusCallback_t pCallback)
	{
		throw new NotImplementedException();
	}

	private void OnP2PSessionConnectFail(P2PSessionConnectFail_t pCallback)
	{
		EP2PSessionError eP2PSessionError = (EP2PSessionError)pCallback.m_eP2PSessionError;
		Debug.LogError("P2p SessionConnectFail! " + eP2PSessionError);
		if (pCallback.m_eP2PSessionError == 4 && MatchmakingHandler.IsNetworkMatch)
		{
		}
	}
}
