using System.IO;
using Steamworks;
using UnityEngine;

[ExecuteInEditMode]
public class NetworkSyncableObject : MonoBehaviour
{
	public struct NetworkPositionPackage
	{
		public ShortVector2 Position;

		public ShortVector2 Rotation;

		public int Size
		{
			get
			{
				return 8;
			}
		}
	}

	protected bool mDontSyncPos;

	public bool mStopWhenKinematic;

	private const float mSyncDelay = 0.2f;

	private NetworkPositionPackage mNetworkPositionPackage;

	private NetworkPositionPackage mLastPositionPackage = default(NetworkPositionPackage);

	private float m_TimeSinceLastPackage;

	[SerializeField]
	protected Transform mObjectToSync;

	[SerializeField]
	private float mSendRatePerSecond = 5f;

	private float mSendRate;

	private float mCurrentSendTickCount;

	[SerializeField]
	protected ushort m_Index;

	private static bool mHasControl;

	private bool mIsListening;

	protected MultiplayerManager mNetworkManager;

	protected P2PPackageHandler mPacketHandler;

	private Vector3 m_EndPos;

	private Quaternion m_EndRot;

	[SerializeField]
	protected bool m_AllowForceFromClient = true;

	[SerializeField]
	protected bool m_HardSync;

	[SerializeField]
	protected bool m_VelocitySync;

	[SerializeField]
	protected bool m_SyncRotation = true;

	[SerializeField]
	public bool m_OnlyRecieveInitState;

	[SerializeField]
	protected float m_DeadZone = 0.1f;

	private Transform mHelpSphere;

	private bool mHasRecievedHelloPackage;

	[SerializeField]
	[HideInInspector]
	private bool firstPassFlag = true;

	[SerializeField]
	protected bool m_UsesMoveAlongPathUsingForce = true;

	[SerializeField]
	public bool m_ShouldDisableAllRigidBodiesOnInit = true;

	private bool mIsSnake;

	private SnakeAI mSnakeComponent;

	private int mUpdateIndex;

	private float m_DontSyncForSeconds;

	private Rigidbody[] mAllRigidBodies;

	private bool mIsLerping;

	private Rigidbody mRigidBody;

	private Vector3 mMoveForce;

	public float mDirectionFractor = 0.04f;

	public float mLerpFriction = 0.8f;

	public float roationLerpSpeed = 5f;

	private float m_TimeBetweenPackages;

	private float m_TimeOfLastPackage;

	private Vector3 m_DistanceToTravel;

	private float m_AngleToTravel;

	private float m_PositionSpeed;

	private float m_RotationSpeed;

	private Vector3 m_TargetAngle;

	private float mLastSnakeSendTime;

	public ushort Index
	{
		get
		{
			return m_Index;
		}
	}

	public bool ListeningForPackages
	{
		get
		{
			return mIsListening;
		}
	}

	public float DontSyncForSeconds
	{
		get
		{
			return m_DontSyncForSeconds;
		}
	}

	protected virtual void Awake()
	{
		if (!MatchmakingHandler.IsNetworkMatch)
		{
			return;
		}
		mUpdateIndex = UpdateIndexHandler.UPDATE_INDEX;
		mNetworkManager = GameManager.Instance.mMultiplayerManager;
		mPacketHandler = GameManager.Instance.P2PPackageHandler;
		mSendRate = 1f / mSendRatePerSecond;
		if (mObjectToSync == null)
		{
			mSnakeComponent = GetComponent<SnakeAI>();
			if ((bool)mSnakeComponent)
			{
				mIsSnake = true;
			}
			else
			{
				mObjectToSync = base.transform;
			}
		}
		if ((bool)GetComponent<MoveAlongPathUsingForce>())
		{
			m_OnlyRecieveInitState = true;
		}
	}

	public void Init()
	{
		if (m_Index != ushort.MaxValue)
		{
			mNetworkManager.AddSyncableObject(m_Index, this);
			mIsListening = true;
		}
		InitRigidBodies();
		if (!mHasControl && m_ShouldDisableAllRigidBodiesOnInit)
		{
			DisableAllRigidBodies();
		}
		mRigidBody = GetComponentInChildren<Rigidbody>();
		if (mObjectToSync != null)
		{
			m_EndPos = mObjectToSync.position;
		}
	}

	public void InitNetworkIndex(ushort syncIndex, bool syncRotation, float dontSyncForSeconds = 0f)
	{
		m_Index = syncIndex;
		m_SyncRotation = syncRotation;
		m_DontSyncForSeconds = dontSyncForSeconds;
		mIsListening = true;
		Debug.Log("Inited A Runtime Spawned Syncableobject with index: " + m_Index + " Name: " + base.gameObject.name);
	}

