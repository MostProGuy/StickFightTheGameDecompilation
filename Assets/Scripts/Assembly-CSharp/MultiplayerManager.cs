using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LevelEditor;
using Steamworks;
using UnityEngine;
using UnityEngine.Analytics;

public class MultiplayerManager : MonoBehaviour
{
	public enum KickResponse : byte
	{
		DidNotRecievePackages = 0,
		IdleTooLong = 1
	}

	private ConnectedClientData[] mConnectedClients = new ConnectedClientData[4];

	private Dictionary<Vector2, MapInfoSyncableBase> mMapDataObjectToSync = new Dictionary<Vector2, MapInfoSyncableBase>();

	private Dictionary<ushort, WeaponPickUp> mSpawnedWeapons = new Dictionary<ushort, WeaponPickUp>();

	private Dictionary<Vector2, WeaponPickUp> mTempPreSpawnedWeapons = new Dictionary<Vector2, WeaponPickUp>();

	private Dictionary<ushort, NetworkSyncableObject> mSpawnedSyncableObjects = new Dictionary<ushort, NetworkSyncableObject>();

	private Dictionary<Vector2, DestructiblePiece> mDestructiblePieces = new Dictionary<Vector2, DestructiblePiece>();

	private Dictionary<ushort, DestructiblePiece> mDestructiblePiecesRuntime = new Dictionary<ushort, DestructiblePiece>();

	private List<Controller> m_Players = new List<Controller>(4) { null, null, null, null };

	public readonly CSteamID mUnassignedID = (CSteamID)0uL;

	private GameObject m_PlayerPrefab;

	private Material[] m_Colors;

	private GameObject[] m_NetworkSpawnableObjects;

	private static P2PPackageHandler mPacketHandler;

	private static MatchmakingHandler mMatchmakingHandler;

	private static GameManager mGameManager;

	private static OnlinePlayerUI mGameUI;

	private static WeaponSelectionHandler mWeaponSelectionHandler;

	private byte mLocalPlayerIndex;

	private bool mHasBeenInitializedFromServer;

	private bool mHasBeenAcceptedFromServer;

	private WeaponPickUp mDefaultWeaponPickUp;

	private NetworkSyncableObject mDefaultSyncableObject;

	private OnlineBox mOnlineBox;

	public static float k_MAX_SECONDS_UNTIL_AUTO_START
	{
		get
		{
			return 3f;
		}
	}

	public ConnectedClientData[] ConnectedClients
	{
		get
		{
			return mConnectedClients;
		}
	}

	public List<Rigidbody> SpawnedWeapons
	{
		get
		{
			List<Rigidbody> list = new List<Rigidbody>();
			foreach (KeyValuePair<Vector2, WeaponPickUp> mTempPreSpawnedWeapon in mTempPreSpawnedWeapons)
			{
				if (!(mTempPreSpawnedWeapon.Value == null))
				{
					Rigidbody component = mTempPreSpawnedWeapon.Value.GetComponent<Rigidbody>();
					list.Add(component);
				}
			}
			foreach (KeyValuePair<ushort, WeaponPickUp> mSpawnedWeapon in mSpawnedWeapons)
			{
				if (!(mSpawnedWeapon.Value == null))
				{
					Rigidbody component = mSpawnedWeapon.Value.GetComponent<Rigidbody>();
					list.Add(component);
				}
			}
			return list;
		}
	}

	public List<Controller> PlayerControllers
	{
		get
		{
			return m_Players;
		}
	}

	public static bool IsServer
	{
		get
		{
			if (MatchmakingHandler.RunningOnSockets)
			{
				return MatchMakingHandlerSockets.IsServer;
			}
			return mMatchmakingHandler != null && mMatchmakingHandler.IsHost;
		}
	}

	public static bool IsAllowedToChangeOptions
	{
		get
		{
			return IsServer && MatchmakingHandler.LobbyType != ELobbyType.k_ELobbyTypePublic;
		}
	}

	private CSteamID mServerID
	{
		get
		{
			return mMatchmakingHandler.LobbyOwner;
		}
	}

	public byte LocalPlayerIndex
	{
		get
		{
			return mLocalPlayerIndex;
		}
	}

	public static uint LastTimeStamp { get; private set; }

	public bool HasBeenInitializedFromServer
	{
		get
		{
			return mHasBeenInitializedFromServer;
		}
	}

	public NetworkSyncableObject DefaultSyncableObject
	{
		get
		{
			return mDefaultSyncableObject;
		}
	}

	public NetworkSyncableObject GetSyncedObject(ushort networkIndex)
	{
		NetworkSyncableObject value = null;
		mSpawnedSyncableObjects.TryGetValue(networkIndex, out value);
		return value;
	}

	public int GetPlayersInLobby(bool excludeSelf = false)
	{
		int num = 0;
		ConnectedClientData[] array = mConnectedClients;
		foreach (ConnectedClientData connectedClientData in array)
		{
			if (connectedClientData != null && connectedClientData.ClientID.IsValid())
			{
				num++;
			}
		}
		if (excludeSelf)
		{
			num--;
		}
		return num;
	}

	private void Awake()
	{
		MatchmakingHandler.SetNetworkMatch(false);
		mMatchmakingHandler = MatchmakingHandler.Instance;
		mWeaponSelectionHandler = UnityEngine.Object.FindObjectOfType<WeaponSelectionHandler>();
		mPacketHandler = P2PPackageHandler.Instance;
		mGameManager = GameManager.Instance;
		mGameUI = UnityEngine.Object.FindObjectOfType<OnlinePlayerUI>();
		mDefaultWeaponPickUp = base.gameObject.FetchComponent<WeaponPickUp>();
		mDefaultWeaponPickUp.enabled = false;
		mDefaultSyncableObject = base.gameObject.FetchComponent<NetworkSyncableObject>();
		mDefaultSyncableObject.enabled = false;
		LastTimeStamp = 0u;
		PingHandler.Init(this);
	}

	private void Update()
	{
		if (Application.isEditor && Input.GetKeyDown(KeyCode.P))
		{
			PingAllUsers();
		}
	}

	private void Start()
	{
		MultiplayerManagerAssets instance = MultiplayerManagerAssets.Instance;
		m_PlayerPrefab = instance.PlayerPrefab;
		m_Colors = instance.Colors;
		mMatchmakingHandler = MatchmakingHandler.Instance;
		mPacketHandler = P2PPackageHandler.Instance;
		mGameManager = GameManager.Instance;
		m_NetworkSpawnableObjects = ResourcesManager.Instance.NetworkSpawnableObjects;
		mPacketHandler.Init();
		mMatchmakingHandler.Disconnect(false);
	}

	public void OnPlayerJoined(CSteamID SteamID)
	{
		Debug.Log("Player Joined!" + SteamFriends.GetFriendPersonaName(SteamID));
		mPacketHandler.SendP2PPacketToUser(SteamID, new byte[0], P2PPackageHandler.MsgType.Ping);
	}

