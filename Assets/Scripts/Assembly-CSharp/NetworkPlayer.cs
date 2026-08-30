using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Steamworks;
using UnityEngine;

public class NetworkPlayer : MonoBehaviour
{
	public struct NetworkPositionPackage
	{
		public ShortVector2 Position;

		public ByteVector2 Rotation;

		public sbyte YValue;

		public byte MovementType;

		public int Size
		{
			get
			{
				return 8;
			}
		}
	}

	public struct NetworkWeaponPackage
	{
		public byte WeaponType;

		public byte FightState;

		public ProjectilePackageStruct[] ProjectilePackages;

		public int Size
		{
			get
			{
				return 2 + (6 + 2 * ProjectilePackages.Length);
			}
		}
	}

	private const float DISTANCE_THRESHOLD_SYNCPOS = 0.5f;

	private const ushort MAX_NETWORK_SPAWNID = ushort.MaxValue;

	private static bool mIsNetworkMatch;

	private P2PPackageHandler mPacketHandler;

	private NetworkPositionPackage mNetworkPositionPackage = default(NetworkPositionPackage);

	private NetworkWeaponPackage mNetworkWeaponPackage = default(NetworkWeaponPackage);

	private MultiplayerManager mNetworkManager;

	private float mSyncThreshold = 10f;

	private bool mIsLerping;

	private float mTimeStartedLerping;

	private float mTimeTakenDuringLerp;

	private Vector3 mStartPos;

	private Vector3 mEndPos;

	private float mSendRatePerSecond = 50f;

	private float mSendRate;

	private float mCurrentSendTickCount;

	private int m_updateIndex;

	private int mDontSyncForFrames;

	private bool mIsActive;

	private float mCurrentSendDeathTickCount;

	private float mSendDeathRate = 2f;

	private Rigidbody[] allMyRigs;

	private ushort mNetworkSpawnID = ushort.MaxValue;

	private int mUpdateChannel = -1;

	private int mEventChannel = -1;

	private bool mHasLocalControl;

	private Movement mMovement;

	private Fighting mFighting;

	private Controller mController;

	private HealthHandler mHealthHandler;

	private Standing mStanding;

	private PunchForce mPunchForce;

	private ChatManager mChatManager;

	private static ChatManager mLocalChatManager;

	private Transform mHip;

	private Transform mTorso;

	private CharacterInformation mCharacterInformation;

	private Rigidbody mHipRigidBody;

	private Rigidbody[] mAllRigidBodies;

	private ParticleSystem mDamageParticleSystem;

	private ParticleSystem mFallOutParticleSystem;

	[SerializeField]
	private Transform mHelpSphere;

	[SerializeField]
	private Transform mHelpPredictionSphere;

	private Vector3 mMoveForce;

	[SerializeField]
	private float mDirectionFractor = 0.1f;

	[SerializeField]
	private float mLerpFriction = 0.5f;

	[SerializeField]
	private float mLerpDistanceCap = 0.5f;

	private Vector3 lastPackagePosition;

	private bool mHasRecievedFirstPackage;

	private static bool IsServer
	{
		get
		{
			return MultiplayerManager.IsServer;
		}
	}

	public ushort NetworkSpawnID
	{
		get
		{
			if (mNetworkSpawnID == ushort.MaxValue)
			{
				throw new Exception("Network spawnid is default value! Not inited!");
			}
			return mNetworkSpawnID;
		}
	}

	public bool HasLocalControl
	{
		get
		{
			return mHasLocalControl;
		}
	}

	public ParticleSystem FallOutParticleSystem
	{
		get
		{
			return mFallOutParticleSystem;
		}
	}

	public void InitNetworkSpawnID(ushort networkSpawnID)
	{
		mNetworkSpawnID = networkSpawnID;
		mUpdateChannel = mNetworkSpawnID * 2 + 2;
		mEventChannel = mUpdateChannel + 1;
	}