	private void Start()
	{
		if (!Application.isPlaying || !MatchmakingHandler.IsNetworkMatch)
		{
			return;
		}
		mHasControl = MultiplayerManager.IsServer;
		if (m_OnlyRecieveInitState && m_UsesMoveAlongPathUsingForce)
		{
			if (mHasControl)
			{
				SendHelloStatusPackageToAllUsers();
			}
			else if (!mHasRecievedHelloPackage)
			{
				RequestHelloPackage();
			}
		}
	}

	public void UpdateHost()
	{
		mHasControl = MultiplayerManager.IsServer;
	}

	private void InitRigidBodies()
	{
		mAllRigidBodies = GetComponentsInChildren<Rigidbody>();
		int num = mAllRigidBodies.Length;
		for (byte b = 0; b < num; b++)
		{
			Rigidbody rigidbody = mAllRigidBodies[b];
			rigidbody.gameObject.AddComponent<RigidBodyIndexHolder>().InitIndex(b);
		}
	}

	private void DisableAllRigidBodies()
	{
		if (Application.isPlaying)
		{
			bool flag = base.gameObject.GetComponent<MoveAlongPathUsingForce>() != null;
			Rigidbody[] componentsInChildren = GetComponentsInChildren<Rigidbody>();
			int num = componentsInChildren.Length;
			for (ushort num2 = 0; num2 < num; num2++)
			{
				componentsInChildren[num2].isKinematic = !flag && !m_HardSync;
			}
			mIsListening = true;
		}
	}

	private void Update()
	{
		if (Application.isPlaying && mIsListening && MatchmakingHandler.IsNetworkMatch && !m_OnlyRecieveInitState && !mHasControl && mIsLerping && !mIsSnake)
		{
			LerpLocalDummy();
		}
	}

	private void LateUpdate()
	{
		if (Application.isPlaying && MatchmakingHandler.IsNetworkMatch && !(GameManager.Instance.matchTime < 1f))
		{
			if (m_DontSyncForSeconds > -0.2f)
			{
				m_DontSyncForSeconds -= Time.unscaledDeltaTime;
			}
			if (mHasControl && mIsListening && !mIsSnake && !m_OnlyRecieveInitState)
			{
				TickSyncPos();
			}
			if (!mIsSnake && mObjectToSync.position.y < -50f && mIsListening)
			{
				SendNewObjectStatePackage();
				mIsListening = false;
			}
		}
	}

	private void OnDestroy()
	{
		mIsListening = false;
	}

	private void LerpLocalDummy()
	{
		base.transform.position += m_DistanceToTravel.normalized * m_PositionSpeed * Time.deltaTime;
		Vector3 vector = Vector3.RotateTowards(base.transform.up, m_TargetAngle, m_RotationSpeed * 0.01f * Time.deltaTime, 0f);
		base.transform.rotation = Quaternion.LookRotation(Vector3.Cross(Vector3.right, vector), vector);
	}

