using System;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

public class MatchmakingHandler : MonoBehaviour
{
	private static bool mHasTriedJoiningOnline;

	public static bool HasSuccededJoining;

	private static bool mIsNetworkMatch;

	private CSteamID m_Lobby;

	private ELobbyDistanceFilter mCurrentDistanceFilter;

	private ConnectionType mCurrentConnectionType;

	protected Callback<FavoritesListChanged_t> m_FavoritesListChanged;

	protected Callback<LobbyInvite_t> m_LobbyInvite;

	protected Callback<LobbyEnter_t> m_LobbyEnter;

	protected Callback<LobbyDataUpdate_t> m_LobbyDataUpdate;

	protected Callback<LobbyChatUpdate_t> m_LobbyChatUpdate;

	protected Callback<LobbyChatMsg_t> m_LobbyChatMsg;

	protected Callback<LobbyGameCreated_t> m_LobbyGameCreated;

	protected Callback<GameLobbyJoinRequested_t> m_LobbyJoinRequest;

	private CallResult<LobbyEnter_t> OnLobbyEnterCallResult;

	private CallResult<LobbyMatchList_t> OnLobbyMatchListCallResult;

	private CallResult<LobbyCreated_t> OnLobbyCreatedCallResult;

	private CallResult<LobbyEnter_t> mLobbyEntered;

	[SerializeField]
	private bool mRunningOnSockets;

	private const int FAILED_ATTEMPTS_UNTIL_EXIT = 10;

	private int mNumberOfFailedAttemps;

	private List<CSteamID> mBannedLobbies = new List<CSteamID>();

	private MultiplayerManager mMultiplayerManager;

	private static MatchmakingHandler _instance;

	private static ELobbyType mLobbyType;

	public static bool HasTriedJoiningOnline
	{
		get
		{
			return mHasTriedJoiningOnline;
		}
	}

	public bool IsInsideLobby
	{
		get
		{
			return m_Lobby.IsValid() && m_Lobby.IsLobby();
		}
	}

	public bool IsHost
	{
		get
		{
			if (!IsInsideLobby)
			{
				Debug.LogError("You should not call IsHost before is inside lobby!");
				return false;
			}
			return LobbyOwner == SteamUser.GetSteamID();
		}
	}

	public static bool IsNetworkMatch
	{
		get
		{
			return mIsNetworkMatch;
		}
	}

	public CSteamID LobbyOwner
	{
		get
		{
			return SteamMatchmaking.GetLobbyOwner(m_Lobby);
		}
	}

	public CSteamID CurrentLobby
	{
		get
		{
			return m_Lobby;
		}
	}

	public static bool RunningOnSockets { get; private set; }

	public static MatchmakingHandler Instance
	{
		get
		{
			return _instance;
		}
	}

