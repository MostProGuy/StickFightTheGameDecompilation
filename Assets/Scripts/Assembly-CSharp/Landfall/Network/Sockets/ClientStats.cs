using UnityEngine;

namespace Landfall.Network.Sockets
{
	public class ClientStats
	{
		private uint mKills;

		private uint mDeaths;

		private uint mPoints;

		public uint Kills
		{
			get
			{
				return mKills;
			}
		}

		public uint Deaths
		{
			get
			{
				return mDeaths;
			}
		}

		public uint Points
		{
			get
			{
				return mPoints;
			}
		}

		public ClientStats()
		{
		}

		public ClientStats(uint kills, uint deaths, uint points)
		{
			mKills = kills;
			mDeaths = deaths;
			mPoints = points;
		}

		public void AddKill()
		{
			mKills++;
			Debug.Log("Added Kill! " + mKills);
		}

		public void AddDeath()
		{
			mDeaths++;
		}

		public void Clear()
		{
			mKills = 0u;
			mDeaths = 0u;
			mPoints = 0u;
		}
	}
}