	private void TickSyncPos()
	{
		TickCurrentSendTime();
		if (mCurrentSendTickCount >= mSendRate && mUpdateIndex == UpdateIndexHandler.FRAME_UPDATE_INDEX)
		{
			SendNewObjectStatePackage();
			ResetCurrentSendTickTime();
		}
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
			if (m_DontSyncForSeconds > 0f)
			{
				break;
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
					ReceivedPackage(msgType, psteamIDRemote, data);
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
						Debug.LogWarning("Packet is obsolete, throwing away!");
						continue;
					}
					byte[] data = binaryReader.ReadBytes((int)(pcubMsgSize - 1));
					ReceivedPackage(msgType, psteamIDRemote, data);
				}
			}
		}
	}

	public virtual void ReceivedPackage(P2PPackageHandler.MsgType msgType, CSteamID steamId, byte[] data)
	{
		switch (msgType)
		{
		case P2PPackageHandler.MsgType.PlayerForceAdded:
			SyncObjectForce(data);
			break;
		case P2PPackageHandler.MsgType.ObjectHello:
			if (mHasControl)
			{
				SendReturnHelloPackage(steamId);
			}
			else
			{
				SyncObjectHelloPakage(data);
			}
			break;
		case P2PPackageHandler.MsgType.ObjectUpdate:
			SyncObjectState(data);
			break;
		case P2PPackageHandler.MsgType.ObjectDestructionCollision:
			break;
		default:
			Debug.LogError("Invalid Messagetype " + msgType);
			break;
		}
	}

	private void SyncObjectForce(byte[] data)
	{
		byte b = 0;
		Vector3 zero = Vector3.zero;
		ForceMode mode;
		using (MemoryStream input = new MemoryStream(data))
		{
			using (BinaryReader binaryReader = new BinaryReader(input))
			{
				binaryReader.ReadUInt16();
				b = binaryReader.ReadByte();
				zero.y = binaryReader.ReadSByte() * 100;
				zero.z = binaryReader.ReadSByte() * 100;
				mode = (ForceMode)binaryReader.ReadByte();
			}
		}
		mAllRigidBodies[b].AddForce(zero, mode);
	}

	private void SyncObjectState(byte[] data)
	{
		if (mDontSyncPos)
		{
			return;
		}
		Vector3 zero = Vector3.zero;
		Vector3 zero2 = Vector3.zero;
		Vector3 vector = new Vector3(0f, 0f, 0f);
		Vector3 zero3 = Vector3.zero;
		byte b = 0;
		if (!mIsSnake)
		{
			mIsSnake = GetComponent<SnakeAI>() != null;
		}
		using (MemoryStream input = new MemoryStream(data))
		{
			using (BinaryReader binaryReader = new BinaryReader(input))
			{
				binaryReader.ReadUInt16();
				if (mIsSnake)
				{
					b = binaryReader.ReadByte();
				}
				else
				{
					zero.y = (float)binaryReader.ReadInt16() / 100f;
					zero.z = (float)binaryReader.ReadInt16() / 100f;
					zero2.y = (float)binaryReader.ReadInt16() / 100f;
					zero2.z = (float)binaryReader.ReadInt16() / 100f;
				}
			}
		}
		if (mIsSnake)
		{
			if (b >= mNetworkManager.PlayerControllers.Count && b != byte.MaxValue)
			{
				Debug.LogWarning("Received invalid snake target");
			}
			Controller c = ((b < mNetworkManager.PlayerControllers.Count) ? mNetworkManager.PlayerControllers[b] : null);
			mSnakeComponent.NetworkForceNewTarget(c);
			return;
		}
		m_TimeBetweenPackages = Time.time - m_TimeOfLastPackage;
		m_TimeOfLastPackage = Time.time;
		m_EndPos = zero;
		m_DistanceToTravel = m_EndPos - base.transform.position;
		m_PositionSpeed = m_DistanceToTravel.magnitude / m_TimeBetweenPackages;
		m_AngleToTravel = Vector3.Angle(base.transform.up, zero2);
		m_RotationSpeed = m_AngleToTravel / m_TimeBetweenPackages;
		m_TargetAngle = zero2;
		if (Application.isEditor && (bool)mHelpSphere)
		{
			mHelpSphere.position = new Vector3(-1f, m_EndPos.y, m_EndPos.z);
		}
		if (!mIsLerping)
		{
			mIsLerping = true;
		}
	}

	private bool ValidatePackage(Vector3 position)
	{
		if (Vector3.Distance(position, mRigidBody.position - mRigidBody.velocity) < 1f)
		{
			return false;
		}
		return true;
	}

	public void ForceAdded(byte rigIndex, Vector3 force, ForceMode mode)
	{
		if (!m_AllowForceFromClient)
		{
			return;
		}
		ByteVector2 byteVector = new ByteVector2(force, true);
		byte[] array = new byte[6];
		using (MemoryStream output = new MemoryStream(array))
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(output))
			{
				binaryWriter.Write(Index);
				binaryWriter.Write(rigIndex);
				binaryWriter.Write(byteVector.X);
				binaryWriter.Write(byteVector.Y);
				binaryWriter.Write((byte)mode);
			}
		}
		mAllRigidBodies[rigIndex].AddForce(force, mode);
		m_DontSyncForSeconds = 0.2f;
		mNetworkManager.OnPlayerAddedForce(array, 11);
	}

	private void SendNewObjectStatePackage()
	{
		if (mDontSyncPos)
		{
			return;
		}
		if (mStopWhenKinematic)
		{
			Rigidbody component = GetComponent<Rigidbody>();
			if ((bool)component && component.isKinematic)
			{
				return;
			}
		}
		mNetworkPositionPackage = CreateNetworkPositionPackage();
		byte[] array = new byte[2 + mNetworkPositionPackage.Size];
		using (MemoryStream output = new MemoryStream(array))
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(output))
			{
				binaryWriter.Write(Index);
				binaryWriter.Write(mNetworkPositionPackage.Position.X);
				binaryWriter.Write(mNetworkPositionPackage.Position.Y);
				binaryWriter.Write(mNetworkPositionPackage.Rotation.X);
				binaryWriter.Write(mNetworkPositionPackage.Rotation.Y);
			}
		}
		mNetworkManager.OnObjectMoved(array, 10);
	}

	private NetworkPositionPackage CreateNetworkPositionPackage()
	{
		mNetworkPositionPackage.Position = new ShortVector2(new Vector3(0f, mObjectToSync.position.y, mObjectToSync.position.z));
		Vector3 up = mObjectToSync.up;
		Vector2 vec = new Vector2(up.y, up.z);
		ShortVector2 rotation = new ShortVector2(vec);
		mNetworkPositionPackage.Rotation = rotation;
		return mNetworkPositionPackage;
	}

	private float ClampDegrees(float degrees)
	{
		degrees %= 360f;
		if (degrees < 0f)
		{
			degrees += 360f;
		}
		return degrees;
	}

	private void TickCurrentSendTime()
	{
		mCurrentSendTickCount += Time.unscaledDeltaTime;
	}

	private void ResetCurrentSendTickTime()
	{
		mCurrentSendTickCount = 0f;
	}

	private void RequestHelloPackage()
	{
		Debug.Log("Requesting hello package");
		byte[] array = new byte[2];
		using (MemoryStream output = new MemoryStream(array))
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(output))
			{
				binaryWriter.Write(Index);
			}
		}
		mNetworkManager.SyncObjectHello(array, 11);
	}

	private void SyncObjectHelloPakage(byte[] data)
	{
		if (mHasRecievedHelloPackage)
		{
			return;
		}
		mHasRecievedHelloPackage = true;
		Debug.Log("Recieved Hello Package with data: " + data);
		MoveAlongPathUsingForce component = GetComponent<MoveAlongPathUsingForce>();
		byte newPositionIndex = 0;
		if (!component)
		{
			Debug.LogWarning("Missing MoveAlongPathUsingForce component");
			return;
		}
		using (MemoryStream input = new MemoryStream(data))
		{
			using (BinaryReader binaryReader = new BinaryReader(input))
			{
				binaryReader.ReadUInt16();
				newPositionIndex = binaryReader.ReadByte();
			}
		}
		component.SetNewPositionIndex(newPositionIndex);
	}

	private void SendHelloStatusPackageToAllUsers()
	{
		byte[] array = CreateHelloPackage();
		if (array != null)
		{
			Debug.Log("Sending Hello Package TO all");
			mNetworkManager.SendObjectHello(array, 11);
		}
		else
		{
			Debug.LogWarning("Trying to send hello but missing MoveAlong");
		}
	}

	private void SendReturnHelloPackage(CSteamID user)
	{
		byte[] array = CreateHelloPackage();
		if (array != null)
		{
			Debug.Log("Got a hello package request, sending message");
			mPacketHandler.SendP2PPacketToUser(user, array, P2PPackageHandler.MsgType.ObjectHello, EP2PSend.k_EP2PSendReliable, 11);
		}
		else
		{
			Debug.LogWarning("Got a hello package request but missing MoveAlong");
		}
	}

	private byte[] CreateHelloPackage()
	{
		MoveAlongPathUsingForce component = GetComponent<MoveAlongPathUsingForce>();
		if (component == null)
		{
			return null;
		}
		byte currentPositionIndexWithLatency = component.GetCurrentPositionIndexWithLatency(100f);
		byte[] array = new byte[3];
		using (MemoryStream output = new MemoryStream(array))
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(output))
			{
				binaryWriter.Write(Index);
				binaryWriter.Write(currentPositionIndexWithLatency);
			}
		}
		Debug.Log("Created Hello package with data: " + array);
		return array;
	}

	public void NewSnakeTarget(byte targetPlayerIndex)
	{
		if (Time.time - mLastSnakeSendTime < 0.5f)
		{
			return;
		}
		mLastSnakeSendTime = Time.time;
		byte[] array = new byte[3];
		using (MemoryStream output = new MemoryStream(array))
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(output))
			{
				binaryWriter.Write(Index);
				binaryWriter.Write(targetPlayerIndex);
			}
		}
		if ((bool)mNetworkManager)
		{
			mNetworkManager.OnObjectMoved(array, 10);
			Controller c = ((targetPlayerIndex < mNetworkManager.PlayerControllers.Count) ? mNetworkManager.PlayerControllers[targetPlayerIndex] : null);
			mSnakeComponent.NetworkForceNewTarget(c);
		}
	}
}