	public void SpawnWeapon(int weaponId, Vector3 spawnPoint, bool spawnAsPresent = false)
	{
		if (!IsServer)
		{
			Debug.LogError("Client Is Trying to call Server Functions, intended?");
		}
		sbyte value = (sbyte)spawnPoint.y;
		sbyte value2 = (sbyte)spawnPoint.z;
		ushort nextWeaponSpawnID = GetNextWeaponSpawnID();
		ushort nextSyncableObjectSpawnID = GetNextSyncableObjectSpawnID();
		byte[] array = new byte[8];
		using (MemoryStream output = new MemoryStream(array))
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(output))
			{
				binaryWriter.Write((byte)weaponId);
				binaryWriter.Write(value);
				binaryWriter.Write(value2);
				binaryWriter.Write(nextWeaponSpawnID);
				binaryWriter.Write(nextSyncableObjectSpawnID);
				binaryWriter.Write((byte)(spawnAsPresent ? 1 : 0));
			}
		}
		SendMessageToAllClients(array, P2PPackageHandler.MsgType.WeaponSpawned, false, 0uL);
	}

	public void AddDestructiblePiece(Vector2 pos, DestructiblePiece piece)
	{
		bool flag = false;
		if (mDestructiblePieces.ContainsKey(pos))
		{
			Debug.Log("Des Pice Already In List! Removing: Pos: " + pos);
			DestructiblePiece value;
			mDestructiblePieces.TryGetValue(pos, out value);
			UnityEngine.Object.Destroy(value.gameObject);
			mDestructiblePieces.Remove(pos);
			flag = true;
		}
		mDestructiblePieces.Add(pos, piece);
	}

	public void AddDestructiblePiece(ushort index, DestructiblePiece piece)
	{
		bool flag = false;
		if (mDestructiblePiecesRuntime.ContainsKey(index))
		{
			DestructiblePiece value;
			mDestructiblePiecesRuntime.TryGetValue(index, out value);
			UnityEngine.Object.Destroy(value.gameObject);
			mDestructiblePiecesRuntime.Remove(index);
			flag = true;
		}
		mDestructiblePiecesRuntime.Add(index, piece);
	}

	public void InvokeDestructionEvent(Vector2 pos)
	{
		byte[] array = new byte[8];
		using (MemoryStream output = new MemoryStream(array))
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(output))
			{
				binaryWriter.Write(pos.x);
				binaryWriter.Write(pos.y);
			}
		}
		if (IsServer)
		{
			SendMessageToAllClients(array, P2PPackageHandler.MsgType.ObjectInvokeDestructionEvent, false, 0uL);
		}
		else
		{
			mPacketHandler.SendP2PPacketToServer(array, P2PPackageHandler.MsgType.ObjectInvokeDestructionEvent);
		}
	}

	public void AddSyncableObject(ushort index, NetworkSyncableObject syncObject)
	{
		NetworkSyncableObject value;
		if (mSpawnedSyncableObjects.ContainsKey(index) && mSpawnedSyncableObjects.TryGetValue(index, out value))
		{
			if (value != null && value != syncObject && value != mDefaultSyncableObject && value.gameObject != null)
			{
				Debug.LogError("Destroying Syncable object: " + index + " : " + value.name);
				UnityEngine.Object.Destroy(value.gameObject);
			}
			mSpawnedSyncableObjects.Remove(index);
		}
		mSpawnedSyncableObjects.Add(index, syncObject);
	}

	public void OnPlayerBlockedAddedForce(byte[] data, int channel)
	{
		ulong steamID = SteamUser.GetSteamID().m_SteamID;
		SendMessageToAllClients(data, P2PPackageHandler.MsgType.PlayerForceAddedAndBlock, false, steamID, EP2PSend.k_EP2PSendReliable, channel);
	}

	public void AddPreSpawnedWeapon(Vector2 pos, WeaponPickUp weapon)
	{
		if (weapon != mDefaultWeaponPickUp)
		{
			mTempPreSpawnedWeapons.Add(pos, weapon);
		}
	}

	public void CheckForGroundWeapons()
	{
		if (!IsServer)
		{
			return;
		}
		ushort num = (ushort)mTempPreSpawnedWeapons.Count;
		Debug.Log("Checking for Ground Weapons! Found: " + num + " Weapons!");
		if (num <= 0)
		{
			return;
		}
		byte[] array = new byte[2 + 12 * num];
		using (MemoryStream output = new MemoryStream(array))
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(output))
			{
				binaryWriter.Write(num);
				foreach (KeyValuePair<Vector2, WeaponPickUp> mTempPreSpawnedWeapon in mTempPreSpawnedWeapons)
				{
					ushort nextWeaponSpawnID = GetNextWeaponSpawnID();
					ushort nextSyncableObjectSpawnID = GetNextSyncableObjectSpawnID(true);
					Vector2 key = mTempPreSpawnedWeapon.Key;
					binaryWriter.Write(key.x);
					binaryWriter.Write(key.y);
					binaryWriter.Write(nextWeaponSpawnID);
					binaryWriter.Write(nextSyncableObjectSpawnID);
					Debug.Log(string.Concat("Writing GroundWeapon With Pos: ", key, " And sync index: ", nextSyncableObjectSpawnID));
				}
			}
		}
		SendMessageToAllClients(array, P2PPackageHandler.MsgType.GroundWeaponsInit, false, 0uL);
	}

	public void DoBlockForPlayer(byte playerIndex)
	{
		GameObject playerObject = mConnectedClients[playerIndex].PlayerObject;
		if ((bool)playerObject)
		{
			playerObject.GetComponentInChildren<BlockHandler>().DoBlock();
		}
	}

	public void OnGroundWeaponsInit(byte[] data)
	{
		ushort num = 0;
		using (MemoryStream input = new MemoryStream(data))
		{
			using (BinaryReader binaryReader = new BinaryReader(input))
			{
				num = binaryReader.ReadUInt16();
				Vector2 vector = default(Vector2);
				for (int i = 0; i < num; i++)
				{
					vector.x = binaryReader.ReadSingle();
					vector.y = binaryReader.ReadSingle();
					ushort weaponSpawnID = binaryReader.ReadUInt16();
					ushort num2 = binaryReader.ReadUInt16();
					WeaponPickUp value;
					if (mTempPreSpawnedWeapons.TryGetValue(vector, out value))
					{
						Debug.Log("Initint Ground weapon with index: " + num2);
						UnityEngine.Object.Destroy(value.transform.GetComponent<NetworkSyncableObject>());
						OnWeaponSpawned(value, weaponSpawnID, num2);
					}
					else
					{
						Debug.LogError("Could Not Find GroundWeapon With Position: " + vector);
					}
				}
			}
		}
	}

	public void ReadyUp()
	{
		if (IsServer)
		{
			ConnectedClientData[] array = mConnectedClients;
			foreach (ConnectedClientData connectedClientData in array)
			{
				if (connectedClientData != null && connectedClientData.ClientID.IsValid() && connectedClientData.ControlledLocally)
				{
					connectedClientData.Ready = true;
				}
			}
			CheckReadyPlayers();
			return;
		}
		List<byte> list = new List<byte>();
		for (byte b = 0; b < mConnectedClients.Length; b++)
		{
			ConnectedClientData connectedClientData2 = mConnectedClients[b];
			if (connectedClientData2 != null && connectedClientData2.ControlledLocally)
			{
				list.Add(b);
			}
		}
		list.Insert(0, (byte)list.Count);
		mPacketHandler.SendP2PPacketToServer(list.ToArray(), P2PPackageHandler.MsgType.ClientReadyUp);
	}

	public void KickPlayer(ushort spawnID, KickResponse response)
	{
		if (IsServer)
		{
			CSteamID clientID = mConnectedClients[spawnID].ClientID;
			byte[] data = new byte[1] { (byte)response };
			mPacketHandler.SendP2PPacketToUser(clientID, data, P2PPackageHandler.MsgType.KickPlayer);
		}
	}

	public void UpdateUI()
	{
		if ((bool)mGameUI)
		{
			mGameUI.OnUpdated();
		}
	}

	public void RequestWeaponPickUp(ushort weaponSpawnID, byte Index)
	{
		if (IsServer)
		{
			WeaponPickUp value;
			if (!mSpawnedWeapons.TryGetValue(weaponSpawnID, out value) || value == mDefaultWeaponPickUp)
			{
				return;
			}
			GameObject playerObject = mConnectedClients[Index].PlayerObject;
			Fighting component = playerObject.GetComponent<Fighting>();
			component.PickUpWeapon(value.id, value.gameObject);
			if (value.sendTheMovementAbility != -1)
			{
				component.SetMovementAbility(value.sendTheMovementAbility);
			}
			if (!value.unEnding)
			{
				UnityEngine.Object.Destroy(value.gameObject);
			}
			component.GetComponent<CharacterStats>().weaponsPickedUp++;
			byte[] array = new byte[3];
			using (MemoryStream output = new MemoryStream(array))
			{
				using (BinaryWriter binaryWriter = new BinaryWriter(output))
				{
					binaryWriter.Write(Index);
					binaryWriter.Write(weaponSpawnID);
				}
			}
			SendMessageToAllClients(array, P2PPackageHandler.MsgType.WeaponWasPickedUp, true, 0uL);
			return;
		}
		byte[] array2 = new byte[3];
		using (MemoryStream output2 = new MemoryStream(array2))
		{
			using (BinaryWriter binaryWriter2 = new BinaryWriter(output2))
			{
				binaryWriter2.Write(Index);
				binaryWriter2.Write(weaponSpawnID);
			}
		}
		mPacketHandler.SendP2PPacketToServer(array2, P2PPackageHandler.MsgType.ClientRequestingWeaponPickUp);
	}

	public void SendObjectHello(byte[] data, int channel)
	{
		SendMessageToAllClients(data, P2PPackageHandler.MsgType.ObjectHello, true, 0uL, EP2PSend.k_EP2PSendReliable, channel);
	}

	public void SyncObjectHello(byte[] v, int mChannel)
	{
		if (IsServer)
		{
			Debug.LogError("Server is trying to call client function, intended?");
		}
		mPacketHandler.SendP2PPacketToServer(v, P2PPackageHandler.MsgType.ObjectHello, EP2PSend.k_EP2PSendReliable, mChannel);
	}

	public void OnObjectSpawned(byte[] data)
	{
		Vector3 zero = Vector3.zero;
		Vector3 zero2 = Vector3.zero;
		bool flag = false;
		ushort num = 0;
		bool flag2 = false;
		ushort num2 = 0;
		ushort objectIndex;
		using (MemoryStream input = new MemoryStream(data))
		{
			using (BinaryReader binaryReader = new BinaryReader(input))
			{
				objectIndex = binaryReader.ReadUInt16();
				zero.y = binaryReader.ReadSingle();
				zero.z = binaryReader.ReadSingle();
				zero2.x = binaryReader.ReadSingle();
				zero2.y = binaryReader.ReadSingle();
				zero2.z = binaryReader.ReadSingle();
				SpawnableObjectType a = (SpawnableObjectType)binaryReader.ReadByte();
				flag = HasFlag(a, SpawnableObjectType.ShallSyncPosition);
				if (flag)
				{
					num = binaryReader.ReadUInt16();
				}
				flag2 = HasFlag(a, SpawnableObjectType.Weapon);
				if (flag2)
				{
					num2 = binaryReader.ReadUInt16();
				}
			}
		}
		GameObject spawnableNetworkObjectByIndex = GetSpawnableNetworkObjectByIndex(objectIndex);
		GameObject gameObject = UnityEngine.Object.Instantiate(spawnableNetworkObjectByIndex, zero, Quaternion.Euler(zero2));
		if (flag)
		{
			NetworkSyncableObject networkSyncableObject = gameObject.FetchComponent<NetworkSyncableObject>();
			bool syncRotation = !flag2;
			networkSyncableObject.InitNetworkIndex(num, syncRotation);
			AddSyncableObject(num, networkSyncableObject);
			if (flag2)
			{
				networkSyncableObject.mLerpFriction = 0.8f;
				networkSyncableObject.mDirectionFractor = 0.04f;
			}
		}
		if (!flag2)
		{
			return;
		}
		WeaponPickUp component = gameObject.GetComponent<WeaponPickUp>();
		component.InitNetwork(num2);
		if (mSpawnedWeapons.ContainsKey(num2))
		{
			WeaponPickUp value;
			if (mSpawnedWeapons.TryGetValue(num2, out value) && value != null && value != mDefaultWeaponPickUp)
			{
				Debug.LogError("Destroying Weapon With Id: " + num2 + " : " + value.name);
				UnityEngine.Object.Destroy(value.gameObject);
			}
			mSpawnedWeapons.Remove(num2);
		}
		try
		{
			mSpawnedWeapons.Add(num2, component);
		}
		catch (ArgumentException)
		{
			Debug.Log("knark");
		}
	}

	public void InitSyncedObjects()
	{
		NetworkSyncableObject[] array = UnityEngine.Object.FindObjectsOfType<NetworkSyncableObject>();
		int num = array.Length;
		for (int i = 0; i < num; i++)
		{
			NetworkSyncableObject networkSyncableObject = array[i];
			if (!networkSyncableObject.GetComponent<WeaponPickUp>() && networkSyncableObject != mDefaultSyncableObject)
			{
				networkSyncableObject.Init();
			}
		}
	}

	public void OnWeaponDropped(byte[] data)
	{
		Vector3 zero = Vector3.zero;
		Vector3 zero2 = Vector3.zero;
		byte b;
		ushort weaponSpawnID;
		ushort syncIndex;
		using (MemoryStream input = new MemoryStream(data))
		{
			using (BinaryReader binaryReader = new BinaryReader(input))
			{
				b = binaryReader.ReadByte();
				zero.y = (float)binaryReader.ReadInt16() / 100f;
				zero.z = (float)binaryReader.ReadInt16() / 100f;
				zero2.y = (float)binaryReader.ReadSByte() / 100f;
				zero2.z = (float)binaryReader.ReadSByte() / 100f;
				weaponSpawnID = binaryReader.ReadUInt16();
				syncIndex = binaryReader.ReadUInt16();
			}
		}
		int num = b;
		GameObject original = mGameManager.FindWeaponByIndex(num - 1);
		GameObject gameObject = UnityEngine.Object.Instantiate(original, zero, Quaternion.LookRotation(zero2));
		OnWeaponSpawned(gameObject.GetComponent<WeaponPickUp>(), weaponSpawnID, syncIndex);
		Rigidbody component = gameObject.GetComponent<Rigidbody>();
		gameObject.GetComponent<ConstantForce>().enabled = false;
		component.maxAngularVelocity = 100f;
		component.AddForce(Vector3.up * 25f, ForceMode.VelocityChange);
		component.AddTorque(Vector3.right * 50f, ForceMode.VelocityChange);
		Debug.Log("Dropping Weapon with index: " + b + " Name: " + gameObject.name);
	}

	public void OnPlayerRequestingWeaponDrop(byte[] data)
	{
		ushort nextWeaponSpawnID = GetNextWeaponSpawnID();
		ushort nextSyncableObjectSpawnID = GetNextSyncableObjectSpawnID();
		byte[] array = new byte[data.Length + 4];
		using (MemoryStream output = new MemoryStream(array))
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(output))
			{
				binaryWriter.Write(data);
				binaryWriter.Write(nextWeaponSpawnID);
				binaryWriter.Write(nextSyncableObjectSpawnID);
			}
		}
		Debug.Log("Client is requesting to drop weapon: Found New SpawnID, returning: " + nextWeaponSpawnID);
		SendMessageToAllClients(array, P2PPackageHandler.MsgType.WeaponDropped, false, 0uL);
	}

	public void OnPlayerFallOut(byte[] data, int channel, ushort ignore = ushort.MaxValue)
	{
		ulong ignoreUserID = ((ignore != ushort.MaxValue) ? mConnectedClients[ignore].ClientID.m_SteamID : 0);
		SendMessageToAllClients(data, P2PPackageHandler.MsgType.PlayerFallOut, false, ignoreUserID, EP2PSend.k_EP2PSendReliable, channel);
	}

	public void OnPlayerRequestingWeaponPickUp(byte[] data)
	{
		byte b;
		ushort num;
		using (MemoryStream input = new MemoryStream(data))
		{
			using (BinaryReader binaryReader = new BinaryReader(input))
			{
				b = binaryReader.ReadByte();
				num = binaryReader.ReadUInt16();
			}
		}
		Debug.Log("Player: " + b + "Is requesting to pickuo weapon: " + num);
		WeaponPickUp value;
		if (mSpawnedWeapons.TryGetValue(num, out value) && value != null && value != mDefaultWeaponPickUp)
		{
			SendMessageToAllClients(data, P2PPackageHandler.MsgType.WeaponWasPickedUp, false, 0uL);
		}
	}

	public void SpawnObject(GameObject objectToSpawn, Vector3 position, float rotation, bool syncPosition)
	{
		SpawnObject(objectToSpawn, position, new Vector3(rotation, 0f, 0f), syncPosition);
	}

	public void SpawnObject(GameObject objectToSpawn, Vector3 position, Vector3 eulerAngles, bool syncPosition)
	{
		SpawnableObjectType spawnableObjectType = (syncPosition ? SpawnableObjectType.ShallSyncPosition : SpawnableObjectType.Default);
		ushort value = FindIndexOfNetworkObject(objectToSpawn);
		ushort value2 = 0;
		if (spawnableObjectType == SpawnableObjectType.ShallSyncPosition)
		{
			value2 = GetNextSyncableObjectSpawnID();
		}
		bool flag = objectToSpawn.GetComponent<WeaponPickUp>();
		ushort value3 = 0;
		if (flag)
		{
			value3 = GetNextWeaponSpawnID();
			spawnableObjectType |= SpawnableObjectType.Weapon;
		}
		byte[] array = new byte[23 + (syncPosition ? 2 : 0) + (flag ? 2 : 0)];
		using (MemoryStream output = new MemoryStream(array))
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(output))
			{
				binaryWriter.Write(value);
				binaryWriter.Write(position.y);
				binaryWriter.Write(position.z);
				binaryWriter.Write(eulerAngles.x);
				binaryWriter.Write(eulerAngles.y);
				binaryWriter.Write(eulerAngles.z);
				binaryWriter.Write((byte)spawnableObjectType);
				if (syncPosition)
				{
					binaryWriter.Write(value2);
				}
				if (flag)
				{
					binaryWriter.Write(value3);
				}
			}
		}
		SendMessageToAllClients(array, P2PPackageHandler.MsgType.ObjectSpawned, false, 0uL);
	}

	private GameObject GetSpawnableNetworkObjectByIndex(ushort objectIndex)
	{
		if (objectIndex > m_NetworkSpawnableObjects.Length - 1)
		{
			throw new Exception("Object Index is Too High! " + objectIndex + " Only: " + m_NetworkSpawnableObjects.Length + " Objects in list!");
		}
		return m_NetworkSpawnableObjects[objectIndex];
	}

	private ushort FindIndexOfNetworkObject(GameObject objectToSpawn)
	{
		int num = m_NetworkSpawnableObjects.Length;
		for (ushort num2 = 0; num2 < num; num2++)
		{
			GameObject gameObject = m_NetworkSpawnableObjects[num2];
			if (gameObject.name == objectToSpawn.name)
			{
				return num2;
			}
		}
		throw new Exception("Could not spawn Object: " + objectToSpawn.name + " On the network, was not found in the list");
	}

	private ushort GetNextWeaponSpawnID(bool beginFromEnd = false)
	{
		CheckForNullWeapons();
		int num = 65534;
		ushort num2 = ((!beginFromEnd) ? ((ushort)mSpawnedWeapons.Count) : ((ushort)num));
		bool flag = false;
		while (!flag)
		{
			if (!mSpawnedWeapons.ContainsKey(num2))
			{
				flag = true;
				break;
			}
			num2 = ((!beginFromEnd) ? ((ushort)(num2 + 1)) : ((ushort)(num2 - 1)));
		}
		mSpawnedWeapons.Add(num2, mDefaultWeaponPickUp);
		return num2;
	}

	private ushort GetNextRuntimeDestructiblePieceSpawnID()
	{
		for (ushort num = 0; num < ushort.MaxValue; num++)
		{
			if (!mDestructiblePiecesRuntime.ContainsKey(num))
			{
				return num;
			}
		}
		throw new Exception("Could not find a valid SpawnINdex For Next Runtime Destructible Piece!");
	}

	public ushort GetNextSyncableObjectSpawnID(bool beginFromEnd = false)
	{
		int num = 65534;
		ushort num2 = ((!beginFromEnd) ? ((ushort)mSpawnedSyncableObjects.Count) : ((ushort)num));
		bool flag = false;
		while (!flag)
		{
			if (!mSpawnedSyncableObjects.ContainsKey(num2))
			{
				flag = true;
				break;
			}
			num2 = ((!beginFromEnd) ? ((ushort)(num2 + 1)) : ((ushort)(num2 - 1)));
		}
		try
		{
			mSpawnedSyncableObjects.Add(num2, mDefaultSyncableObject);
		}
		catch (ArgumentException)
		{
			Debug.Log("Key Already Exists!");
		}
		return num2;
	}

	private void CheckForNullWeapons()
	{
	}

	private void CheckForNullSyncableObjects()
	{
	}

	public void OnPlayerLeft(CSteamID SteamID)
	{
		Debug.Log("Player Left!" + SteamFriends.GetFriendPersonaName(SteamID));
		bool flag = RemoveClientFromList(SteamID);
		if (SteamNetworking.CloseP2PSessionWithUser(SteamID))
		{
			Debug.Log("Successfully Closed P2P Connection With Steam User: " + SteamID);
		}
		else
		{
			Debug.Log("Failed To Close P2P Connection With Steam User: " + SteamID);
		}
		if (!(mServerID == SteamUser.GetSteamID()))
		{
			return;
		}
		Debug.Log("Server LEFT The Game, Changing Host...");
		Debug.Log(string.Concat("New Host: ", mServerID, " IS ME?: ", mServerID == SteamUser.GetSteamID()));
		if (!IsServer)
		{
			return;
		}
		if (!GameManager.inFight)
		{
			CheckReadyPlayers();
		}
		foreach (KeyValuePair<ushort, NetworkSyncableObject> mSpawnedSyncableObject in mSpawnedSyncableObjects)
		{
			mSpawnedSyncableObject.Value.UpdateHost();
		}
		SendCustomMapsCycleToClients();
	}

	private void SendCustomMapsCycleToClients()
	{
		LevelSelection levelSelection = UnityEngine.Object.FindObjectOfType<LevelSelection>();
		levelSelection.MakeNewWorkshopLevelCycle();
		PlayableWorkshopLevel[] allCustomMapsActive = levelSelection.GetAllCustomMapsActive(false);
		ushort num = (ushort)allCustomMapsActive.Length;
		if (num <= 0)
		{
			return;
		}
		byte[] array = new byte[2 + 8 * num];
		using (MemoryStream output = new MemoryStream(array))
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(output))
			{
				binaryWriter.Write(num);
				for (int i = 0; i < num; i++)
				{
					binaryWriter.Write(allCustomMapsActive[i].MapID);
				}
			}
		}
		SendMessageToAllClients(array, P2PPackageHandler.MsgType.WorkshopMapsLoaded, true, 0uL);
	}

	public void OnNewWorkshopMapsRecieved(byte[] mapData)
	{
		using (MemoryStream input = new MemoryStream(mapData))
		{
			using (BinaryReader binaryReader = new BinaryReader(input))
			{
				ushort num = binaryReader.ReadUInt16();
				ulong[] array = new ulong[num];
				for (int i = 0; i < num; i++)
				{
					array[i] = binaryReader.ReadUInt64();
				}
				if (num > 0)
				{
					bool flag = WorkshopMapsLoader.Instance.NewMapCycleLoaded(array, null);
					Debug.Log("New Workshop Cycle Loaded: NeedsToDownloadMaps? " + flag);
				}
			}
		}
	}

	public void OnPlayerKicked(CSteamID SteamID)
	{
		Debug.Log("Player Kicked!" + SteamFriends.GetFriendPersonaName(SteamID));
	}

	public void OnMapChanged(byte[] data)
	{
		Debug.Log("OnMapChanged");
		ClearWeapons();
		ClearDestructiblePieces();
		ClearSyncableObjects();
		ClearMapDataObjects();
		GameManager.Instance.isLoading = true;
		WorkshopMapsLoader.Instance.m_lastMapNeededDownloading = false;
		MapWrapper map = new MapWrapper();
		byte winner;
		using (MemoryStream input = new MemoryStream(data))
		{
			using (BinaryReader binaryReader = new BinaryReader(input))
			{
				winner = binaryReader.ReadByte();
				map.MapType = binaryReader.ReadByte();
				map.MapData = binaryReader.ReadBytes(data.Length - 2);
			}
		}
		Action callback = () =>
		{
			mGameManager.LoadMapCourotine(map);
		};
		switch ((MapType)map.MapType)
		{
		case MapType.Landfall:
			Debug.Log("MapType.Landfall");
			break;
		case MapType.CustomLocal:
			throw new Exception("Fail");
		case MapType.CustomOnline:
		{
			ulong map2 = BitConverter.ToUInt64(map.MapData, 0);
			WorkshopMapsLoader.Instance.CheckNewWorkshopMaps(map2, callback, true);
			Debug.Log("MapType.CustomOnline");
			break;
		}
		}
		mGameManager.NetworkAllPlayersDiedButOne(map, winner);
		CheckForDisconnectedPlayers();
		LastTimeStamp = SteamUtils.GetServerRealTime();
	}

	private void CheckForDisconnectedPlayers()
	{
		ConnectedClientData[] array = mConnectedClients;
		foreach (ConnectedClientData connectedClientData in array)
		{
			if (connectedClientData != null && !(connectedClientData.ClientID == mUnassignedID) && !mMatchmakingHandler.IsUserInsideMyLobby(connectedClientData.ClientID))
			{
				Debug.LogError("Checked For disconnected players! Found one, removing...");
				RemoveClientFromList(connectedClientData.ClientID);
			}
		}
	}

	private void ClearWeapons()
	{
		mSpawnedWeapons.Clear();
		mTempPreSpawnedWeapons.Clear();
	}

	private void ClearSyncableObjects()
	{
		mSpawnedSyncableObjects.Clear();
	}

	private void ClearDestructiblePieces()
	{
		int count = mDestructiblePieces.Count;
		mDestructiblePieces.Clear();
		mDestructiblePiecesRuntime.Clear();
		Debug.Log("Removed: " + count + " Des Pieces From last map!");
	}

	private void ClearMapDataObjects()
	{
		mMapDataObjectToSync.Clear();
	}

	public void ChangeMap(MapWrapper nextLevel, byte indexOfWinner)
	{
		if (!IsServer)
		{
			Debug.LogError("Client Is Trying To Call Server Function, Intended?");
			return;
		}
		UnReadyAllPlayers();
		byte[] array = new byte[2 + nextLevel.MapData.Length];
		using (MemoryStream output = new MemoryStream(array))
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(output))
			{
				binaryWriter.Write(indexOfWinner);
				binaryWriter.Write(nextLevel.MapType);
				binaryWriter.Write(nextLevel.MapData);
			}
		}
		SendMessageToAllClients(array, P2PPackageHandler.MsgType.MapChange, false, 0uL);
	}

	private void UnReadyAllPlayers()
	{
		ConnectedClientData[] array = mConnectedClients;
		foreach (ConnectedClientData connectedClientData in array)
		{
			if (connectedClientData != null && connectedClientData.ClientID.IsValid())
			{
				connectedClientData.Ready = false;
			}
		}
	}

	private void SpawnPlayer()
	{
		Vector3 vector = new Vector3(0f, 0f, 0f);
		Vector3 eulerAngles = Quaternion.identity.eulerAngles;
		byte[] array = new byte[26];
		using (MemoryStream output = new MemoryStream(array))
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(output))
			{
				binaryWriter.Write(mLocalPlayerIndex);
				binaryWriter.Write(vector.x);
				binaryWriter.Write(vector.y);
				binaryWriter.Write(vector.z);
				binaryWriter.Write(eulerAngles.x);
				binaryWriter.Write(eulerAngles.y);
				binaryWriter.Write(eulerAngles.z);
			}
		}
		Debug.Log(string.Concat("Sending Request to spawn to the server at pos: ", vector, " Rotation: ", eulerAngles));
		SendMessageToAllClients(array, P2PPackageHandler.MsgType.ClientSpawned, false, 0uL);
	}

	public void OnSceneStarted()
	{
		MatchmakingHandler.SetNetworkMatch(true);
		LastTimeStamp = (SteamManager.Initialized ? SteamUtils.GetServerRealTime() : 0u);
		int num = ((!SteamManager.Initialized) ? 4 : SteamMatchmaking.GetLobbyMemberLimit(MatchmakingHandler.Instance.CurrentLobby));
		ClearWeapons();
		ClearDestructiblePieces();
		ClearSyncableObjects();
		mConnectedClients = new ConnectedClientData[num];
		UnityEngine.Object.FindObjectOfType<LevelSelection>().RemoveAllLocalMapsFromWorkshopCycle();
		if (IsServer)
		{
			StartCoroutine(HostInit());
		}
		else
		{
			RequestClientInit();
		}
	}

	private void RequestClientInit()
	{
		bool flag = false;
		CSteamID currentLobby = mMatchmakingHandler.CurrentLobby;
		int numLobbyMembers = SteamMatchmaking.GetNumLobbyMembers(currentLobby);
		for (int i = 0; i < numLobbyMembers; i++)
		{
			CSteamID lobbyMemberByIndex = SteamMatchmaking.GetLobbyMemberByIndex(currentLobby, i);
			P2PSessionState_t pConnectionState;
			if (!(lobbyMemberByIndex == SteamUser.GetSteamID()) && SteamNetworking.GetP2PSessionState(lobbyMemberByIndex, out pConnectionState))
			{
				Debug.Log("Connection To User: " + i + " : Connecting: " + pConnectionState.m_bConnecting + " : Connection Active: " + pConnectionState.m_bConnectionActive + " : Using Relay: " + pConnectionState.m_bUsingRelay);
				if (pConnectionState.m_bConnectionActive == 0)
				{
					flag = false;
				}
			}
		}
		Debug.Log("Requesting Client Init");
		mPacketHandler.SendP2PPacketToServer(new byte[0], P2PPackageHandler.MsgType.ClientRequestingAccepting);
		StartCoroutine(TimeOutAfterSecondsIfNotAccepted(5f));
	}

	private IEnumerator TimeOutAfterSecondsIfNotAccepted(float v)
	{
		float time = Time.time;
		float maxOut = time + v;
		time = 1f;
		while (Time.time < maxOut && !mHasBeenAcceptedFromServer)
		{
			if (time >= 1f)
			{
				mPacketHandler.SendP2PPacketToServer(new byte[0], P2PPackageHandler.MsgType.ClientRequestingAccepting);
				Debug.Log("Notice me senpai...");
				time = 0f;
			}
			time += Time.deltaTime;
			yield return null;
		}
		if (!mHasBeenAcceptedFromServer)
		{
			mMatchmakingHandler.TryReconnect();
		}
	}

	private IEnumerator TimeOutAfterSecondsIfNotInitialized(float v)
	{
		float time = Time.time;
		float maxOut = time + v;
		while (Time.time < maxOut && !mHasBeenInitializedFromServer)
		{
			yield return null;
		}
		if (!mHasBeenInitializedFromServer)
		{
			mMatchmakingHandler.TryReconnect();
		}
	}

	private void CleanUpPlayers()
	{
		if (mConnectedClients == null)
		{
			return;
		}
		ConnectedClientData[] array = mConnectedClients;
		foreach (ConnectedClientData connectedClientData in array)
		{
			if (connectedClientData != null && !(connectedClientData.PlayerObject == null))
			{
				UnityEngine.Object.Destroy(connectedClientData.PlayerObject);
			}
		}
	}

	private IEnumerator HostInit()
	{
		mGameUI.Init();
		OnlineBox.Joined();
		if ((bool)mOnlineBox)
		{
			mGameManager.DisableAllPlayers();
			mOnlineBox.PlayBoxAnimation();
			yield return new WaitForSeconds(3f);
			UnityEngine.Object.FindObjectOfType<LoadingScreenManager>().LoadForSeconds(3f);
			yield return new WaitForSeconds(0.5f);
		}
		OnlineRoom room = UnityEngine.Object.FindObjectOfType<OnlineRoom>();
		room.GoBack();
		if (MatchmakingHandler.LobbyType != ELobbyType.k_ELobbyTypePublic)
		{
			HostLeverHandler hostLeverHandler = UnityEngine.Object.FindObjectOfType<HostLeverHandler>();
			if ((bool)hostLeverHandler)
			{
				hostLeverHandler.SetLeverActive();
			}
		}
		int playersHere = mGameManager.KillAllPlayers(true);
		for (int i = 0; i < playersHere; i++)
		{
			AddClientToList(mServerID, false, true);
		}
		if (playersHere == 1)
		{
			SpawnPlayer();
		}
		else
		{
			SpawnAllPlayersOnThisMachine();
		}
		UnityEngine.Object.FindObjectOfType<OnlinePlayerUI>().TextStay();
		MatchmakingHandler.Instance.SetLobbyJoinable();
	}

	private void SpawnAllPlayersOnThisMachine()
	{
		int num = mConnectedClients.Length;
		for (byte b = 0; b < num; b++)
		{
			ConnectedClientData connectedClientData = mConnectedClients[b];
			if (connectedClientData != null && connectedClientData.ControlledLocally)
			{
				Vector3 vector = new Vector3(0f, 0f, 0f);
				Vector3 eulerAngles = Quaternion.identity.eulerAngles;
				byte[] array = new byte[26];
				using (MemoryStream output = new MemoryStream(array))
				{
					using (BinaryWriter binaryWriter = new BinaryWriter(output))
					{
						binaryWriter.Write(b);
						binaryWriter.Write(vector.x);
						binaryWriter.Write(vector.y);
						binaryWriter.Write(vector.z);
						binaryWriter.Write(eulerAngles.x);
						binaryWriter.Write(eulerAngles.y);
						binaryWriter.Write(eulerAngles.z);
						binaryWriter.Write(false);
					}
				}
				Debug.Log(string.Concat("Sending Request to spawn to the server at pos: ", vector, " Rotation: ", eulerAngles));
				SendMessageToAllClients(array, P2PPackageHandler.MsgType.ClientSpawned, false, 0uL);
			}
		}
	}

	public void AddMapDataObject(Vector2 pos, MapInfoSyncableBase objectToSync)
	{
		mMapDataObjectToSync.Add(pos, objectToSync);
	}

	public void InitMapDataObjects()
	{
	}

	public void SyncMapData(MapInfoSyncableBase objectToSync)
	{
		byte[] data = objectToSync.GetData();
		Vector2 startPos = objectToSync.GetStartPos();
		byte[] array = new byte[8 + data.Length];
		using (MemoryStream output = new MemoryStream(array))
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(output))
			{
				binaryWriter.Write(startPos.x);
				binaryWriter.Write(startPos.y);
				binaryWriter.Write(data);
			}
		}
		SendMessageToAllClients(array, P2PPackageHandler.MsgType.MapInfoSync, true, 0uL);
	}

	public void OnMapDataRecieved(byte[] data)
	{
		Vector2 objectPos = default(Vector2);
		byte[] data2;
		using (MemoryStream input = new MemoryStream(data))
		{
			using (BinaryReader binaryReader = new BinaryReader(input))
			{
				objectPos.x = binaryReader.ReadSingle();
				objectPos.y = binaryReader.ReadSingle();
				data2 = binaryReader.ReadBytes(data.Length - 8);
			}
		}
		MapInfoSyncableBase objectToSync = GetObjectToSync(objectPos);
		if ((bool)objectToSync)
		{
			objectToSync.SetData(data2);
		}
	}

	private MapInfoSyncableBase GetObjectToSync(Vector2 objectPos)
	{
		if (!mMapDataObjectToSync.ContainsKey(objectPos))
		{
			Debug.LogError("Could not find MapObject with pos: " + objectPos);
			return null;
		}
		MapInfoSyncableBase mapInfoSyncableBase = mMapDataObjectToSync[objectPos];
		if (mapInfoSyncableBase != null)
		{
			return mapInfoSyncableBase;
		}
		Debug.LogError("Could not find MapObject with pos: " + objectPos);
		return null;
	}

	public void OnMapInfoRecieved(byte[] data)
	{
		MapInfoOnlineTag mapInfoOnlineTag = UnityEngine.Object.FindObjectOfType<MapInfoOnlineTag>();
		if ((bool)mapInfoOnlineTag)
		{
			mapInfoOnlineTag.SendMessage("RecieveMapInfo", data);
		}
	}

	public void SendMapInfo(byte[] data)
	{
		SendMessageToAllClients(data, P2PPackageHandler.MsgType.MapInfo, false, 0uL);
	}

	public void OnPlayerTookDamage(byte[] data, int channel, ushort index)
	{
		ulong steamID = mConnectedClients[index].ClientID.m_SteamID;
		SendMessageToAllClients(data, P2PPackageHandler.MsgType.PlayerTookDamage, false, 0uL, EP2PSend.k_EP2PSendReliable, channel);
	}

	public void OnPlayerAddedForce(byte[] data, int channel)
	{
		ulong steamID = SteamUser.GetSteamID().m_SteamID;
		SendMessageToAllClients(data, P2PPackageHandler.MsgType.PlayerForceAdded, false, steamID, EP2PSend.k_EP2PSendReliable, channel);
	}

	public void OnPlayerAddedLavaForce(byte[] data, int channel)
	{
		ulong steamID = SteamUser.GetSteamID().m_SteamID;
		SendMessageToAllClients(data, P2PPackageHandler.MsgType.PlayerLavaForceAdded, false, steamID, EP2PSend.k_EP2PSendReliable, channel);
	}

	public void OnWeaponSpawned(byte[] data)
	{
		Vector3 zero = Vector3.zero;
		ushort num = 0;
		ushort num2 = 0;
		byte index;
		byte b;
		using (MemoryStream input = new MemoryStream(data))
		{
			using (BinaryReader binaryReader = new BinaryReader(input))
			{
				index = binaryReader.ReadByte();
				zero.y = binaryReader.ReadSByte();
				zero.z = binaryReader.ReadSByte();
				num = binaryReader.ReadUInt16();
				num2 = binaryReader.ReadUInt16();
				b = binaryReader.ReadByte();
			}
		}
		GameObject weaponByIndex = mWeaponSelectionHandler.GetWeaponByIndex(index);
		GameObject gameObject = UnityEngine.Object.Instantiate(weaponByIndex, zero, Quaternion.identity);
		WeaponPickUp component = gameObject.GetComponent<WeaponPickUp>();
		component.InitNetwork(num);
		if (mSpawnedWeapons.ContainsKey(num))
		{
			WeaponPickUp value;
			if (mSpawnedWeapons.TryGetValue(num, out value) && value != null && value != mDefaultWeaponPickUp)
			{
				UnityEngine.Object.Destroy(value.gameObject);
			}
			mSpawnedWeapons.Remove(num);
		}
		mSpawnedWeapons.Add(num, gameObject.GetComponent<WeaponPickUp>());
		NetworkSyncableObject networkSyncableObject = gameObject.FetchComponent<NetworkSyncableObject>();
		networkSyncableObject.InitNetworkIndex(num2, false);
		AddSyncableObject(num2, networkSyncableObject);
		networkSyncableObject.mLerpFriction = 0.8f;
		networkSyncableObject.mDirectionFractor = 0.04f;
		PingAllUsers();
		if (b == 1)
		{
			component.ChangeToPresent();
		}
	}

	public void OnWeaponSpawned(WeaponPickUp weaponThatWasSpawned, ushort weaponSpawnID, ushort syncIndex, bool initSyncableObject = true)
	{
		weaponThatWasSpawned.InitNetwork(weaponSpawnID);
		if (mSpawnedWeapons.ContainsKey(weaponSpawnID))
		{
			WeaponPickUp value;
			if (mSpawnedWeapons.TryGetValue(weaponSpawnID, out value) && value != null && value != mDefaultWeaponPickUp)
			{
				Debug.LogError("Spawned Weapon With Id: " + weaponSpawnID + " Is Already Here? Removing");
				UnityEngine.Object.Destroy(value.gameObject);
			}
			mSpawnedWeapons.Remove(weaponSpawnID);
		}
		mSpawnedWeapons.Add(weaponSpawnID, weaponThatWasSpawned);
		if (initSyncableObject)
		{
			NetworkSyncableObject networkSyncableObject = weaponThatWasSpawned.gameObject.FetchComponent<NetworkSyncableObject>();
			networkSyncableObject.InitNetworkIndex(syncIndex, false, 0.3f);
			AddSyncableObject(syncIndex, networkSyncableObject);
			networkSyncableObject.mLerpFriction = 0.8f;
			networkSyncableObject.mDirectionFractor = 0.04f;
		}
	}

	public void OnWeaponWasPickedUp(byte[] data)
	{
		ushort num;
		ushort num2;
		using (MemoryStream input = new MemoryStream(data))
		{
			using (BinaryReader binaryReader = new BinaryReader(input))
			{
				num = binaryReader.ReadByte();
				num2 = binaryReader.ReadUInt16();
			}
		}
		WeaponPickUp value;
		if (!mSpawnedWeapons.TryGetValue(num2, out value))
		{
			Debug.LogError("Failed To Get Weapon With Index: " + num2 + " This Should NOT Happen!");
			return;
		}
		ConnectedClientData connectedClientData = mConnectedClients[num];
		Fighting component = connectedClientData.PlayerObject.GetComponent<Fighting>();
		if (connectedClientData.ControlledLocally)
		{
			Debug.Log("I picked Up Something!!");
			component.PickUpWeapon(value.id, value.gameObject);
		}
		if (value.sendTheMovementAbility != -1)
		{
			component.SetMovementAbility(value.sendTheMovementAbility);
		}
		m_Players[num].GetComponent<CharacterStats>().weaponsPickedUp++;
		if (value != mDefaultWeaponPickUp)
		{
			if (!value.unEnding)
			{
				UnityEngine.Object.Destroy(value.gameObject);
			}
			Debug.Log("Destroying weapon with index: " + num2 + " : " + value.name);
		}
	}

	private void AddClientToList(CSteamID newClient, bool SendCallBackToClient = true, bool guest = false)
	{
		for (byte b = 0; b < mConnectedClients.Length; b++)
		{
			if (mConnectedClients[b] != null && mConnectedClients[b].ClientID == newClient && !guest)
			{
				Debug.LogError("Client: " + newClient.ToString() + " Is Already In The Server!");
				return;
			}
		}
		for (byte b2 = 0; b2 < mConnectedClients.Length; b2++)
		{
			if (mConnectedClients[b2] == null || mConnectedClients[b2].ClientID == mUnassignedID)
			{
				byte[] array = new byte[9];
				using (MemoryStream output = new MemoryStream(array))
				{
					using (BinaryWriter binaryWriter = new BinaryWriter(output))
					{
						binaryWriter.Write(b2);
						binaryWriter.Write(newClient.m_SteamID);
					}
				}
				SendMessageToAllClients(array, P2PPackageHandler.MsgType.ClientJoined, true, 0uL);
				ConnectedClientData connectedClientData = new ConnectedClientData();
				connectedClientData.Stats = new ClientStats();
				connectedClientData.ClientID = newClient;
				connectedClientData.Ready = false;
				mConnectedClients[b2] = connectedClientData;
				if (SendCallBackToClient)
				{
					byte[] prefixData = new byte[2] { 1, b2 };
					byte[] statusData = GetStatusData(prefixData);
					mPacketHandler.SendP2PPacketToUser(newClient, statusData, P2PPackageHandler.MsgType.ClientInit);
				}
				Debug.Log("Added New client to list: " + mConnectedClients[b2].PlayerName + " Send Callback: " + SendCallBackToClient);
				break;
			}
		}
	}

	public void OnInitFromServer(byte[] data)
	{
		mGameUI.Init();
		StartCoroutine(InitDataFromServerRecieved(data));
	}

	private IEnumerator InitDataFromServerRecieved(byte[] data)
	{
		if (data[0] != 1)
		{
			Debug.LogError("Client Init From Server Refused!");
			mMatchmakingHandler.TryReconnect();
			yield break;
		}
		OnlineBox.Joined();
		mPacketHandler.PauseNetworkTraffic();
		if ((bool)mOnlineBox)
		{
			mGameManager.DisableAllPlayers();
			mOnlineBox.PlayBoxAnimation();
			yield return new WaitForSeconds(3f);
			UnityEngine.Object.FindObjectOfType<LoadingScreenManager>().LoadForSeconds(3f);
			yield return new WaitForSeconds(0.5f);
		}
		mGameManager.KillAllPlayers(true);
		byte mapType = 0;
		bool needsToDownloadMaps = false;
		bool isInLobby = false;
		MapWrapper newMapWrapper = new MapWrapper();
		Action spawnPlayerAction = () =>
		{
			RequestSpawnPlayer(isInLobby);
		};
		Action startMatchAction = () =>
		{
			mGameManager.StartMatch(mGameManager.GetCurrentMap(), false);
		};
		byte playerindex;
		using (MemoryStream input = new MemoryStream(data))
		{
			using (BinaryReader binaryReader = new BinaryReader(input))
			{
				binaryReader.ReadByte();
				playerindex = binaryReader.ReadByte();
				mapType = binaryReader.ReadByte();
				int count = binaryReader.ReadInt32();
				byte[] mapData = binaryReader.ReadBytes(count);
				newMapWrapper.MapType = mapType;
				newMapWrapper.MapData = mapData;
				mGameManager.lastMapNumber = newMapWrapper;
				isInLobby = mGameManager.IsInLobby();
				Debug.Log("Recieved PlayerIndex: " + playerindex + " From server");
				for (byte b = 0; b < mConnectedClients.Length; b++)
				{
					ulong ulSteamID = binaryReader.ReadUInt64();
					CSteamID cSteamID = new CSteamID(ulSteamID);
					bool flag = cSteamID != mUnassignedID;
					Debug.Log("PlayerIndex: " + b + " is Inside? " + flag);
					if (flag)
					{
						mConnectedClients[b] = new ConnectedClientData();
						mConnectedClients[b].ClientID = cSteamID;
						if (cSteamID != SteamUser.GetSteamID())
						{
							GameObject gameObject = SpawnPlayerDummy(b);
							mConnectedClients[b].PlayerObject = gameObject;
							CharacterStats component = gameObject.GetComponent<CharacterStats>();
							int wins = binaryReader.ReadInt32();
							int kills = binaryReader.ReadInt32();
							int deaths = binaryReader.ReadInt32();
							int suicides = binaryReader.ReadInt32();
							int falls = binaryReader.ReadInt32();
							int crownSteals = binaryReader.ReadInt32();
							int bulletsHit = binaryReader.ReadInt32();
							int bulletsMissed = binaryReader.ReadInt32();
							int bulletsShot = binaryReader.ReadInt32();
							int blocks = binaryReader.ReadInt32();
							int punchesLanded = binaryReader.ReadInt32();
							int weaponsPickedUp = binaryReader.ReadInt32();
							int weaponsThrown = binaryReader.ReadInt32();
							if (MatchmakingHandler.LobbyType != ELobbyType.k_ELobbyTypePublic)
							{
								component.wins = wins;
								component.kills = kills;
								component.deaths = deaths;
								component.suicides = suicides;
								component.falls = falls;
								component.crownSteals = crownSteals;
								component.bulletsHit = bulletsHit;
								component.bulletsMissed = bulletsMissed;
								component.bulletsShot = bulletsShot;
								component.blocks = blocks;
								component.punchesLanded = punchesLanded;
								component.weaponsPickedUp = weaponsPickedUp;
								component.weaponsThrown = weaponsThrown;
							}
						}
					}
				}
				ushort num = binaryReader.ReadUInt16();
				for (int num2 = 0; num2 < num; num2++)
				{
					ushort num3 = binaryReader.ReadUInt16();
					if (num3 != ushort.MaxValue)
					{
						ushort syncIndex = binaryReader.ReadUInt16();
						byte b2 = binaryReader.ReadByte();
						if (b2 != byte.MaxValue)
						{
							GameObject weaponWithIndexAndOverFlow = mGameManager.GetWeaponWithIndexAndOverFlow(b2);
							GameObject gameObject2 = UnityEngine.Object.Instantiate(weaponWithIndexAndOverFlow, Vector3.zero, Quaternion.identity);
							OnWeaponSpawned(gameObject2.GetComponent<WeaponPickUp>(), num3, syncIndex);
						}
					}
				}
				byte[] array = binaryReader.ReadBytes(4);
				if (MatchmakingHandler.LobbyType != ELobbyType.k_ELobbyTypePublic)
				{
					OptionsHolder.NetworkOptionsChanged(array);
				}
				OptionsHolder.NetworkMapChanged(array[0]);
				if (newMapWrapper.MapType == 2)
				{
					Action action = spawnPlayerAction;
					if (!isInLobby)
					{
						action = (Action)Delegate.Combine(action, startMatchAction);
					}
					ulong[] maps = new ulong[1] { BitConverter.ToUInt64(newMapWrapper.MapData, 0) };
					needsToDownloadMaps = WorkshopMapsLoader.Instance.NewMapCycleLoaded(maps, action);
					Debug.Log("Custom Maps Got From Host: Needs to download Any? " + needsToDownloadMaps);
				}
			}
		}
		mLocalPlayerIndex = playerindex;
		if (!needsToDownloadMaps)
		{
			spawnPlayerAction();
		}
		if (!isInLobby)
		{
			if (!needsToDownloadMaps)
			{
				startMatchAction();
			}
		}
		else
		{
			ConnectedClientData[] array2 = mConnectedClients;
			foreach (ConnectedClientData connectedClientData in array2)
			{
				if (connectedClientData != null && connectedClientData.ClientID.IsValid() && !(connectedClientData.PlayerObject == null))
				{
					connectedClientData.PlayerObject.GetComponent<NetworkPlayer>().SetActive(true);
				}
			}
			UnityEngine.Object.FindObjectOfType<OnlinePlayerUI>().TextStay();
			if (MatchmakingHandler.LobbyType != ELobbyType.k_ELobbyTypePublic)
			{
				HostLeverHandler hostLeverHandler = UnityEngine.Object.FindObjectOfType<HostLeverHandler>();
				if ((bool)hostLeverHandler)
				{
					hostLeverHandler.SetLeverActive();
				}
			}
		}
		mHasBeenInitializedFromServer = true;
		if (!MatchmakingHandler.HasSuccededJoining)
		{
			Analytics.CustomEvent(AnalyticsEvents.NEVER_GOT_TO_PLAY_ONLINE, new Dictionary<string, object> { { "Success", 1 } });
			MatchmakingHandler.HasSuccededJoining = true;
		}
		Analytics.CustomEvent(AnalyticsEvents.CONNECTION_SUCCESSFUL_EVENT);
		mPacketHandler.ResumeNetworkTraffic();
		OnlineRoom room = UnityEngine.Object.FindObjectOfType<OnlineRoom>();
		room.GoBack();
		if (!needsToDownloadMaps)
		{
			UnityEngine.Object.FindObjectOfType<LoadingScreenManager>().StopLoading();
		}
	}

	public void OnPlayerMoved(byte[] data, int channel, ushort indexIgnore = ushort.MaxValue)
	{
		ulong ignoreUserID = ((indexIgnore != ushort.MaxValue) ? mConnectedClients[indexIgnore].ClientID.m_SteamID : 0);
		SendMessageToAllClients(data, P2PPackageHandler.MsgType.PlayerUpdate, false, ignoreUserID, EP2PSend.k_EP2PSendUnreliableNoDelay, channel);
	}

	public void OnPlayerTalked(byte[] data, int channel, ushort id)
	{
		CSteamID clientID = mConnectedClients[id].ClientID;
		SendMessageToAllClients(data, P2PPackageHandler.MsgType.PlayerTalked, false, clientID.m_SteamID, EP2PSend.k_EP2PSendReliable, channel);
	}

	public void OnObjectMoved(byte[] data, int channel)
	{
		SendMessageToAllClients(data, P2PPackageHandler.MsgType.ObjectUpdate, true, 0uL, EP2PSend.k_EP2PSendUnreliableNoDelay, channel);
	}

	public void OnDestructibleDestroyed(byte[] data, int channel)
	{
		SendMessageToAllClients(data, P2PPackageHandler.MsgType.ObjectDestructionCollision, true, 0uL, EP2PSend.k_EP2PSendReliable, channel);
	}

	public void OnPlayerThrowWeapon(byte[] data, int channel)
	{
		ushort nextWeaponSpawnID = GetNextWeaponSpawnID();
		ushort nextSyncableObjectSpawnID = GetNextSyncableObjectSpawnID();
		byte[] array = new byte[data.Length + 4];
		using (MemoryStream output = new MemoryStream(array))
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(output))
			{
				binaryWriter.Write(data);
				binaryWriter.Write(nextWeaponSpawnID);
				binaryWriter.Write(nextSyncableObjectSpawnID);
			}
		}
		SendMessageToAllClients(array, P2PPackageHandler.MsgType.WeaponThrown, false, 0uL, EP2PSend.k_EP2PSendReliable, channel);
	}

	public void OnPlayerWonWithRicochet(byte[] data, int channel)
	{
		SendMessageToAllClients(data, P2PPackageHandler.MsgType.PlayerWonWithRicochet, false, 0uL, EP2PSend.k_EP2PSendReliable, channel);
	}

	public void OnPlayerDroppedWeapon(byte[] data)
	{
		ushort nextWeaponSpawnID = GetNextWeaponSpawnID();
		ushort nextSyncableObjectSpawnID = GetNextSyncableObjectSpawnID();
		byte[] array = new byte[data.Length + 4];
		using (MemoryStream output = new MemoryStream(array))
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(output))
			{
				if (data.Length > 0)
				{
					binaryWriter.Write(data);
				}
				binaryWriter.Write(nextWeaponSpawnID);
				binaryWriter.Write(nextSyncableObjectSpawnID);
			}
		}
		SendMessageToAllClients(array, P2PPackageHandler.MsgType.WeaponDropped, false, 0uL);
	}

	public void OnPlayerSpawned(byte[] data)
	{
		byte b;
		Vector3 vector = default(Vector3);
		Vector3 euler = default(Vector3);
		bool flag;
		using (MemoryStream input = new MemoryStream(data))
		{
			using (BinaryReader binaryReader = new BinaryReader(input))
			{
				b = binaryReader.ReadByte();
				vector.x = binaryReader.ReadSingle();
				vector.y = binaryReader.ReadSingle();
				vector.z = binaryReader.ReadSingle();
				euler.x = binaryReader.ReadSingle();
				euler.y = binaryReader.ReadSingle();
				euler.z = binaryReader.ReadSingle();
				flag = binaryReader.ReadBoolean();
				if (flag)
				{
					vector = new Vector3(0f, -100f, 0f);
				}
			}
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(m_PlayerPrefab, vector, Quaternion.Euler(euler));
		LineRenderer[] componentsInChildren = gameObject.GetComponentsInChildren<LineRenderer>();
		foreach (LineRenderer lineRenderer in componentsInChildren)
		{
			lineRenderer.sharedMaterial = m_Colors[b];
		}
		SpriteRenderer[] componentsInChildren2 = gameObject.GetComponentsInChildren<SpriteRenderer>();
		foreach (SpriteRenderer spriteRenderer in componentsInChildren2)
		{
			if (spriteRenderer.transform.tag != "DontChangeColor")
			{
				spriteRenderer.color = m_Colors[b].color;
			}
		}
		CharacterInformation component = gameObject.GetComponent<CharacterInformation>();
		component.myMaterial = m_Colors[b];
		Controller component2 = gameObject.GetComponent<Controller>();
		component2.playerID = b;
		component2.SetCollision(true);
		NetworkPlayer networkPlayer = gameObject.FetchComponent<NetworkPlayer>();
		networkPlayer.InitNetworkSpawnID(b);
		if (vector == Vector3.zero)
		{
			networkPlayer.SetActive(true);
		}
		ConnectedClientData connectedClientData = mConnectedClients[b];
		if (connectedClientData.ControlledLocally)
		{
			CharacterActions nextSavedDeviceForNetwork = mGameManager.GetNextSavedDeviceForNetwork();
			networkPlayer.GetComponent<Controller>().TakeLocalControl(nextSavedDeviceForNetwork);
			networkPlayer.GetComponent<NetworkPlayer>().TakeLocalControl();
		}
		Debug.Log("Spawning request by server: " + b + " Pos: " + vector);
		UpdateLocalClientsData(b, gameObject);
		m_Players[b] = component2;
		if (!flag)
		{
			mGameManager.RevivePlayer(component2);
		}
		else
		{
			gameObject.GetComponent<HealthHandler>().ForcedDie();
		}
	}

	private void UpdateLocalClientsData(ushort playerID, GameObject spawnedPlayer)
	{
		if (mConnectedClients[playerID].PlayerObject != null)
		{
			HealthHandler component = mConnectedClients[playerID].PlayerObject.GetComponent<HealthHandler>();
			if ((bool)component)
			{
				component.ForcedDie();
			}
		}
		mConnectedClients[playerID].PlayerObject = spawnedPlayer;
		Debug.Log("Updating Player: " + playerID + " With playerobject: ", spawnedPlayer);
	}

	public void OnPlayerRequestingToSpawn(byte[] data)
	{
		byte[] array = new byte[data.Length + 1];
		bool value = !mGameManager.IsInLobby() && GetPlayersInLobby(true) > 1;
		using (MemoryStream output = new MemoryStream(array))
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(output))
			{
				binaryWriter.Write(data);
				binaryWriter.Write(value);
			}
		}
		SendMessageToAllClients(array, P2PPackageHandler.MsgType.ClientSpawned, false, 0uL);
	}

	public void RequestSpawnPlayer(bool isInLobby)
	{
		if (IsServer)
		{
			SpawnPlayer();
			Debug.Log("Server Called Client Function 'Request Player Spawn', Intentional'? Spawning Player Immediately instead...");
			return;
		}
		if (mLocalPlayerIndex < 0)
		{
			throw new Exception("Attempting to request when no index has been set, wait for server respone!");
		}
		Vector3 vector = ((!isInLobby) ? new Vector3(0f, 12f, 0f) : new Vector3(0f, 0f, 0f));
		Vector3 eulerAngles = Quaternion.identity.eulerAngles;
		for (byte b = 0; b < mConnectedClients.Length; b++)
		{
			if (mConnectedClients[b] != null && mConnectedClients[b].ControlledLocally)
			{
				byte[] array = new byte[25];
				using (MemoryStream output = new MemoryStream(array))
				{
					using (BinaryWriter binaryWriter = new BinaryWriter(output))
					{
						binaryWriter.Write(b);
						binaryWriter.Write(vector.x);
						binaryWriter.Write(vector.y);
						binaryWriter.Write(vector.z);
						binaryWriter.Write(eulerAngles.x);
						binaryWriter.Write(eulerAngles.y);
						binaryWriter.Write(eulerAngles.z);
					}
				}
				Debug.Log(string.Concat("Sending Request to spawn to the server at pos: ", vector, " Rotation: ", eulerAngles));
				mPacketHandler.SendP2PPacketToUser(mServerID, array, P2PPackageHandler.MsgType.ClientRequestingToSpawn);
			}
		}
	}

	private GameObject SpawnPlayerDummy(byte i)
	{
		Debug.Log("Trying To Spawn Dummy With Index: " + i);
		Vector3 position = new Vector3(0f, -10f, 0f);
		Quaternion rotation = Quaternion.Euler(Vector3.zero);
		GameObject gameObject = UnityEngine.Object.Instantiate(m_PlayerPrefab, position, rotation);
		LineRenderer[] componentsInChildren = gameObject.GetComponentsInChildren<LineRenderer>();
		foreach (LineRenderer lineRenderer in componentsInChildren)
		{
			lineRenderer.sharedMaterial = m_Colors[i];
		}
		SpriteRenderer[] componentsInChildren2 = gameObject.GetComponentsInChildren<SpriteRenderer>();
		foreach (SpriteRenderer spriteRenderer in componentsInChildren2)
		{
			if (spriteRenderer.transform.tag != "DontChangeColor")
			{
				spriteRenderer.color = m_Colors[i].color;
			}
		}
		CharacterInformation component = gameObject.GetComponent<CharacterInformation>();
		component.myMaterial = m_Colors[i];
		Controller component2 = gameObject.GetComponent<Controller>();
		component2.playerID = i;
		component2.SetCollision(true);
		NetworkPlayer networkPlayer = gameObject.FetchComponent<NetworkPlayer>();
		networkPlayer.InitNetworkSpawnID(i);
		UpdateLocalClientsData(i, gameObject);
		m_Players[i] = component2;
		mGameManager.RevivePlayer(component2);
		return gameObject;
	}

	public void OnPlayerRequestingIndex(byte[] data)
	{
		ulong num;
		byte value;
		using (MemoryStream input = new MemoryStream(data))
		{
			using (BinaryReader binaryReader = new BinaryReader(input))
			{
				num = binaryReader.ReadUInt64();
				value = binaryReader.ReadByte();
			}
		}
		value = (byte)Mathf.Clamp(value, 1, 4);
		Debug.Log("Recieving index request by User: " + num + " For: " + value + " Players!");
		CSteamID cSteamID = new CSteamID(num);
		int connectedPlayers = GetConnectedPlayers();
		if (connectedPlayers + value > mConnectedClients.Length)
		{
			Debug.LogError("Client Is Trying To Join With Too Many Players! Refusing Client Init");
			byte[] data2 = new byte[1];
			mPacketHandler.SendP2PPacketToUser(cSteamID, data2, P2PPackageHandler.MsgType.ClientInit);
			return;
		}
		Debug.Log("Added First Player!");
		if (value > 1)
		{
			AddClientToList(cSteamID, false);
			for (byte b = 1; b < value; b++)
			{
				if (b == value - 1)
				{
					Debug.Log("Added last player with callback!");
					AddClientToList(cSteamID, true, true);
				}
				else
				{
					Debug.Log("Added Player: " + (b + 1) + " No Callback");
					AddClientToList(cSteamID, false, true);
				}
			}
		}
		else
		{
			AddClientToList(cSteamID);
		}
	}

	private int GetConnectedPlayers()
	{
		int num = 0;
		ConnectedClientData[] array = mConnectedClients;
		foreach (ConnectedClientData connectedClientData in array)
		{
			if (connectedClientData != null && connectedClientData.ClientID != mUnassignedID)
			{
				num++;
			}
		}
		return num;
	}

	public void OnClientAcceptedByServer()
	{
		if (!mHasBeenAcceptedFromServer)
		{
			mHasBeenAcceptedFromServer = true;
			Debug.Log("Client Accepted By Server: Initing");
			RequestPlayerIndex();
			StartCoroutine(TimeOutAfterSecondsIfNotInitialized(5f));
		}
	}

	private void RequestPlayerIndex()
	{
		int num = 0;
		num = ((mGameManager.SavedDevicesForNetwork != null && mGameManager.SavedDevicesForNetwork.Count > 0) ? mGameManager.SavedDevicesForNetwork.Count : mGameManager.GetPlayersAlive());
		Debug.Log("Requesting player index for " + num + " Players!");
		byte[] array = new byte[9];
		using (MemoryStream output = new MemoryStream(array))
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(output))
			{
				binaryWriter.Write(SteamUser.GetSteamID().m_SteamID);
				binaryWriter.Write((byte)num);
			}
		}
		mPacketHandler.SendP2PPacketToUser(mServerID, array, P2PPackageHandler.MsgType.ClientRequestingIndex);
	}

	public void OnClientJoined(byte[] data)
	{
		if (IsServer)
		{
			Debug.LogError("Server recieved a client function, intended?");
		}
		byte b;
		CSteamID cSteamID;
		using (MemoryStream input = new MemoryStream(data))
		{
			using (BinaryReader binaryReader = new BinaryReader(input))
			{
				b = binaryReader.ReadByte();
				cSteamID = new CSteamID(binaryReader.ReadUInt64());
			}
		}
		ConnectedClientData connectedClientData = new ConnectedClientData();
		connectedClientData.Stats = new ClientStats();
		connectedClientData.ClientID = cSteamID;
		connectedClientData.Ready = false;
		mConnectedClients[b] = connectedClientData;
		P2PSessionState_t pConnectionState;
		SteamNetworking.GetP2PSessionState(cSteamID, out pConnectionState);
		string playerName = connectedClientData.PlayerName;
		Debug.Log("Connection State For: " + playerName + " Connected: " + pConnectionState.m_bConnectionActive + " Relay: " + pConnectionState.m_bUsingRelay);
		PingAllUsers();
	}

	private byte[] GetStatusData(byte[] prefixData = null)
	{
		CheckForNullWeapons();
		int num = ((prefixData != null) ? prefixData.Length : 0);
		ushort num2 = (ushort)mSpawnedWeapons.Count;
		LevelSelection levelSelection = UnityEngine.Object.FindObjectOfType<LevelSelection>();
		PlayableWorkshopLevel[] allCustomMapsActive = levelSelection.GetAllCustomMapsActive(false);
		ushort num3 = (ushort)allCustomMapsActive.Length;
		byte[] optionsData = OptionsHolder.GetOptionsData();
		byte[] array = new byte[num + 1 + 4 + mGameManager.lastMapNumber.MapData.Length + 60 * mConnectedClients.Length + 2 + 5 * num2 + optionsData.Length];
		using (MemoryStream output = new MemoryStream(array))
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(output))
			{
				if (num > 0)
				{
					binaryWriter.Write(prefixData);
				}
				binaryWriter.Write(mGameManager.lastMapNumber.MapType);
				binaryWriter.Write(mGameManager.lastMapNumber.MapData.Length);
				binaryWriter.Write(mGameManager.lastMapNumber.MapData);
				for (byte b = 0; b < mConnectedClients.Length; b++)
				{
					CSteamID cSteamID = ((mConnectedClients[b] != null) ? mConnectedClients[b].ClientID : mUnassignedID);
					binaryWriter.Write(cSteamID.m_SteamID);
					if (cSteamID != mUnassignedID && !(mConnectedClients[b].PlayerObject == null))
					{
						CharacterStats component = mConnectedClients[b].PlayerObject.GetComponent<CharacterStats>();
						binaryWriter.Write(component.wins);
						binaryWriter.Write(component.kills);
						binaryWriter.Write(component.deaths);
						binaryWriter.Write(component.suicides);
						binaryWriter.Write(component.falls);
						binaryWriter.Write(component.crownSteals);
						binaryWriter.Write(component.bulletsHit);
						binaryWriter.Write(component.bulletsMissed);
						binaryWriter.Write(component.bulletsShot);
						binaryWriter.Write(component.blocks);
						binaryWriter.Write(component.punchesLanded);
						binaryWriter.Write(component.weaponsPickedUp);
						binaryWriter.Write(component.weaponsThrown);
					}
				}
				binaryWriter.Write(num2);
				foreach (KeyValuePair<ushort, WeaponPickUp> mSpawnedWeapon in mSpawnedWeapons)
				{
					WeaponPickUp value = mSpawnedWeapon.Value;
					if (value == mDefaultWeaponPickUp || value == null)
					{
						binaryWriter.Write(ushort.MaxValue);
						continue;
					}
					binaryWriter.Write(value.NetworkSpawnIndex);
					NetworkSyncableObject component2 = value.GetComponent<NetworkSyncableObject>();
					binaryWriter.Write(component2.Index);
					byte value2 = mGameManager.FindWeaponIdByName(value.name);
					binaryWriter.Write(value2);
				}
				binaryWriter.Write(optionsData);
				return array;
			}
		}
	}

	public void OptionsChanged(byte[] optionsData)
	{
		SendMessageToAllClients(optionsData, P2PPackageHandler.MsgType.OptionsChanged, true, 0uL);
	}

	public void OnDisconnected()
	{
		mHasBeenAcceptedFromServer = false;
		ConnectedClientData[] array = mConnectedClients;
		foreach (ConnectedClientData connectedClientData in array)
		{
			if (connectedClientData == null)
			{
				continue;
			}
			CSteamID clientID = connectedClientData.ClientID;
			if (clientID.IsValid())
			{
				if (SteamNetworking.CloseP2PSessionWithUser(clientID))
				{
					Debug.Log("Successfully Closed P2P Connection With Steam User: " + clientID);
				}
				else
				{
					Debug.Log("Failed To Close P2P Connection With Steam User: " + clientID);
				}
			}
			if (connectedClientData.PlayerObject != null)
			{
				UnityEngine.Object.Destroy(connectedClientData.PlayerObject);
			}
		}
		mConnectedClients = new ConnectedClientData[4];
		mGameManager.SaveDevicesForNextGame(m_Players);
	}

	private void SendMessageToAllClients(byte[] data, P2PPackageHandler.MsgType type, bool ignoreServer = false, ulong ignoreUserID = 0uL, EP2PSend sendMethod = EP2PSend.k_EP2PSendReliable, int channel = 0)
	{
		List<CSteamID> list = new List<CSteamID>();
		ushort num = 0;
		for (byte b = 0; b < mConnectedClients.Length; b++)
		{
			CSteamID cSteamID = ((mConnectedClients[b] != null) ? mConnectedClients[b].ClientID : mUnassignedID);
			if (cSteamID != mUnassignedID && (!ignoreServer || !(cSteamID == mServerID)) && (ignoreUserID == 0 || cSteamID.m_SteamID != ignoreUserID) && !list.Contains(cSteamID))
			{
				num++;
				mPacketHandler.SendP2PPacketToUser(cSteamID, data, type, sendMethod, channel);
				list.Add(cSteamID);
			}
		}
		if (num > 0 && type == P2PPackageHandler.MsgType.PlayerTookDamage)
		{
			Debug.Log("Sending Message to: " + num + " clients: " + type.ToString());
		}
	}

	public static void FlushChannel(int Channel)
	{
		uint pcubMsgSize;
		while (SteamNetworking.IsP2PPacketAvailable(out pcubMsgSize, Channel))
		{
			byte[] pubDest = new byte[pcubMsgSize];
			uint pcubMsgSize2;
			CSteamID psteamIDRemote;
			if (!SteamNetworking.ReadP2PPacket(pubDest, pcubMsgSize, out pcubMsgSize2, out psteamIDRemote, Channel))
			{
				Debug.Log("Failed to read P2P Package!");
			}
		}
		Debug.Log("Flush Sucesful for channel: " + Channel);
	}

	private bool RemoveClientFromList(CSteamID steamID)
	{
		Debug.Log("Trying To Remove Client: " + steamID);
		bool flag = false;
		byte b = byte.MaxValue;
		string empty = string.Empty;
		for (int num = mConnectedClients.Length - 1; num >= 0; num--)
		{
			ConnectedClientData connectedClientData = mConnectedClients[num];
			if (connectedClientData != null && !(connectedClientData.ClientID != steamID))
			{
				b = (byte)num;
				Debug.Log(string.Concat("Found Client: ", steamID, " Removing..."));
				empty = connectedClientData.PlayerName;
				connectedClientData.ClientID = mUnassignedID;
				Controller controller = m_Players[num];
				if (controller != null)
				{
					mGameManager.KillPlayer(controller);
				}
				m_Players[num] = null;
				if (connectedClientData.PlayerObject != null)
				{
					UnityEngine.Object.Destroy(connectedClientData.PlayerObject);
				}
				flag = true;
			}
		}
		WinCounterUI winCounterUI = UnityEngine.Object.FindObjectOfType<WinCounterUI>();
		if (winCounterUI != null)
		{
			winCounterUI.RefreshWinTexts();
		}
		Debug.Log((!flag) ? string.Concat(" REMOVING FAILED: Client: ", steamID, " Was Not In Client List?") : string.Concat("Succesfully Removed Client ", steamID, " Sending Callback To Everyone!"));
		return flag;
	}

	public void AssignBox(OnlineBox onlineBox)
	{
		mOnlineBox = onlineBox;
	}

	public string GetGUIInfo()
	{
		return string.Empty;
	}

	private void OnGUI()
	{
		if (!Application.isEditor)
		{
			return;
		}
		int num = 40;
		int num2 = 10;
		int num3 = 0;
		GUI.Label(new Rect(0f, num + num2 * ++num3, 100f, 20f), "Options:");
		GUI.Label(new Rect(0f, num + num2 * ++num3, 100f, 20f), "HP: " + OptionsHolder.HP);
		GUI.Label(new Rect(0f, num + num2 * ++num3, 100f, 20f), "Maps: " + OptionsHolder.maps);
		GUI.Label(new Rect(0f, num + num2 * ++num3, 100f, 20f), "Regen: " + OptionsHolder.regen);
		if (mSpawnedSyncableObjects == null)
		{
			return;
		}
		foreach (KeyValuePair<Vector2, MapInfoSyncableBase> item in mMapDataObjectToSync)
		{
			GUI.Label(new Rect(0f, num + num2 * ++num3, 100f, 20f), string.Concat(item.Key, " : ", (!(item.Value == null)) ? item.Value.name : "NULL"));
		}
		if (mSpawnedWeapons == null)
		{
			return;
		}
		int num4 = 0;
		foreach (KeyValuePair<ushort, WeaponPickUp> mSpawnedWeapon in mSpawnedWeapons)
		{
			GUI.Label(new Rect(Screen.width - 150, num + num2 * ++num4, 100f, 20f), mSpawnedWeapon.Key + " : " + ((!(mSpawnedWeapon.Value == null)) ? mSpawnedWeapon.Value.name : "NULL"));
		}
	}

	public void OnMatchStart(byte[] data)
	{
		mGameManager.StartCountDown();
	}

	public void CheckReadyPlayers()
	{
		bool flag = false;
		ConnectedClientData[] array = mConnectedClients;
		foreach (ConnectedClientData connectedClientData in array)
		{
			if (connectedClientData != null && !(connectedClientData.ClientID == mUnassignedID) && !connectedClientData.Ready)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			SendMessageToAllClients(new byte[0], P2PPackageHandler.MsgType.StartMatch, false, 0uL);
		}
	}

	public void OnClientReadyUp(byte[] data)
	{
		byte b = data[0];
		for (int i = 0; i < b; i++)
		{
			byte b2 = data[i + 1];
			mConnectedClients[b2].Ready = true;
		}
		if (GameManager.inFight)
		{
			mPacketHandler.SendP2PPacketToUser(mConnectedClients[data[1]].ClientID, new byte[0], P2PPackageHandler.MsgType.StartMatch);
		}
		else
		{
			CheckReadyPlayers();
		}
	}

	public void OnKicked(byte[] data)
	{
		if (!IsServer)
		{
			KickResponse kickResponse = (KickResponse)data[0];
			Debug.Log("Kicked From Lobby Due To: " + kickResponse);
			mMatchmakingHandler.Disconnect();
		}
	}

	public void OnServerJoined(LobbyEnter_t param, bool bIOFailure)
	{
		if (bIOFailure)
		{
			Debug.Log("Biofailure!");
			return;
		}
		Debug.Log("Joined Server!");
		CSteamID cSteamID = new CSteamID(param.m_ulSteamIDLobby);
		string lobbyData = SteamMatchmaking.GetLobbyData(cSteamID, StickFightConstants.VERSION_KEY);
		if (lobbyData != StickFightConstants.VERSION_VALUE)
		{
			UnityEngine.Object.FindObjectOfType<LoadingScreenManager>().LoadThenFail(ConnectionErrorType.InvalidVersion, "Host: " + lobbyData + " Your: " + StickFightConstants.VERSION_VALUE);
			return;
		}
		mMatchmakingHandler.ClientInitLobbyAndOwner(cSteamID);
		ELobbyType newLobbyType = (ELobbyType)Enum.Parse(typeof(ELobbyType), SteamMatchmaking.GetLobbyData(cSteamID, StickFightConstants.LOBBY_TYPE_KEY));
		MatchmakingHandler.SetNewLobbyType(newLobbyType);
		int numLobbyMembers = SteamMatchmaking.GetNumLobbyMembers(cSteamID);
		for (int i = 0; i < numLobbyMembers; i++)
		{
			CSteamID lobbyMemberByIndex = SteamMatchmaking.GetLobbyMemberByIndex(cSteamID, i);
			mPacketHandler.SendP2PPacketToUser(lobbyMemberByIndex, new byte[0], P2PPackageHandler.MsgType.Ping);
		}
		OnSceneStarted();
	}

	public void UpdatePingForUser(ulong user, float pingInMs)
	{
		int num = mConnectedClients.Length;
		for (int i = 0; i < num; i++)
		{
			ConnectedClientData connectedClientData = mConnectedClients[i];
			if (connectedClientData != null && connectedClientData.ClientID.IsValid() && user == connectedClientData.ClientID.m_SteamID)
			{
				connectedClientData.UpdatePing(pingInMs);
			}
		}
	}

	private void PingAllUsers()
	{
		ConnectedClientData[] array = mConnectedClients;
		foreach (ConnectedClientData connectedClientData in array)
		{
			if (connectedClientData != null && connectedClientData.ClientID.IsValid() && !(connectedClientData.ClientID == SteamUser.GetSteamID()))
			{
				PingUser(connectedClientData.ClientID);
			}
		}
	}

	private void PingUser(CSteamID user)
	{
		int value = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
		byte[] array = new byte[4];
		using (MemoryStream output = new MemoryStream(array))
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(output))
			{
				binaryWriter.Write(value);
			}
		}
		mPacketHandler.SendP2PPacketToUser(user, array, P2PPackageHandler.MsgType.Ping);
	}

	public void OnSocketServerJoined()
	{
		Debug.Log("Joined Server!");
		ELobbyType newLobbyType = ELobbyType.k_ELobbyTypePublic;
		MatchmakingHandler.SetNewLobbyType(newLobbyType);
		OnSceneStarted();
	}

	public void OnSocketServerCreated()
	{
		mHasBeenInitializedFromServer = true;
		OnSceneStarted();
	}

	public void OnServerCreated(LobbyCreated_t param, bool bIOFailure)
	{
		if (bIOFailure)
		{
			Debug.Log("Biofailure!");
			return;
		}
		if (param.m_eResult == EResult.k_EResultOK)
		{
			Debug.Log("Server Created! " + param.m_ulSteamIDLobby);
			CSteamID cSteamID = new CSteamID(param.m_ulSteamIDLobby);
			SteamMatchmaking.SetLobbyData(cSteamID, StickFightConstants.LOBBY_TYPE_KEY, MatchmakingHandler.LobbyType.ToString());
			SteamMatchmaking.SetLobbyData(cSteamID, StickFightConstants.VERSION_KEY, StickFightConstants.VERSION_VALUE);
			string lobbyData = SteamMatchmaking.GetLobbyData(cSteamID, StickFightConstants.VERSION_KEY);
			SteamMatchmaking.SetLobbyJoinable(cSteamID, false);
			mMatchmakingHandler.ClientInitLobbyAndOwner(cSteamID);
			mHasBeenInitializedFromServer = true;
			OnSceneStarted();
			return;
		}
		Debug.Log("Error Creating server..." + param.m_eResult);
		ConnectionErrorType type;
		switch (param.m_eResult)
		{
		case EResult.k_EResultNoConnection:
			type = ConnectionErrorType.NoConnection;
			break;
		case EResult.k_EResultTimeout:
			type = ConnectionErrorType.TimeOut;
			break;
		case EResult.k_EResultFail:
			type = ConnectionErrorType.Unknown;
			break;
		default:
			type = ConnectionErrorType.Unknown;
			break;
		}
		UnityEngine.Object.FindObjectOfType<LoadingScreenManager>().LoadThenFail(type, string.Empty);
	}

	private static bool HasFlag(SpawnableObjectType a, SpawnableObjectType b)
	{
		return (a & b) == b;
	}
}
