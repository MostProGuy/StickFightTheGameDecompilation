using System;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

public class SteamStatsAndAchievements : MonoBehaviour
{
	public enum ELevelGroup
	{
		_1to32 = 0,
		_33to64 = 1,
		_65to96 = 2,
		_97to128 = 3,
		_129to160 = 4,
		MAX = 5
	}

	public enum EAchievement
	{
		Ace = 0,
		RoyalAce = 1,
		Explorer = 2,
		Conqueror = 3,
		Blackhole = 4,
		DoubleKill = 5,
		TripleKill = 6,
		KillingSpree = 7,
		Rampage = 8,
		Dominating = 9,
		Unstoppable = 10,
		Genocide = 11,
		Godlike = 12,
		WickedStick = 13,
		Snake = 14,
		StickIrvin = 15,
		Ricochet = 16,
		Riposte = 17,
		YourKungFuIsStrong = 18,
		Lightsaber = 19,
		Headshot = 20,
		WhiteDeath = 21,
		Bounce = 22,
		Walkover = 23,
		IceAge = 24,
		BlinkDagger = 25,
		APoultryMeal = 26,
		XiaoXiao = 27,
		MAX = 28
	}

	public delegate bool AchievementCheck();

	public struct AchievementData
	{
		public string m_strName;

		public string m_strDescription;

		public bool m_bAchieved;

		public AchievementData(string name, string desc, bool achieved)
		{
			m_strName = name;
			m_strDescription = desc;
			m_bAchieved = achieved;
		}
	}

	public const string STAT_KILLS_KEY = "STAT_Kills";

	public const string STAT_SNIPERKILLS_KEY = "STAT_SniperKills";

	public const string STAT_LEVELSCOMPLETED = "STAT_LevelsCompleted";

	public const string STAT_LEVELSWON = "STAT_LevelsWon";

	public Dictionary<EAchievement, AchievementData> mAchievements = new Dictionary<EAchievement, AchievementData>();

	public MemoryBucket TransientMemory = new MemoryBucket();

	public static MemoryBucket PersistentMemory = new MemoryBucket();

	private CGameID m_GameID;

	private bool m_bRequestedStats;

	private bool m_bStatsValid;

	private bool m_bStoreStats;

	protected Callback<UserStatsReceived_t> m_UserStatsReceived;

	protected Callback<UserStatsStored_t> m_UserStatsStored;

	protected Callback<UserAchievementStored_t> m_UserAchievementStored;

	private static SteamStatsAndAchievements _instance;