	private void Awake()
	{
		allMyRigs = GetComponentsInChildren<Rigidbody>();
		mSendRate = 1f / mSendRatePerSecond;
		mTimeTakenDuringLerp = mSendRate;
		mNetworkManager = UnityEngine.Object.FindObjectOfType<MultiplayerManager>();
		mMovement = GetComponent<Movement>();
		mFighting = GetComponent<Fighting>();
		mController = GetComponent<Controller>();
		mHip = GetComponentInChildren<Hip>().transform;
		mTorso = GetComponentInChildren<Torso>().transform;
		mHipRigidBody = mHip.GetComponent<Rigidbody>();
		mHealthHandler = GetComponent<HealthHandler>();
		mStanding = GetComponent<Standing>();
		mChatManager = GetComponentInChildren<ChatManager>();
		mCharacterInformation = GetComponent<CharacterInformation>();
		mDamageParticleSystem = GetComponentInChildren<DamageParticle>().GetComponent<ParticleSystem>();
		mFallOutParticleSystem = GetComponentInChildren<FallOutPart>().GetComponent<ParticleSystem>();
		mPunchForce = GetComponentInChildren<PunchForce>();
	}

	private void InitRigidBodies()
	{
		mAllRigidBodies = GetComponentsInChildren<Rigidbody>();
		int num = mAllRigidBodies.Length;
		for (byte b = 0; b < num; b++)
		{
			Rigidbody rigidbody = mAllRigidBodies[b];
			rigidbody.gameObject.AddComponent<RigidBodyIndexHolder>().InitIndex(b);
			if (!mHasLocalControl)
			{
				rigidbody.isKinematic = true;
			}
		}
	}

	private void Start()
	{
		mPacketHandler = P2PPackageHandler.Instance;
		MatchmakingHandler instance = MatchmakingHandler.Instance;
		mIsNetworkMatch = !(instance == null) && instance.IsInsideLobby;
		if (!mIsNetworkMatch)
		{
			UnityEngine.Object.Destroy(this);
			return;
		}
		InitRigidBodies();
		lastPackagePosition = base.transform.position;
	}

	private void Update()
	{
		if (mCharacterInformation.isDead || !mIsActive)
		{
			if (mHasLocalControl)
			{
				TickSyncDeath();
			}
			else
			{
				ListenForDeadPackages(mEventChannel);
			}
			ListenForChatMessages(mEventChannel);
		}
		else
		{
			if (mHasLocalControl)
			{
				TickSyncPos();
			}
			else
			{
				ListenForPositionPackages(mUpdateChannel);
			}
			ListenForEventPackages(mEventChannel);
		}
	}

	public void FlushChannels()
	{
		MultiplayerManager.FlushChannel(mUpdateChannel);
		MultiplayerManager.FlushChannel(mEventChannel);
	}

	private void FixedUpdate()
	{
		if (mIsLerping)
		{
			LerpLocalDummy();
		}
	}

	private void LerpLocalDummy()
	{
		float num = Time.time - mTimeStartedLerping;
		float num2 = num / mTimeTakenDuringLerp;
		if (mDontSyncForFrames > 0)
		{
			mDontSyncForFrames--;
			Debug.Log("Waited To Sync Character: " + mDontSyncForFrames + " Frames Left");
		}
		else
		{
			base.transform.position = Vector3.Lerp(base.transform.position, mEndPos, Time.deltaTime * 10f);
		}
		if (num2 >= 1f)
		{
			mIsLerping = false;
		}
	}

	private void ResetCurrentSendTickTime()
	{
		mCurrentSendTickCount = 0f;
	}

	private void TickSyncPos()
	{
		if (mIsActive)
		{
			TickCurrentSendTime();
			if (mCurrentSendTickCount >= mSendRate)
			{
				SendNewClientStatePackage();
				ResetCurrentSendTickTime();
			}
		}
	}

	private void TickSyncDeath()
	{
		if (mIsActive)
		{
			TickCurrentSendDeathTime();
			if (mCurrentSendDeathTickCount >= mSendDeathRate)
			{
				UnitWasDamaged(10000f, true);
				ResetCurrentSendDeathTickTime();
			}
		}
	}

	private void ResetCurrentSendDeathTickTime()
	{
		mCurrentSendDeathTickCount = 0f;
	}

	private void TickCurrentSendTime()
	{
		mCurrentSendTickCount += Time.unscaledDeltaTime;
	}

	private void TickCurrentSendDeathTime()
	{
		mCurrentSendDeathTickCount += Time.unscaledDeltaTime;
	}

