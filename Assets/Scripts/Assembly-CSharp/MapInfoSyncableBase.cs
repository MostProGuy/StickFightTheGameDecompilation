using UnityEngine;

public abstract class MapInfoSyncableBase : MonoBehaviour
{
	[SerializeField]
	protected float m_SendRatePerSecond = 5f;

	protected float m_SendRate;

	protected float m_CurrentSendTickCount;

	protected static MultiplayerManager m_NetworkManager;

	protected static bool m_NetworkControl;

	protected Vector2 m_StartPos;

	public Vector2 GetStartPos()
	{
		return m_StartPos;
	}

	protected virtual void Awake()
	{
		if (MatchmakingHandler.IsNetworkMatch)
		{
			m_SendRate = 1f / m_SendRatePerSecond;
			m_NetworkManager = Object.FindObjectOfType<MultiplayerManager>();
			m_NetworkControl = MatchmakingHandler.IsNetworkMatch && MultiplayerManager.IsServer;
			Vector3 position = base.transform.position;
			m_StartPos = new Vector2(position.y, position.z);
			m_NetworkManager.AddMapDataObject(m_StartPos, this);
		}
	}

	protected virtual void Update()
	{
		if (m_NetworkControl)
		{
			TickSyncPos();
		}
	}

	private void TickSyncPos()
	{
		TickCurrentSendTime();
		if (m_CurrentSendTickCount >= m_SendRate)
		{
			SendNewStatePackage();
			ResetCurrentSendTickTime();
		}
	}

	private void TickCurrentSendTime()
	{
		m_CurrentSendTickCount += Time.unscaledDeltaTime;
	}

	private void ResetCurrentSendTickTime()
	{
		m_CurrentSendTickCount = 0f;
	}

	protected void SendNewStatePackage()
	{
		m_NetworkManager.SyncMapData(this);
	}

	public abstract byte[] GetData();

	public abstract void SetData(byte[] data);
}
