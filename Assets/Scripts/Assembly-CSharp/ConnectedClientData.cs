using Steamworks;
using UnityEngine;

public class ConnectedClientData
{
	public CSteamID ClientID;

	public GameObject PlayerObject;

	public bool Ready;

	public ulong LastTick;

	private bool m_IsSocket;

	private bool m_Control;

	private float mPingInMs;

	public bool IsRed;

	private ClientStats mStats;

	public bool ControlledLocally
	{
		get
		{
			return (!m_IsSocket) ? (ClientID == SteamUser.GetSteamID()) : m_Control;
		}
	}

	public string PlayerName
	{
		get
		{
			return SteamFriends.GetFriendPersonaName(ClientID);
		}
	}

	public bool Spawned
	{
		get
		{
			return PlayerObject != null;
		}
	}

	public string Ping
	{
		get
		{
			if (OptionsHolder.ping == 1)
			{
				return string.Empty;
			}
			if (!ControlledLocally)
			{
				P2PSessionState_t pConnectionState;
				if (SteamNetworking.GetP2PSessionState(ClientID, out pConnectionState))
				{
					Debug.Log("Connected To This Client? " + pConnectionState.m_bConnectionActive + " Using Relay? " + pConnectionState.m_bUsingRelay);
				}
				else
				{
					Debug.LogError("Could Not Retrieve P2pSessionState For client: " + SteamFriends.GetFriendPersonaName(ClientID));
				}
			}
			return (!ControlledLocally) ? (" : " + Mathf.RoundToInt(mPingInMs) + " ms") : string.Empty;
		}
	}

	public ClientStats Stats
	{
		get
		{
			if (mStats == null)
			{
				return new ClientStats();
			}
			return mStats;
		}
		set
		{
			mStats = value;
		}
	}

	public ConnectedClientData(bool isSocket = false)
	{
		m_IsSocket = isSocket;
	}

	public void SetControl(bool control)
	{
		m_Control = control;
	}

	public void UpdatePing(float newPing)
	{
		float num = (mPingInMs + newPing) / 2f;
		if (ControlledLocally)
		{
			Debug.Log("Updating Ping For Local Client!?");
		}
		else
		{
			Debug.Log("Updating Ping For user: " + PlayerName + " Old Ping: " + mPingInMs + " New Ping: " + newPing + " AVG: " + num);
		}
		mPingInMs = num;
	}
}
