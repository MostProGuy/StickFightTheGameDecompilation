using Lidgren.Network;
using UnityEngine;

namespace Landfall.Network.Sockets
{
	public class ConnectedClientData
	{
		private string m_PlayerName;

		public NetConnection ClientSocketID;

		public GameObject PlayerObject;

		public bool Ready;

		public ulong LastTick;

		private bool m_Control;

		private float mPingInMs;

		public bool IsRed;

		private ClientStats mStats;

		public bool ControlledLocally
		{
			get
			{
				return m_Control;
			}
		}

		public string PlayerName
		{
			get
			{
				return m_PlayerName;
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

		public ConnectedClientData(string playerName)
		{
			m_PlayerName = playerName;
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
}