	public static SteamStatsAndAchievements Instance
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
		}
		else
		{
			_instance = this;
		}
	}

	private void OnEnable()
	{
		if (SteamManager.Initialized)
		{
			m_GameID = new CGameID(SteamUtils.GetAppID());
			m_UserStatsReceived = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
			m_UserStatsStored = Callback<UserStatsStored_t>.Create(OnUserStatsStored);
			m_UserAchievementStored = Callback<UserAchievementStored_t>.Create(OnAchievementStored);
			m_bRequestedStats = false;
			m_bStatsValid = false;
			PersistentMemory.OnValueChanged<int>("STAT_SniperKills", OnSniperKillsChanged);
			PersistentMemory.OnValueChanged<int>("STAT_Kills", OnKillsChanged);
		}
	}

	public void Start()
	{
		GameManager instance = GameManager.Instance;
		instance.OnMatchEnded = (Action)Delegate.Combine(instance.OnMatchEnded, new Action(OnMatchEnded));
	}

	private void OnKillsChanged(string key, int value)
	{
		if (value >= 99)
		{
			UnlockAchievement(EAchievement.APoultryMeal);
		}
	}

	private void OnSniperKillsChanged(string key, int value)
	{
		if (value >= 77)
		{
			UnlockAchievement(EAchievement.WhiteDeath);
		}
	}

	public void OnDisabled()
	{
		GameManager instance = GameManager.Instance;
		instance.OnMatchEnded = (Action)Delegate.Remove(instance.OnMatchEnded, new Action(OnMatchEnded));
	}

	private void Update()
	{
		if (!SteamManager.Initialized)
		{
			return;
		}
		if (!m_bRequestedStats)
		{
			if (!SteamManager.Initialized)
			{
				m_bRequestedStats = true;
				return;
			}
			bool bRequestedStats = SteamUserStats.RequestCurrentStats();
			m_bRequestedStats = bRequestedStats;
		}
		if (m_bStatsValid && m_bStoreStats)
		{
			SteamUserStats.SetStat("STAT_Kills", PersistentMemory.Copy<int>("STAT_Kills").GetValue(0));
			SteamUserStats.SetStat("STAT_SniperKills", PersistentMemory.Copy<int>("STAT_SniperKills").GetValue(0));
			for (int i = 0; i < 5; i++)
			{
				ELevelGroup eLevelGroup = (ELevelGroup)i;
				string text = "STAT_LevelsCompleted" + eLevelGroup;
				int value = PersistentMemory.Copy<int>(text).GetValue(0);
				SteamUserStats.SetStat(text, value);
				text = "STAT_LevelsWon" + eLevelGroup;
				int value2 = PersistentMemory.Copy<int>(text).GetValue(0);
				SteamUserStats.SetStat(text, value2);
			}
			bool flag = SteamUserStats.StoreStats();
			m_bStoreStats = !flag;
		}
	}

	private uint CountNumberOfSetBitsInInteger(uint i)
	{
		i -= (i >> 1) & 0x55555555;
		i = (i & 0x33333333) + ((i >> 2) & 0x33333333);
		return ((i + (i >> 4)) & 0xF0F0F0F) * 16843009 >> 24;
	}

	private int GetLevelFlagCount(string statPrefix)
	{
		int num = 0;
		for (int i = 0; i < 5; i++)
		{
			ELevelGroup eLevelGroup = (ELevelGroup)i;
			string key = statPrefix + eLevelGroup;
			num += (int)CountNumberOfSetBitsInInteger((uint)PersistentMemory.Copy<int>(key).GetValue(0));
		}
		return num;
	}

	private void SetLevelFlag(string statPrefix, ELevelGroup levelGroup, int bit)
	{
		string key = statPrefix + levelGroup;
		int value = PersistentMemory.Copy<int>(key).GetValue(0);
		value |= 1 << bit;
		PersistentMemory.Put(key, value);
	}

	public void OnMatchEnded()
	{
		MapWrapper currentMap = GameManager.Instance.GetCurrentMap();
		if (currentMap.MapType != 0)
		{
			return;
		}
		int num = BitConverter.ToInt32(currentMap.MapData, 0);
		int num2 = Mathf.FloorToInt(num / 32);
		int bit = num % 32;
		if (num2 >= 5)
		{
			return;
		}
		ELevelGroup levelGroup = (ELevelGroup)num2;
		SetLevelFlag("STAT_LevelsCompleted", levelGroup, bit);
		int levelFlagCount = GetLevelFlagCount("STAT_LevelsCompleted");
		if (levelFlagCount >= 94)
		{
			UnlockAchievement(EAchievement.Explorer);
		}
		List<Controller> playersAlive = GameManager.Instance.playersAlive;
		if (playersAlive != null && playersAlive.Count > 0 && playersAlive[0] != null && playersAlive[0].HasControl)
		{
			SetLevelFlag("STAT_LevelsWon", levelGroup, bit);
			int levelFlagCount2 = GetLevelFlagCount("STAT_LevelsWon");
			if (levelFlagCount2 >= 94)
			{
				UnlockAchievement(EAchievement.Conqueror);
			}
		}
	}

	public void CleanUpAndStoreStats()
	{
		TransientMemory.ClearMemory();
		m_bStoreStats = true;
	}

	public void UnlockAchievement(EAchievement achievement)
	{
		if (!mAchievements.ContainsKey(achievement))
		{
			Debug.LogWarning("Achievement with key " + achievement.ToString() + "(" + (int)achievement + ") not found");
			return;
		}
		AchievementData achievementData = mAchievements[achievement];
		Debug.Log("ACHIEVEMENT :: Trying to unlock " + achievement);
		if (!achievementData.m_bAchieved)
		{
			Debug.Log("ACHIEVEMENT :: Success");
			achievementData.m_bAchieved = true;
			if (SteamManager.Initialized)
			{
				SteamUserStats.SetAchievement(achievement.ToString());
			}
			m_bStoreStats = true;
			mAchievements[achievement] = new AchievementData(achievementData.m_strName, achievementData.m_strDescription, achievementData.m_bAchieved);
			if (mAchievements[EAchievement.XiaoXiao].m_bAchieved)
			{
				return;
			}
			bool flag = true;
			for (int i = 0; i < mAchievements.Count - 1; i++)
			{
				if (!mAchievements[(EAchievement)i].m_bAchieved)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				UnlockAchievement(EAchievement.XiaoXiao);
			}
		}
		else
		{
			Debug.Log("ACHIEVEMENT :: " + achievement.ToString() + " already achieved");
		}
	}

	private void OnUserStatsReceived(UserStatsReceived_t pCallback)
	{
		if (!SteamManager.Initialized || (ulong)m_GameID != pCallback.m_nGameID)
		{
			return;
		}
		if (pCallback.m_eResult == EResult.k_EResultOK)
		{
			Debug.Log("Received stats and achievements from Steam\n");
			m_bStatsValid = true;
			mAchievements.Clear();
			for (int i = 0; i < 28; i++)
			{
				EAchievement key = (EAchievement)i;
				bool pbAchieved = false;
				string text = string.Empty;
				string desc = string.Empty;
				if (SteamUserStats.GetAchievement(key.ToString(), out pbAchieved))
				{
					text = SteamUserStats.GetAchievementDisplayAttribute(key.ToString(), "name");
					desc = SteamUserStats.GetAchievementDisplayAttribute(key.ToString(), "desc");
				}
				else
				{
					Debug.LogWarning("SteamUserStats.GetAchievement failed for Achievement " + key.ToString() + "\nIs it registered in the Steam Partner site?");
				}
				mAchievements.Add(key, new AchievementData(text, desc, pbAchieved));
			}
			int pData = 0;
			SteamUserStats.GetStat("STAT_Kills", out pData);
			PersistentMemory.Put("STAT_Kills", pData);
			pData = 0;
			SteamUserStats.GetStat("STAT_SniperKills", out pData);
			PersistentMemory.Put("STAT_SniperKills", pData);
			for (int j = 0; j < 5; j++)
			{
				ELevelGroup eLevelGroup = (ELevelGroup)j;
				string text2 = "STAT_LevelsCompleted" + eLevelGroup;
				int pData2 = 0;
				SteamUserStats.GetStat(text2, out pData2);
				PersistentMemory.Put(text2, pData2);
				text2 = "STAT_LevelsWon" + eLevelGroup;
				int pData3 = 0;
				SteamUserStats.GetStat(text2, out pData3);
				PersistentMemory.Put(text2, pData3);
			}
		}
		else
		{
			Debug.Log("RequestStats - failed, " + pCallback.m_eResult);
		}
	}

	private void OnUserStatsStored(UserStatsStored_t pCallback)
	{
		if ((ulong)m_GameID == pCallback.m_nGameID)
		{
			if (pCallback.m_eResult == EResult.k_EResultOK)
			{
				Debug.Log("StoreStats - success");
			}
			else if (pCallback.m_eResult == EResult.k_EResultInvalidParam)
			{
				Debug.Log("StoreStats - some failed to validate");
				OnUserStatsReceived(new UserStatsReceived_t
				{
					m_eResult = EResult.k_EResultOK,
					m_nGameID = (ulong)m_GameID
				});
			}
			else
			{
				Debug.Log("StoreStats - failed, " + pCallback.m_eResult);
			}
		}
	}

	private void OnAchievementStored(UserAchievementStored_t pCallback)
	{
		if ((ulong)m_GameID == pCallback.m_nGameID)
		{
			if (pCallback.m_nMaxProgress == 0)
			{
				Debug.Log("Achievement '" + pCallback.m_rgchAchievementName + "' unlocked!");
				return;
			}
			Debug.Log("Achievement '" + pCallback.m_rgchAchievementName + "' progress callback, (" + pCallback.m_nCurProgress + "," + pCallback.m_nMaxProgress + ")");
		}
	}
}