	private void SendNewClientStatePackage()
	{
		mNetworkPositionPackage = CreateNetworkPositionPackage();
		mNetworkWeaponPackage = CreateNetworkWeaponPackage();
		uint serverRealTime = SteamUtils.GetServerRealTime();
		byte[] array = new byte[mNetworkPositionPackage.Size + mNetworkWeaponPackage.Size + 2];
		using (MemoryStream output = new MemoryStream(array))
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(output))
			{
				binaryWriter.Write(mNetworkPositionPackage.Position.X);
				binaryWriter.Write(mNetworkPositionPackage.Position.Y);
				binaryWriter.Write(mNetworkPositionPackage.Rotation.X);
				binaryWriter.Write(mNetworkPositionPackage.Rotation.Y);
				binaryWriter.Write(mNetworkPositionPackage.YValue);
				binaryWriter.Write(mNetworkPositionPackage.MovementType);
				binaryWriter.Write(mNetworkWeaponPackage.FightState);
				ProjectilePackageStruct[] projectilePackages = mNetworkWeaponPackage.ProjectilePackages;
				ushort num = (ushort)projectilePackages.Length;
				binaryWriter.Write(num);
				if (num > 0)
				{
					for (int i = 0; i < num; i++)
					{
						ProjectilePackageStruct projectilePackageStruct = projectilePackages[i];
						binaryWriter.Write(projectilePackageStruct.shootPosition.X);
						binaryWriter.Write(projectilePackageStruct.shootPosition.Y);
						binaryWriter.Write(projectilePackageStruct.shootVector.X);
						binaryWriter.Write(projectilePackageStruct.shootVector.Y);
						binaryWriter.Write(projectilePackageStruct.syncIndex);
						Debug.Log("Sending: ProjectilePackage: " + projectilePackageStruct.shootPosition.ToString() + " : " + projectilePackageStruct.shootVector.ToString());
					}
				}
				binaryWriter.Write(mNetworkWeaponPackage.WeaponType);
			}
		}
		mNetworkManager.OnPlayerMoved(array, mUpdateChannel, mNetworkSpawnID);
	}

	private void ListenForDeadPackages(int channel)
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
				break;
			}
			using (MemoryStream input = new MemoryStream(array))
			{
				using (BinaryReader binaryReader = new BinaryReader(input))
				{
					uint num = binaryReader.ReadUInt32();
					P2PPackageHandler.MsgType msgType = (P2PPackageHandler.MsgType)binaryReader.ReadByte();
					byte[] data = binaryReader.ReadBytes((int)(pcubMsgSize - 1));
					switch (msgType)
					{
					case P2PPackageHandler.MsgType.PlayerFallOut:
						SyncClientFallOut(data);
						break;
					case P2PPackageHandler.MsgType.PlayerTalked:
						SyncClientChat(data);
						break;
					case P2PPackageHandler.MsgType.PlayerWonWithRicochet:
						SyncClientWonWithRicochet(data);
						break;
					case P2PPackageHandler.MsgType.PlayerTookDamage:
						break;
					default:
						Debug.LogError("Invalid Messagetype " + msgType, this);
						break;
					}
				}
			}
		}
	}

	private void ListenForChatMessages(int channel)
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
				break;
			}
			using (MemoryStream input = new MemoryStream(array))
			{
				using (BinaryReader binaryReader = new BinaryReader(input))
				{
					uint num = binaryReader.ReadUInt32();
					P2PPackageHandler.MsgType msgType = (P2PPackageHandler.MsgType)binaryReader.ReadByte();
					byte[] data = binaryReader.ReadBytes((int)(pcubMsgSize - 1));
					switch (msgType)
					{
					case P2PPackageHandler.MsgType.PlayerTalked:
						SyncClientChat(data);
						break;
					case P2PPackageHandler.MsgType.PlayerTookDamage:
						break;
					default:
						Debug.LogError("Invalid Messagetype " + msgType, this);
						break;
					}
				}
			}
		}
	}

	private void ListenForEventPackages(int channel)
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
			}
			else
			{
				if (!mIsActive)
				{
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
							Debug.LogError("Packet arrived with a delay of more than seconds, throwing away! Of TYPE: " + msgType);
							continue;
						}
						byte[] data = binaryReader.ReadBytes((int)(pcubMsgSize - 1));
						switch (msgType)
						{
						case P2PPackageHandler.MsgType.PlayerTookDamage:
							SyncClienthealth(data);
							break;
						case P2PPackageHandler.MsgType.PlayerForceAdded:
							SyncClientForceAdded(data);
							break;
						case P2PPackageHandler.MsgType.PlayerLavaForceAdded:
							SyncClientLavaForceAdded(data);
							break;
						case P2PPackageHandler.MsgType.PlayerForceAddedAndBlock:
							SyncClientBlockForceAdded(data);
							break;
						case P2PPackageHandler.MsgType.PlayerWonWithRicochet:
							SyncClientWonWithRicochet(data);
							break;
						case P2PPackageHandler.MsgType.WeaponThrown:
							SyncClientWeaponThrow(data);
							break;
						case P2PPackageHandler.MsgType.RequestingWeaponThrow:
							if (IsServer)
							{
								mNetworkManager.OnPlayerThrowWeapon(data, channel);
							}
							break;
						case P2PPackageHandler.MsgType.PlayerFallOut:
							SyncClientFallOut(data);
							break;
						case P2PPackageHandler.MsgType.PlayerTalked:
							SyncClientChat(data);
							break;
						default:
							Debug.LogError("Invalid Messagetype " + msgType, this);
							break;
						}
					}
				}
			}
		}
	}

	private void SyncClientChat(byte[] data)
	{
		if (OptionsHolder.chat != 1)
		{
			string text = Encoding.UTF8.GetString(data);
			Debug.Log("Recieving chat message: " + text);
			if (mHasLocalControl)
			{
				mLocalChatManager.Talk(text);
			}
			else
			{
				mChatManager.Talk(text);
			}
		}
	}

	private void SyncClientBlockForceAdded(byte[] data)
	{
		byte b;
		Vector3 force = default(Vector3);
		ForceMode mode;
		byte playerIndex;
		using (MemoryStream input = new MemoryStream(data))
		{
			using (BinaryReader binaryReader = new BinaryReader(input))
			{
				b = binaryReader.ReadByte();
				force.x = binaryReader.ReadSingle();
				force.y = binaryReader.ReadSingle();
				force.z = binaryReader.ReadSingle();
				mode = (ForceMode)binaryReader.ReadByte();
				playerIndex = binaryReader.ReadByte();
			}
		}
		mAllRigidBodies[b].AddForce(force, mode);
		mNetworkManager.DoBlockForPlayer(playerIndex);
	}

	private void SyncClientLavaForceAdded(byte[] data)
	{
		Vector3 zero = Vector3.zero;
		byte b;
		ForceMode mode;
		using (MemoryStream input = new MemoryStream(data))
		{
			using (BinaryReader binaryReader = new BinaryReader(input))
			{
				b = binaryReader.ReadByte();
				zero.y = (float)binaryReader.ReadSByte() / 100f;
				zero.z = (float)binaryReader.ReadSByte() / 100f;
				mode = (ForceMode)binaryReader.ReadByte();
			}
		}
		mAllRigidBodies[b].AddForce(zero, mode);
	}

	private void SyncClientForceAdded(byte[] data)
	{
		Vector3 zero = Vector3.zero;
		byte b;
		ForceMode mode;
		using (MemoryStream input = new MemoryStream(data))
		{
			using (BinaryReader binaryReader = new BinaryReader(input))
			{
				b = binaryReader.ReadByte();
				zero.y = (float)binaryReader.ReadSByte() * 100f;
				zero.z = (float)binaryReader.ReadSByte() * 100f;
				mode = (ForceMode)binaryReader.ReadByte();
			}
		}
		mAllRigidBodies[b].AddForce(zero, mode);
	}

	private void ListenForPositionPackages(int channel)
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
			}
			else
			{
				if (!mIsActive && mHasRecievedFirstPackage)
				{
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
							Debug.LogWarning("Packet Is obsolete, throwing away! Of TYPE: " + msgType);
							continue;
						}
						byte[] data = binaryReader.ReadBytes((int)(pcubMsgSize - 1));
						if (msgType == P2PPackageHandler.MsgType.PlayerUpdate)
						{
							SyncClientState(data);
						}
						else
						{
							Debug.LogError("Invalid Messagetype " + msgType);
						}
					}
				}
			}
		}
	}

	private void SyncClientFallOut(byte[] data)
	{
		Quaternion identity = Quaternion.identity;
		Vector3 zero = Vector3.zero;
		byte index;
		using (MemoryStream input = new MemoryStream(data))
		{
			using (BinaryReader binaryReader = new BinaryReader(input))
			{
				index = binaryReader.ReadByte();
				identity.x = binaryReader.ReadSingle();
				identity.y = binaryReader.ReadSingle();
				identity.z = binaryReader.ReadSingle();
				identity.w = binaryReader.ReadSingle();
				zero.x = binaryReader.ReadSingle();
				zero.y = binaryReader.ReadSingle();
				zero.z = binaryReader.ReadSingle();
			}
		}
		Controller controller = mNetworkManager.PlayerControllers[index];
		if (controller != null)
		{
			controller.OnFallOut();
		}
		ParticleSystem particleSystem = mFallOutParticleSystem;
		particleSystem.transform.rotation = identity;
		particleSystem.transform.position = zero;
		particleSystem.Play();
		GetComponent<CharacterStats>().falls++;
	}

	public void TakeLocalControl()
	{
		mHasLocalControl = true;
		mChatManager.enabled = true;
		if ((bool)mHelpSphere)
		{
			mHelpSphere.gameObject.SetActive(false);
		}
		mHelpPredictionSphere.gameObject.SetActive(false);
		if (mLocalChatManager != null)
		{
			mLocalChatManager.enabled = false;
		}
		mLocalChatManager = GetComponentInChildren<ChatManager>();
		mLocalChatManager.enabled = true;
		if (mAllRigidBodies != null)
		{
			Rigidbody[] array = mAllRigidBodies;
			foreach (Rigidbody rigidbody in array)
			{
				rigidbody.isKinematic = false;
			}
		}
	}

	private void SyncClienthealth(byte[] data)
	{
		Vector3 zero = Vector3.zero;
		DamageType damageType = DamageType.Other;
		byte attacker;
		float damage;
		bool flag;
		using (MemoryStream input = new MemoryStream(data))
		{
			using (BinaryReader binaryReader = new BinaryReader(input))
			{
				attacker = binaryReader.ReadByte();
				damage = binaryReader.ReadSingle();
				flag = binaryReader.ReadBoolean();
				if (flag)
				{
					zero.y = binaryReader.ReadSingle();
					zero.z = binaryReader.ReadSingle();
				}
				if (binaryReader.BaseStream.Position != binaryReader.BaseStream.Length)
				{
					damageType = (DamageType)binaryReader.ReadByte();
				}
			}
		}
		if (flag)
		{
			mDamageParticleSystem.transform.position = mHip.position + zero.normalized;
			mDamageParticleSystem.transform.rotation = Quaternion.LookRotation(zero);
			ParticleSystem.MainModule main = mDamageParticleSystem.main;
			main.startSpeedMultiplier = 61f;
			mDamageParticleSystem.Emit(20);
		}
		if (damageType == DamageType.Punch)
		{
			mPunchForce.PlayPunchSound();
		}
		mHealthHandler.TakeDamage(damage, attacker, mHasLocalControl);
	}

	private void SyncClientWonWithRicochet(byte[] data)
	{
		byte index;
		using (MemoryStream input = new MemoryStream(data))
		{
			using (BinaryReader binaryReader = new BinaryReader(input))
			{
				index = binaryReader.ReadByte();
			}
		}
		List<ConnectedClientData> list = new List<ConnectedClientData>();
		list.AddRange(UnityEngine.Object.FindObjectOfType<MultiplayerManager>().ConnectedClients);
		Controller component = list[index].PlayerObject.GetComponent<Controller>();
		if (component.HasControl)
		{
			SteamStatsAndAchievements.Instance.UnlockAchievement(SteamStatsAndAchievements.EAchievement.Ricochet);
		}
	}

	private void SyncClientWeaponThrow(byte[] data)
	{
		Vector3 zero = Vector3.zero;
		Vector3 zero2 = Vector3.zero;
		Vector3 zero3 = Vector3.zero;
		ushort spawnIndex = 0;
		ushort syncIndex = 0;
		bool flag;
		byte weaponIndex;
		using (MemoryStream input = new MemoryStream(data))
		{
			using (BinaryReader binaryReader = new BinaryReader(input))
			{
				flag = binaryReader.ReadBoolean();
				weaponIndex = binaryReader.ReadByte();
				zero.y = (float)binaryReader.ReadInt16() / 100f;
				zero.z = (float)binaryReader.ReadInt16() / 100f;
				zero2.y = (float)binaryReader.ReadSByte() / 100f;
				zero2.z = (float)binaryReader.ReadSByte() / 100f;
				if (!flag)
				{
					zero3.y = (float)binaryReader.ReadSByte() / 100f;
					zero3.z = (float)binaryReader.ReadSByte() / 100f;
					spawnIndex = binaryReader.ReadUInt16();
				}
				syncIndex = binaryReader.ReadUInt16();
			}
		}
		if (!mHasLocalControl || !flag)
		{
			mFighting.NetworkThrowWeapon(flag, weaponIndex, zero, zero2, zero3, spawnIndex, syncIndex);
		}
	}

	private void SyncClientState(byte[] data)
	{
		if (!mHasRecievedFirstPackage)
		{
			Rigidbody[] array = mAllRigidBodies;
			foreach (Rigidbody rigidbody in array)
			{
				rigidbody.isKinematic = false;
			}
			mIsActive = true;
			mHasRecievedFirstPackage = true;
		}
		Vector3 zero = Vector3.zero;
		Vector2 newLookRotation = default(Vector2);
		float newLeftStickYValue = 0f;
		byte b = 0;
		byte newMovementType = 0;
		byte b2;
		ProjectilePackageStruct[] array2;
		using (MemoryStream input = new MemoryStream(data))
		{
			using (BinaryReader binaryReader = new BinaryReader(input))
			{
				zero.y = (float)binaryReader.ReadInt16() / 100f;
				zero.z = (float)binaryReader.ReadInt16() / 100f;
				newLookRotation.x = (float)binaryReader.ReadSByte() / 100f;
				newLookRotation.y = (float)binaryReader.ReadSByte() / 100f;
				newLeftStickYValue = (float)binaryReader.ReadSByte() / 100f;
				newMovementType = binaryReader.ReadByte();
				b2 = binaryReader.ReadByte();
				ushort num = binaryReader.ReadUInt16();
				array2 = new ProjectilePackageStruct[num];
				for (ushort num2 = 0; num2 < num; num2++)
				{
					ShortVector2 shootPosition = new ShortVector2(binaryReader.ReadInt16(), binaryReader.ReadInt16());
					ByteVector2 shootVector = new ByteVector2(binaryReader.ReadSByte(), binaryReader.ReadSByte());
					ushort syncIndex = binaryReader.ReadUInt16();
					ProjectilePackageStruct projectilePackageStruct = new ProjectilePackageStruct
					{
						shootPosition = shootPosition,
						shootVector = shootVector,
						syncIndex = syncIndex
					};
					array2[num2] = projectilePackageStruct;
				}
				b = binaryReader.ReadByte();
			}
		}
		Vector3 vector = (zero - lastPackagePosition) * 0.5f;
		lastPackagePosition = zero;
		if ((bool)mHelpSphere)
		{
			mHelpSphere.position = zero;
		}
		mHelpPredictionSphere.position = zero;
		Vector3 zero2 = Vector3.zero;
		for (int j = 0; j < allMyRigs.Length; j++)
		{
			zero2 += allMyRigs[j].transform.position;
		}
		zero2 /= (float)allMyRigs.Length;
		Vector3 vector2 = CalculateDifference(zero, zero2);
		mIsLerping = true;
		mStartPos = base.transform.position;
		mEndPos = mStartPos + vector2;
		mFighting.SetFightState(b2);
		if (b2 == 1)
		{
			mFighting.FirePackages(array2, b);
		}
		if ((b2 == 1 && b != 0) || b2 == 0)
		{
			mFighting.NetworkPickUpWeapon(b);
		}
		mController.SetNewLookRotation(newLookRotation);
		mController.SetNewMovementType(newMovementType);
		mStanding.SetNewLeftStickYValue(newLeftStickYValue);
	}

	public void DelaySyncingBy(int nrOfFrames)
	{
		mDontSyncForFrames = nrOfFrames;
	}

	public void FallOut(Quaternion rot, Vector3 point)
	{
		byte[] array = new byte[29];
		using (MemoryStream output = new MemoryStream(array))
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(output))
			{
				binaryWriter.Write(mNetworkManager.LocalPlayerIndex);
				binaryWriter.Write(rot.x);
				binaryWriter.Write(rot.y);
				binaryWriter.Write(rot.z);
				binaryWriter.Write(rot.w);
				binaryWriter.Write(point.x);
				binaryWriter.Write(point.y);
				binaryWriter.Write(point.z);
			}
		}
		SyncClientFallOut(array);
		mNetworkManager.OnPlayerFallOut(array, mEventChannel, mNetworkSpawnID);
	}

	public void UnitWasDamaged(float damage, bool killingBlow, DamageType dmgType = DamageType.Other, bool playParticles = false, Vector3 particlePosition = default(Vector3), Vector3 particleDirection = default(Vector3))
	{
		byte b = mNetworkManager.LocalPlayerIndex;
		if (mController.damager != null)
		{
			for (int i = 0; i < mNetworkManager.ConnectedClients.Length; i++)
			{
				ConnectedClientData connectedClientData = mNetworkManager.ConnectedClients[i];
				if (object.ReferenceEquals(connectedClientData.PlayerObject, mController.damager.gameObject))
				{
					b = (byte)i;
					break;
				}
			}
		}
		int num = 2;
		byte[] array = new byte[8 + (playParticles ? (4 * num) : 0)];
		using (MemoryStream output = new MemoryStream(array))
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(output))
			{
				binaryWriter.Write(b);
				if (killingBlow)
				{
					binaryWriter.Write(666.666f);
				}
				else
				{
					binaryWriter.Write(damage);
				}
				binaryWriter.Write(playParticles);
				if (playParticles)
				{
					binaryWriter.Write(particleDirection.y);
					binaryWriter.Write(particleDirection.z);
				}
				binaryWriter.Write((byte)dmgType);
			}
		}
		mNetworkManager.OnPlayerTookDamage(array, mEventChannel, b);
	}

	public void ThrowWeapon(bool justDrop, byte index, Vector3 position, Vector3 rotation, Vector3 aimVector)
	{
		ShortVector2 shortVector = new ShortVector2(new Vector2(position.y, position.z));
		ByteVector2 byteVector = new ByteVector2(new Vector2(rotation.y, rotation.z));
		byte[] array = new byte[8 + ((!justDrop) ? 2 : 0)];
		using (MemoryStream output = new MemoryStream(array))
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(output))
			{
				binaryWriter.Write(justDrop);
				binaryWriter.Write(index);
				binaryWriter.Write(shortVector.X);
				binaryWriter.Write(shortVector.Y);
				binaryWriter.Write(byteVector.X);
				binaryWriter.Write(byteVector.Y);
				if (!justDrop)
				{
					ByteVector2 byteVector2 = new ByteVector2(aimVector);
					binaryWriter.Write(byteVector2.X);
					binaryWriter.Write(byteVector2.Y);
				}
			}
		}
		if (IsServer)
		{
			mNetworkManager.OnPlayerThrowWeapon(array, mEventChannel);
		}
		else
		{
			mPacketHandler.SendP2PPacketToServer(array, P2PPackageHandler.MsgType.RequestingWeaponThrow, EP2PSend.k_EP2PSendReliable, mEventChannel);
		}
	}

	public void WonWithRicochet()
	{
		byte value = mNetworkManager.LocalPlayerIndex;
		if (mController != null)
		{
			value = (byte)mNetworkManager.PlayerControllers.FindIndex((Controller x) => x == mController);
		}
		byte[] array = new byte[1];
		using (MemoryStream output = new MemoryStream(array))
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(output))
			{
				binaryWriter.Write(value);
			}
		}
		mNetworkManager.OnPlayerWonWithRicochet(array, mEventChannel);
	}

	public void DropWeapon(byte index, Vector3 position, Vector3 rotation)
	{
		Vector2 vector = new Vector2(position.y, position.z);
		Vector2 vec = new Vector2(rotation.y, rotation.z);
		ByteVector2 byteVector = new ByteVector2(vec);
		ShortVector2 shortVector = new ShortVector2(vector);
		byte[] array = new byte[7];
		using (MemoryStream output = new MemoryStream(array))
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(output))
			{
				binaryWriter.Write(index);
				binaryWriter.Write(shortVector.X);
				binaryWriter.Write(shortVector.Y);
				binaryWriter.Write(byteVector.X);
				binaryWriter.Write(byteVector.Y);
			}
		}
		if (IsServer)
		{
			mNetworkManager.OnPlayerDroppedWeapon(array);
			return;
		}
		Debug.Log("Requesting to drop weapon: " + index + " at pos: " + vector);
		mPacketHandler.SendP2PPacketToServer(array, P2PPackageHandler.MsgType.ClientRequestWeaponDrop);
	}

	private Vector3 CalculateDifference(Vector3 vec1, Vector3 vec2)
	{
		return vec1 - vec2;
	}

	public void SendAddedForceAndPlayBlock(byte index, Vector3 force, ForceMode mode, byte playerIndex)
	{
		byte[] array = new byte[15];
		using (MemoryStream output = new MemoryStream(array))
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(output))
			{
				binaryWriter.Write(index);
				binaryWriter.Write(force.x);
				binaryWriter.Write(force.y);
				binaryWriter.Write(force.z);
				binaryWriter.Write((byte)mode);
				binaryWriter.Write(playerIndex);
			}
		}
		mNetworkManager.OnPlayerBlockedAddedForce(array, mEventChannel);
	}

	public void SendAddedLavaForce(byte index, Vector3 force, ForceMode mode)
	{
		ByteVector2 byteVector = new ByteVector2(force);
		byte[] array = new byte[4];
		using (MemoryStream output = new MemoryStream(array))
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(output))
			{
				binaryWriter.Write(index);
				binaryWriter.Write(byteVector.X);
				binaryWriter.Write(byteVector.Y);
				binaryWriter.Write((byte)mode);
			}
		}
		mNetworkManager.OnPlayerAddedLavaForce(array, mEventChannel);
	}

	public void SendAddedForce(byte index, Vector3 force, ForceMode mode)
	{
		ByteVector2 byteVector = new ByteVector2(force, true);
		byte[] array = new byte[4];
		using (MemoryStream output = new MemoryStream(array))
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(output))
			{
				binaryWriter.Write(index);
				binaryWriter.Write(byteVector.X);
				binaryWriter.Write(byteVector.Y);
				binaryWriter.Write((byte)mode);
			}
		}
		mNetworkManager.OnPlayerAddedForce(array, mEventChannel);
	}

	private NetworkPositionPackage CreateNetworkPositionPackage()
	{
		Vector3 zero = Vector3.zero;
		for (int i = 0; i < allMyRigs.Length; i++)
		{
			zero += allMyRigs[i].transform.position;
		}
		zero /= (float)allMyRigs.Length;
		Vector3 vec = zero + mHipRigidBody.velocity * 0.05f;
		ShortVector2 position = new ShortVector2(vec);
		mNetworkPositionPackage.Position = position;
		Vector2 lookRotation = mController.LookRotation;
		ByteVector2 rotation = new ByteVector2(lookRotation);
		mNetworkPositionPackage.Rotation = rotation;
		mNetworkPositionPackage.MovementType = mController.MovementState;
		float num = float.Parse(mController.PlayerActions.Movement.Y.ToString("F2"));
		sbyte yValue = (sbyte)(num * 100f);
		mNetworkPositionPackage.YValue = yValue;
		return mNetworkPositionPackage;
	}

	private NetworkWeaponPackage CreateNetworkWeaponPackage()
	{
		byte fightState = mFighting.FightState;
		mNetworkWeaponPackage.FightState = fightState;
		mNetworkWeaponPackage.ProjectilePackages = new ProjectilePackageStruct[0];
		if (fightState == 1)
		{
			mNetworkWeaponPackage.ProjectilePackages = mFighting.GetProjectilePackages();
		}
		mNetworkWeaponPackage.WeaponType = mFighting.CurrentWeaponIndex;
		return mNetworkWeaponPackage;
	}

	public void OnTalked(string t)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(t);
		mNetworkManager.OnPlayerTalked(bytes, mEventChannel, mNetworkSpawnID);
		SyncClientChat(bytes);
	}

	public void SetActive(bool active)
	{
		mIsActive = active;
		if (!mIsActive)
		{
			mController.SetNewMovementType(0);
			mFighting.SetFightState(0);
		}
	}
}
