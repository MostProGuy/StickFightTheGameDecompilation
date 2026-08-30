using System.IO;
using Steamworks;
using UnityEngine;
using UnityEngine.Events;

public class DestructiblePiece : NetworkSyncableObject
{
	public float forceThreshold;

	private Rigidbody rig;

	private ScreenshakeHandler screenshake;

	private float counter;

	public Controller damager;

	public bool simpleDestruction;

	public bool eventDestruction;

	public UnityEvent destructionEvent;

	public IceDestructionAudio iceDestructionAudio;

	private bool mInited;

	private bool mAssigned;

	private bool mSentDestructionForThisPiece;

	protected override void Awake()
	{
		base.Awake();
		m_SyncRotation = false;
		m_UsesMoveAlongPathUsingForce = false;
		m_ShouldDisableAllRigidBodiesOnInit = false;
		mDontSyncPos = true;
		rig = GetComponent<Rigidbody>();
		screenshake = ScreenshakeHandler.Instance;
		if (MatchmakingHandler.IsNetworkMatch && !GetComponent<Prop>())
		{
			mAssigned = true;
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		float multiplier = 1f;
		Rigidbody rigidbody = collision.rigidbody;
		if ((bool)rigidbody && rigidbody.mass > 300f)
		{
			multiplier = 10f;
		}
		if ((bool)rigidbody && rigidbody.mass > 1000f)
		{
			multiplier = 50f;
		}
		if (MatchmakingHandler.IsNetworkMatch && (bool)rigidbody && rigidbody.mass < 300f)
		{
			Controller component = rigidbody.transform.root.GetComponent<Controller>();
			if ((bool)component && !component.HasControl)
			{
				return;
			}
		}
		Collide(collision.relativeVelocity * 0.1f, multiplier);
	}

	public override void ReceivedPackage(P2PPackageHandler.MsgType msgType, CSteamID steamId, byte[] data)
	{
		if (msgType == P2PPackageHandler.MsgType.ObjectDestructionCollision)
		{
			ReceivedDestruction(data);
		}
		else
		{
			base.ReceivedPackage(msgType, steamId, data);
		}
	}

	private void ReceivedDestruction(byte[] data)
	{
		if (MultiplayerManager.IsServer)
		{
			mNetworkManager.OnDestructibleDestroyed(data, 11);
		}
		Vector3 zero = Vector3.zero;
		using (MemoryStream input = new MemoryStream(data))
		{
			using (BinaryReader binaryReader = new BinaryReader(input))
			{
				binaryReader.ReadInt16();
				zero.y = (float)binaryReader.ReadInt16() / 100f;
				zero.z = (float)binaryReader.ReadInt16() / 100f;
				float multiplier = binaryReader.ReadSingle();
				NetworkForceDestruction(zero, multiplier);
			}
		}
	}

	private void SendDestructMessage(Vector3 force, float multiplier)
	{
		byte[] array = new byte[10];
		ShortVector2 shortVector = new ShortVector2(force);
		using (MemoryStream output = new MemoryStream(array))
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(output))
			{
				binaryWriter.Write(base.Index);
				binaryWriter.Write(shortVector.X);
				binaryWriter.Write(shortVector.Y);
				binaryWriter.Write(multiplier);
			}
		}
		if (MultiplayerManager.IsServer)
		{
			mNetworkManager.OnDestructibleDestroyed(array, 11);
			NetworkForceDestruction(force, multiplier);
		}
		else
		{
			mPacketHandler.SendP2PPacketToServer(array, P2PPackageHandler.MsgType.ObjectDestructionCollision, EP2PSend.k_EP2PSendReliable, 11);
		}
	}

	public void Collide(Vector3 force, float multiplier, bool networkForce = false)
	{
		if (MatchmakingHandler.IsNetworkMatch && !networkForce)
		{
			if (mSentDestructionForThisPiece)
			{
				return;
			}
			bool flag = false;
			if ((simpleDestruction || eventDestruction) && force.magnitude * multiplier > forceThreshold)
			{
				flag = true;
			}
			float num = multiplier;
			if (!flag)
			{
				num *= (float)(rig.isKinematic ? 1 : 2);
				if (force.magnitude * num > forceThreshold)
				{
					flag = true;
				}
			}
			if (flag)
			{
				SendDestructMessage(force, num);
				mSentDestructionForThisPiece = true;
			}
			return;
		}
		if (simpleDestruction)
		{
			if (force.magnitude * multiplier > forceThreshold)
			{
				ConfigurableJoint[] componentsInChildren = GetComponentsInChildren<ConfigurableJoint>();
				foreach (ConfigurableJoint obj in componentsInChildren)
				{
					Object.Destroy(obj);
				}
				Collider[] componentsInChildren2 = GetComponentsInChildren<Collider>();
				foreach (Collider obj2 in componentsInChildren2)
				{
					Object.Destroy(obj2);
				}
			}
			return;
		}
		if (eventDestruction)
		{
			if (force.magnitude * multiplier > forceThreshold)
			{
				destructionEvent.Invoke();
			}
			return;
		}
		float num2 = 1f;
		if (!rig.isKinematic)
		{
			num2 = 0.8f;
		}
		if (!(force.magnitude * multiplier * num2 > forceThreshold))
		{
			return;
		}
		if (rig.isKinematic && (bool)iceDestructionAudio)
		{
			iceDestructionAudio.PlayDestruction();
		}
		Vector3 center = rig.transform.GetComponent<Renderer>().bounds.center;
		Rigidbody[] componentsInChildren3 = base.transform.parent.GetComponentsInChildren<Rigidbody>();
		for (int k = 0; k < componentsInChildren3.Length; k++)
		{
			if (!(componentsInChildren3[k] == rig))
			{
				float num3 = Vector3.Distance(center, componentsInChildren3[k].transform.GetComponent<Renderer>().bounds.center);
				float num4 = force.magnitude / forceThreshold * 1f;
				if (num3 < num4 * multiplier * num2)
				{
					componentsInChildren3[k].isKinematic = false;
					float num5 = 1f - num3 / (num4 * multiplier * num2);
					componentsInChildren3[k].AddForce(force * num5 * 300f, ForceMode.Impulse);
					screenshake.AddShake(force * multiplier * 0.001f);
				}
			}
		}
		rig.isKinematic = false;
		Prop[] componentsInChildren4 = base.transform.parent.GetComponentsInChildren<Prop>();
		for (int l = 0; l < componentsInChildren4.Length; l++)
		{
			float num6 = Vector3.Distance(center, componentsInChildren4[l].transform.position);
			float num7 = force.magnitude / forceThreshold * 1f;
			if (num6 < num7 * multiplier * num2 + 2f)
			{
				componentsInChildren4[l].Destroy();
			}
		}
	}

	public void NetworkForceSimpleDestruction()
	{
		ConfigurableJoint[] componentsInChildren = GetComponentsInChildren<ConfigurableJoint>();
		foreach (ConfigurableJoint obj in componentsInChildren)
		{
			Object.Destroy(obj);
		}
		Collider[] componentsInChildren2 = GetComponentsInChildren<Collider>();
		foreach (Collider obj2 in componentsInChildren2)
		{
			Object.Destroy(obj2);
		}
	}

	public void NetworkForceEvent()
	{
		destructionEvent.Invoke();
	}

	public void NetworkForceDestruction(Vector3 force, float multiplier)
	{
		Collide(force, multiplier, true);
	}
}