	public static ELobbyType LobbyType
	{
		get
		{
			return mLobbyType;
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
		RunningOnSockets = mRunningOnSockets;
		mMultiplayerManager = UnityEngine.Object.FindObjectOfType<MultiplayerManager>();
		mCurrentDistanceFilter = ELobbyDistanceFilter.k_ELobbyDistanceFilterClose;
	}

	private void OnEnable()
	{
		if (!SteamManager.Initialized)
		{
			Debug.LogError("Hej Brodal, Här skulle det vara ett Steam Error Men nu slipper du det! Istället får du detta, Oh im sorry..");
			return;
		}
		m_LobbyInvite = Callback<LobbyInvite_t>.Create(OnLobbyInvite);
		m_LobbyDataUpdate = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdate);
		m_LobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
		m_LobbyChatMsg = Callback<LobbyChatMsg_t>.Create(OnLobbyChatMsg);
		m_LobbyGameCreated = Callback<LobbyGameCreated_t>.Create(OnLobbyGameCreated);
		m_LobbyJoinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnLobbyJoinRequest);
		OnLobbyEnterCallResult = CallResult<LobbyEnter_t>.Create(OnLobbyEnter);
		OnLobbyMatchListCallResult = CallResult<LobbyMatchList_t>.Create();
		OnLobbyCreatedCallResult = CallResult<LobbyCreated_t>.Create();
	}

	public void CreateSteamLobby(int maxPlayers, bool privateLobby)
	{
		if (RunningOnSockets)
		{
			if (UnityEngine.Object.FindObjectOfType<MatchMakingHandlerSockets>().HostServer())
			{
				mMultiplayerManager.OnSocketServerCreated();
			}
			return;
		}
		Debug.Log("Creating steam lobby... Trying");
		if (!SteamManager.Initialized)
		{
			UnityEngine.Object.FindObjectOfType<LoadingScreenManager>().LoadThenFail(ConnectionErrorType.SteamNotInit, string.Empty);
			return;
		}
		RunningOnSockets = false;
		mLobbyType = (privateLobby ? ELobbyType.k_ELobbyTypeFriendsOnly : ELobbyType.k_ELobbyTypePublic);
		if (mLobbyType == ELobbyType.k_ELobbyTypePublic)
		{
			OptionsHolder.DefaultSettings();
		}
		SteamAPICall_t hAPICall = SteamMatchmaking.CreateLobby(mLobbyType, maxPlayers);
		OnLobbyCreatedCallResult.Set(hAPICall, mMultiplayerManager.OnServerCreated);
	}

	public void OnRandomServerJoinFailed()
	{
		ELobbyDistanceFilter eLobbyDistanceFilter = ELobbyDistanceFilter.k_ELobbyDistanceFilterDefault;
		if (mCurrentDistanceFilter < eLobbyDistanceFilter)
		{
			Debug.Log("Did not find a match for distance filter: " + mCurrentDistanceFilter.ToString() + " Increasing Distance!");
			mCurrentDistanceFilter++;
			JoinRandomServer();
		}
		else
		{
			mCurrentDistanceFilter = ELobbyDistanceFilter.k_ELobbyDistanceFilterClose;
			Debug.Log("Tried to join random server, no found! Creating One!");
			CreateSteamLobby(4, false);
		}
	}

	public bool Disconnect(bool showScreen = true)
	{
		if (!IsInsideLobby)
		{
			return false;
		}
		Debug.Log("Disconnecting From Lobby: " + m_Lobby.ToString());
		UnityEngine.Object.FindObjectOfType<MultiplayerManager>().OnDisconnected();
		SteamMatchmaking.LeaveLobby(m_Lobby);
		m_Lobby = (CSteamID)0uL;
		if (showScreen)
		{
			GameManager.Instance.RestartGame();
			return true;
		}
		return false;
	}

	public void JoinRandomServer()
	{
		if (RunningOnSockets)
		{
			JoinDefaultSocketServer();
			return;
		}
		if (!SteamManager.Initialized)
		{
			UnityEngine.Object.FindObjectOfType<LoadingScreenManager>().LoadThenFail(ConnectionErrorType.SteamNotInit, string.Empty);
			return;
		}
		mCurrentConnectionType = ConnectionType.Quickmatch;
		SteamMatchmaking.AddRequestLobbyListDistanceFilter(mCurrentDistanceFilter);
		Debug.Log("Getting servers for region: " + mCurrentDistanceFilter);
		SteamAPICall_t hAPICall = SteamMatchmaking.RequestLobbyList();
		OnLobbyMatchListCallResult.Set(hAPICall, OnRandomServerJoined);
	}

	private void JoinDefaultSocketServer()
	{
		UnityEngine.Object.FindObjectOfType<MatchMakingHandlerSockets>().JoinServer();
	}

	public void OnRandomServerJoined(LobbyMatchList_t param, bool bIOFailure)
	{
		if (bIOFailure)
		{
			Debug.Log("Biofailure!");
			return;
		}
		uint nLobbiesMatching = param.m_nLobbiesMatching;
		if (nLobbiesMatching == 0)
		{
			OnRandomServerJoinFailed();
			return;
		}
		List<CSteamID> list = new List<CSteamID>();
		for (int i = 0; i < nLobbiesMatching; i++)
		{
			CSteamID lobbyByIndex = SteamMatchmaking.GetLobbyByIndex(i);
			string lobbyData = SteamMatchmaking.GetLobbyData(lobbyByIndex, StickFightConstants.VERSION_KEY);
			if (lobbyData == StickFightConstants.VERSION_VALUE)
			{
				list.Add(lobbyByIndex);
			}
		}
		foreach (CSteamID mBannedLobby in mBannedLobbies)
		{
			Debug.Log("Have Banned Lobbies!");
			if (list.Contains(mBannedLobby))
			{
				list.Remove(mBannedLobby);
				Debug.Log("Found Banned lobby in matchlist, removing it");
			}
		}
		if (list.Count <= 0)
		{
			OnRandomServerJoinFailed();
			return;
		}
		int index = UnityEngine.Random.Range(0, list.Count);
		CSteamID currentLobby = list[index];
		JoinServer(currentLobby, mMultiplayerManager.OnServerJoined);
	}

	public void JoinServer(CSteamID currentLobby, CallResult<LobbyEnter_t>.APIDispatchDelegate functionToCall)
	{
		Disconnect(!IsInsideLobby);
		SteamAPICall_t hAPICall = SteamMatchmaking.JoinLobby(currentLobby);
		OnLobbyEnterCallResult.Set(hAPICall, functionToCall);
		mHasTriedJoiningOnline = true;
	}

	public void OnLobbyCreated(CSteamID currentLobby)
	{
		Debug.Log("Recieved current lobby from MainmenuNetworkhandler!!");
		m_Lobby = currentLobby;
	}

	public string GetValueFromLobby(string pKey)
	{
		return SteamMatchmaking.GetLobbyData(m_Lobby, pKey);
	}

	private void OnLobbyEnter(LobbyEnter_t pCallback, bool bIOFailure)
	{
		Debug.Log("[" + 513 + " - LobbyEnter] -  -- " + pCallback.m_ulSteamIDLobby);
		m_Lobby = (CSteamID)pCallback.m_ulSteamIDLobby;
	}

	private void OnLobbyGameCreated(LobbyGameCreated_t param)
	{
		throw new NotImplementedException();
	}

	private void OnLobbyChatMsg(LobbyChatMsg_t param)
	{
		throw new NotImplementedException();
	}

	private void OnLobbyChatUpdate(LobbyChatUpdate_t param)
	{
		MultiplayerManager multiplayerManager = UnityEngine.Object.FindObjectOfType<MultiplayerManager>();
		CSteamID steamID = new CSteamID(param.m_ulSteamIDMakingChange);
		switch ((EChatMemberStateChange)param.m_rgfChatMemberStateChange)
		{
		case EChatMemberStateChange.k_EChatMemberStateChangeEntered:
			Debug.Log("player Entered The room!");
			multiplayerManager.OnPlayerJoined(steamID);
			break;
		case EChatMemberStateChange.k_EChatMemberStateChangeLeft:
			Debug.Log("player Left The room!");
			multiplayerManager.OnPlayerLeft(steamID);
			break;
		case EChatMemberStateChange.k_EChatMemberStateChangeDisconnected:
			Debug.Log("player Disconnected!");
			multiplayerManager.OnPlayerLeft(steamID);
			break;
		case EChatMemberStateChange.k_EChatMemberStateChangeKicked:
			Debug.Log("player Kicked!");
			multiplayerManager.OnPlayerKicked(steamID);
			break;
		case EChatMemberStateChange.k_EChatMemberStateChangeBanned:
			Debug.Log("player banned!");
			multiplayerManager.OnPlayerLeft(steamID);
			break;
		}
	}

	private void OnLobbyDataUpdate(LobbyDataUpdate_t param)
	{
		Debug.Log("OnLobbyDdataUpdate: ");
	}

	private void OnLobbyEnter(LobbyEnter_t param)
	{
	}

	private void OnLobbyInvite(LobbyInvite_t param)
	{
		throw new NotImplementedException();
	}

	private void OnLobbyJoinRequest(GameLobbyJoinRequested_t param)
	{
		CSteamID steamIDLobby = param.m_steamIDLobby;
		Debug.Log("Trying to join: " + SteamFriends.GetFriendPersonaName(param.m_steamIDFriend));
		OnlineBox onlineBox = UnityEngine.Object.FindObjectOfType<OnlineBox>();
		if ((bool)onlineBox)
		{
			onlineBox.StartLoading();
		}
		JoinSpecificServer(steamIDLobby);
	}

	private void JoinSpecificServer(CSteamID lobby)
	{
		mCurrentConnectionType = ConnectionType.Specific;
		JoinServer(lobby, mMultiplayerManager.OnServerJoined);
	}

	internal void ClientInitLobbyAndOwner(CSteamID lobby)
	{
		m_Lobby = lobby;
	}

	public bool IsUserInsideMyLobby(CSteamID requestee)
	{
		int lobbyMemberLimit = SteamMatchmaking.GetLobbyMemberLimit(m_Lobby);
		for (int i = 0; i < lobbyMemberLimit; i++)
		{
			CSteamID lobbyMemberByIndex = SteamMatchmaking.GetLobbyMemberByIndex(m_Lobby, i);
			if (lobbyMemberByIndex == requestee)
			{
				return true;
			}
		}
		return false;
	}

	public void SetLobbyJoinable()
	{
		SteamMatchmaking.SetLobbyJoinable(m_Lobby, true);
	}

	public static void SetNewLobbyType(ELobbyType lobbyType)
	{
		mLobbyType = lobbyType;
		if (mLobbyType == ELobbyType.k_ELobbyTypePublic)
		{
			OptionsHolder.DefaultSettings();
		}
	}

	public static void SetNetworkMatch(bool v)
	{
		mIsNetworkMatch = v;
	}

	internal void ChangeLobbyType(ELobbyType type)
	{
		if (SteamMatchmaking.SetLobbyType(m_Lobby, type))
		{
			Debug.Log("Set new lobbyType! " + type);
		}
	}

	internal void TryReconnect()
	{
		CSteamID currentLobby = CurrentLobby;
		if (mNumberOfFailedAttemps >= 10 || mCurrentConnectionType == ConnectionType.Specific)
		{
			UnityEngine.Object.FindObjectOfType<LoadingScreenManager>().LoadThenFail(ConnectionErrorType.NoConnectionToHost, string.Empty);
			mNumberOfFailedAttemps = 0;
			mBannedLobbies = new List<CSteamID>();
		}
		else
		{
			Disconnect(false);
			mNumberOfFailedAttemps++;
			Debug.Log("Failed to join, retrying... Attempt: " + mNumberOfFailedAttemps);
			mBannedLobbies.Add(currentLobby);
			JoinRandomServer();
		}
	}
}
