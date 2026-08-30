using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	public static int playersBeingMoved;

	public static bool inFight;

	public static bool stillInMenu = true;

	public static bool spawnWeaponsAsPresents = true;

	public CodeStateAnimation mapAnimation;

	public AnimationCurve movePlayerCurve;

	public float movePlayerTime = 1f;

	private ControllerHandler controllerHandler;

	private List<Rigidbody> mSpawnedWeapons = new List<Rigidbody>();

	public List<Controller> playersAlive = new List<Controller>();

	public MultiplayerManager mMultiplayerManager;

	public P2PPackageHandler P2PPackageHandler;

	private MapInfo[] maps;

	public MapInfo currentMapInfo;

	public MapInfo oldMap;

	public GameObject mapHolder;

	public MapWrapper lastMapNumber;

	public GameObject[] weapons;

	public GameObject[] weaponsT2;

	public GameObject[] weaponsT3;

	public GameObject[] weaponSpeciaTier;

	public GameObject[] GodWeapons;

	public GameObject[] poolRanged;

	public GameObject[] poolMeele;

	public GameObject[] poolSniper;

	public GameObject[] poolHandguns;

	public GameObject[] poolExplosive;

	public GameObject[] poolSnakes;

	public GameObject[] poolLava;

	public TextMeshProUGUI winText;

	private float randomWeaponCounter = 99f;

	private AudioSource au;

	public AudioClip[] winClips;

	public bool isLoading;

	private bool isLoadingInternal;

	private bool dontSpawnItems;

	private float secondsBeforeSuddendeath;

	public float matchTime;

	private HoardHandler hoardHandler;

	private Crown crown;

	private Vicotory vicotory;

	public bool testing;

	private bool loadSuccessful = true;

	private static GameManager _instance;

	public int numberOfMaps;

	private CountDown mCountDownHandler;

	[SerializeField]
	private CodeStateAnimation mWaitTextAnimator;

	private LevelSelection levelSelector;

	private OnlineRoom onlineRoom;

	private WeaponSelectionHandler m_WeaponSelectionHandler;

	private MultiplayerManager mNetworkManager;

	public TMP_InputField chatInputField;

	public GameObject[] enableOnStart;

	public GameObject[] disableOnStart;

	[SerializeField]
	private CustomMapInfoSubscriberHandler m_CustomMapInfoHandler;

	private bool spawnedLastWeaponOnLeftSide;

	private float extraSpawnWeaponTime;

	public Action OnMatchEnded;

	public GameObject GameCanvas;

	private static AnalytcisTrigger m_AnalyticsTrigger;

	private List<CharacterActions> mSavedDevicesForNetwork = new List<CharacterActions>();

	[SerializeField]
	private AspectFix m_CameraAspectFix;

	[SerializeField]
	private Transform m_Bars;

	public float LastAppliedScale = 1f;

	private bool mPlayingCountdown;

	public List<Rigidbody> SpawnedWeapons
	{
		get
		{
			return mSpawnedWeapons;
		}
	}

	public static GameManager Instance
	{
		get
		{
			return _instance;
		}
	}

	public Controller LastWinner { get; private set; }

	public List<CharacterActions> SavedDevicesForNetwork
	{
		get
		{
			return mSavedDevicesForNetwork;
		}
	}

	private void Awake()
	{
		Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
		Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
		Debug.Log("StickVer " + StickFightConstants.VERSION_VALUE);
		numberOfMaps = Application.levelCount - 2;
		levelSelector = GetComponent<LevelSelection>();
		onlineRoom = GetComponent<OnlineRoom>();
		vicotory = GetComponent<Vicotory>();
		mCountDownHandler = UnityEngine.Object.FindObjectOfType<CountDown>();
		if (_instance != null && _instance != this)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else
		{
			_instance = this;
		}
		m_WeaponSelectionHandler = UnityEngine.Object.FindObjectOfType<WeaponSelectionHandler>();
		mNetworkManager = UnityEngine.Object.FindObjectOfType<MultiplayerManager>();
		lastMapNumber = new MapWrapper
		{
			MapType = 0,
			MapData = BitConverter.GetBytes(0)
		};
	}

	private void Start()
	{
		controllerHandler = GetComponent<ControllerHandler>();
		winText = GetComponentInChildren<WinText>(true).GetComponent<TextMeshProUGUI>();
		au = GetComponentInChildren<AudioSource>();
		hoardHandler = UnityEngine.Object.FindObjectOfType<HoardHandler>();
		crown = UnityEngine.Object.FindObjectOfType<Crown>();
		InitAnalytics();
	}

	private void InitAnalytics()
	{
		if (SteamManager.Initialized)
		{
			Analytics.SetUserId(SteamUser.GetSteamID().m_SteamID.ToString());
		}
		m_AnalyticsTrigger = AnalytcisTrigger.Instance;
	}

	private void OnGUI()
	{
	}

	private void Update()
	{
		Cursor.lockState = CursorLockMode.Confined;
		if (inFight)
		{
			randomWeaponCounter -= Time.deltaTime;
		}
		if (randomWeaponCounter < 0f)
		{
			SpawnRandomWeapon();
		}
		if (inFight && !stillInMenu)
		{
			matchTime += Time.deltaTime;
		}
		else
		{
			matchTime = 0f;
		}
		hoardHandler.specificNumber = (int)Mathf.Clamp((matchTime - secondsBeforeSuddendeath) / 10f, 0f, OptionsHolder.bots);
	}

	public MapWrapper GetCurrentMap()
	{
		return lastMapNumber;
	}

	public void RestartGame()
	{
		if (MatchmakingHandler.IsNetworkMatch)
		{
			MatchmakingHandler.Instance.Disconnect(false);
		}
		Application.LoadLevel(Application.loadedLevel);
		stillInMenu = true;
		PauseManager.isPaused = false;
		inFight = false;
		playersBeingMoved = 0;
		Time.timeScale = 1f;
		TimeHandler.managerTime = 1f;
		TimeHandler.pauseTime = 1f;
		DisableIfPlayed.hasPlayed = true;
	}

	private void SpawnRandomWeapon()
	{
		if (dontSpawnItems || OptionsHolder.weaponsSpawn == 2 || (MatchmakingHandler.IsNetworkMatch && !MultiplayerManager.IsServer))
		{
			return;
		}
		if (OptionsHolder.weaponsSpawn == 1)
		{
			randomWeaponCounter = UnityEngine.Random.Range(3f, 5f);
		}
		if (OptionsHolder.weaponsSpawn == 0)
		{
			randomWeaponCounter = UnityEngine.Random.Range(5f, 8f);
		}
		if (OptionsHolder.weaponsSpawn == 3)
		{
			randomWeaponCounter = UnityEngine.Random.Range(8f, 12f);
		}
		randomWeaponCounter += extraSpawnWeaponTime;
		float num = UnityEngine.Random.Range(0f, 8f);
		if (spawnedLastWeaponOnLeftSide)
		{
			num *= -1f;
		}
		spawnedLastWeaponOnLeftSide = !spawnedLastWeaponOnLeftSide;
		float num2 = 11f;
		if ((bool)Instance)
		{
			num2 *= Instance.LastAppliedScale;
		}
		Vector3 vector = Vector3.up * num2 + Vector3.forward * num;
		GameObject weaponObject;
		int randomWeaponIndex = m_WeaponSelectionHandler.GetRandomWeaponIndex(true, out weaponObject);
		if (randomWeaponIndex < 0)
		{
			return;
		}
		bool flag = false;
		if (lastMapNumber.MapType == 0)
		{
			int num3 = BitConverter.ToInt32(lastMapNumber.MapData, 0);
			flag = num3 >= 104 && num3 <= 124;
		}
		if (MatchmakingHandler.IsNetworkMatch)
		{
			mNetworkManager.SpawnWeapon(randomWeaponIndex, vector, flag);
		}
		else
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(weaponObject, vector, Quaternion.identity);
			if (flag)
			{
				gameObject.GetComponent<WeaponPickUp>().ChangeToPresent();
			}
			mSpawnedWeapons.Add(gameObject.GetComponent<Rigidbody>());
		}
		if ((double)UnityEngine.Random.value > 0.9)
		{
			SpawnRandomWeapon();
		}
	}

	private void DissarmPlayers()
	{
		foreach (Controller player in controllerHandler.players)
		{
			if (!(player == null))
			{
				player.GetComponent<Fighting>().Dissarm();
				DragHandler[] componentsInChildren = player.GetComponentsInChildren<DragHandler>();
				foreach (DragHandler dragHandler in componentsInChildren)
				{
					dragHandler.extraDrag = 0f;
				}
			}
		}
	}

	public void RevivePlayer(Controller playerToRevive, bool newMap = true)
	{
		if (playerToRevive == null)
		{
			Debug.LogWarning("Trying to revive null player");
			return;
		}
		int count = playersAlive.Count;
		bool flag = false;
		for (int i = 0; i < count; i++)
		{
			if (playersAlive[i] == null)
			{
				playersAlive[i] = playerToRevive;
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			playersAlive.Add(playerToRevive);
		}
		bool flag2 = false;
		HealthHandler component = playerToRevive.GetComponent<HealthHandler>();
		if (component == null)
		{
			Debug.LogWarning("Trying to revive player without HealthHandler");
			return;
		}
		component.health = 100f;
		component.sinceSpawn = 0f;
		if (component.FirstDeathFlag)
		{
			flag2 = true;
			if (newMap)
			{
				if (component.DiedWithinSeconds(1f))
				{
					playerToRevive.GetComponent<CharacterInformation>().isDead = true;
				}
				else
				{
					component.FirstDeathFlag = false;
				}
			}
		}
		else
		{
			playerToRevive.GetComponent<CharacterInformation>().isDead = false;
			playerToRevive.gameObject.SetActive(true);
		}
		playerToRevive.GetComponent<CharacterInformation>().sinceFallen = 2f;
		playerToRevive.GetComponent<GrabHandler>().EndGrab();
		playerToRevive.damager = null;
		if (MatchmakingHandler.IsNetworkMatch)
		{
			playerToRevive.GetComponent<NetworkPlayer>().SetActive(true);
		}
		if (flag2)
		{
			playersAlive.Remove(playerToRevive);
		}
		WinCounterUI winCounterUI = UnityEngine.Object.FindObjectOfType<WinCounterUI>();
		if (winCounterUI != null)
		{
			winCounterUI.RefreshWinTexts();
		}
	}

	public void ReviveAllPlayers(bool newMap = true)
	{
		playersAlive.Clear();
		foreach (Controller player in controllerHandler.players)
		{
			RevivePlayer(player, newMap);
		}
	}

	public void RemovePlayer(Controller player)
	{
		playersAlive.Remove(player);
	}

	public void KillPlayer(Controller playerToKill)
	{
		if (playersAlive.Contains(playerToKill))
		{
			playersAlive.Remove(playerToKill);
		}
		if (playerToKill.damager != null && !playerToKill.damager.isAI)
		{
			if (crown.crownBarrer == playerToKill)
			{
				crown.SetNewKing(playerToKill.damager, false);
				playerToKill.damager.GetComponent<CharacterStats>().crownSteals++;
			}
			playerToKill.damager.OnKilledEnemy(playerToKill);
			playerToKill.damager.GetComponent<CharacterStats>().kills++;
		}
		else
		{
			playerToKill.GetComponent<CharacterStats>().suicides++;
		}
		int num = 0;
		Controller controller = null;
		foreach (Controller item in playersAlive)
		{
			if (item != null && !item.GetComponent<CharacterInformation>().isDead)
			{
				controller = item;
				num++;
			}
		}
		if (num <= 1)
		{
			if (MatchmakingHandler.IsNetworkMatch)
			{
				if (MultiplayerManager.IsServer)
				{
					if (stillInMenu && mNetworkManager.GetPlayersInLobby() < 2)
					{
						return;
					}
					MapWrapper nextLevel = levelSelector.GetNextLevel();
					byte indexOfWinner = ((num != 0) ? ((byte)controller.GetComponent<NetworkPlayer>().NetworkSpawnID) : byte.MaxValue);
					LastWinner = controller;
					mNetworkManager.ChangeMap(nextLevel, indexOfWinner);
					bool customMap = lastMapNumber.MapType == 2;
					m_AnalyticsTrigger.OnMatchEnd(true, customMap);
				}
			}
			else
			{
				AllButOnePlayersDied();
			}
		}
		playerToKill.OnDeath();
		playerToKill.GetComponent<CharacterStats>().deaths++;
	}

	public GameObject FindWeaponByIndex(int weaponIndex)
	{
		int num = ((weapons != null) ? weapons.Length : 0);
		for (int i = 0; i < num; i++)
		{
			GameObject gameObject = weapons[i];
			if (!gameObject)
			{
				continue;
			}
			string text = gameObject.name.Split('(')[0].Replace("Gun", string.Empty);
			int result;
			if (int.TryParse(text, out result))
			{
				if (weaponIndex == result)
				{
					return weapons[i];
				}
			}
			else
			{
				Debug.LogError("Could not parse weaponNumber: " + text + " Index: " + weaponIndex);
			}
		}
		num = ((weaponsT2 != null) ? weaponsT2.Length : 0);
		for (int j = 0; j < num; j++)
		{
			GameObject gameObject = weaponsT2[j];
			if (!gameObject)
			{
				continue;
			}
			string text = gameObject.name.Split('(')[0].Replace("Gun", string.Empty);
			int result2;
			if (int.TryParse(text, out result2))
			{
				if (weaponIndex == result2)
				{
					return weaponsT2[j];
				}
			}
			else
			{
				Debug.LogError("Could not parse weaponNumber: " + text + " Index: " + weaponIndex);
			}
		}
		num = ((weaponsT3 != null) ? weaponsT3.Length : 0);
		for (int k = 0; k < num; k++)
		{
			GameObject gameObject = weaponsT3[k];
			if (!gameObject)
			{
				continue;
			}
			string text = gameObject.name.Split('(')[0].Replace("Gun", string.Empty);
			int result3;
			if (int.TryParse(text, out result3))
			{
				if (weaponIndex == result3)
				{
					return weaponsT3[k];
				}
			}
			else
			{
				Debug.LogError("Could not parse weaponNumber: " + text + " Index: " + weaponIndex);
			}
		}
		num = ((weaponSpeciaTier != null) ? weaponSpeciaTier.Length : 0);
		for (int l = 0; l < num; l++)
		{
			GameObject gameObject = weaponSpeciaTier[l];
			if (!gameObject)
			{
				continue;
			}
			string text = gameObject.name.Split('(')[0].Replace("Gun", string.Empty);
			int result4;
			if (int.TryParse(text, out result4))
			{
				if (weaponIndex == result4)
				{
					return weaponSpeciaTier[l];
				}
			}
			else
			{
				Debug.LogError("Could not parse weaponNumber: " + text + " Index: " + weaponIndex);
			}
		}
		throw new Exception("Could not find weapon with index: " + weaponIndex);
	}

	public int GetPlayersAlive()
	{
		return playersAlive.Count;
	}

	public int KillAllPlayers(bool network)
	{
		int num = 0;
		if (mSavedDevicesForNetwork.Count == 0)
		{
			foreach (Controller item in playersAlive)
			{
				if (item.HasControl)
				{
					mSavedDevicesForNetwork.Add(item.PlayerActions);
				}
				UnityEngine.Object.Destroy(item.gameObject);
				num++;
			}
		}
		playersAlive.Clear();
		return num;
	}

	public void KillAllPlayers(List<Controller> playersToCheck)
	{
		playersAlive.Clear();
		if (playersToCheck == null)
		{
			return;
		}
		foreach (Controller item in playersToCheck)
		{
			if (!(item == null) && item.HasControl)
			{
				playersAlive.Add(item);
			}
		}
	}

	private void AllButOnePlayersDied()
	{
		mSpawnedWeapons.Clear();
		onlineRoom.hasStarted = true;
		Controller controller = null;
		foreach (Controller item in playersAlive)
		{
			if (item != null)
			{
				controller = item;
				break;
			}
		}
		if (controller == null)
		{
			Debug.LogWarning("Ending match with no player alive");
		}
		if (controller != null)
		{
			winText.color = controller.gameObject.GetComponentInChildren<SpriteRenderer>().GetComponent<SetColorWhenDamaged>().startColor;
		}
		winText.text = vicotory.GetRandomWinText();
		winText.gameObject.SetActive(true);
		au.PlayOneShot(winClips[UnityEngine.Random.Range(0, winClips.Length)]);
		if (stillInMenu)
		{
			EnableObjects();
		}
		if (controller != null)
		{
			LastWinner = controller;
		}
		StartMatch(levelSelector.GetNextLevel());
		if (controller != null)
		{
			crown.SetNewKing(controller, true);
		}
		bool customMap = lastMapNumber.MapType == 2 || lastMapNumber.MapType == 1;
		m_AnalyticsTrigger.OnMatchEnd(false, customMap);
	}

	private void EnableObjects()
	{
		GameObject[] array = enableOnStart;
		foreach (GameObject gameObject in array)
		{
			if ((bool)gameObject)
			{
				gameObject.SetActive(true);
			}
		}
		GameObject[] array2 = disableOnStart;
		foreach (GameObject gameObject2 in array2)
		{
			if ((bool)gameObject2)
			{
				gameObject2.SetActive(false);
			}
		}
	}

	public void StartMatch(MapWrapper mapIndex, bool MovePlayers = true)
	{
		if (inFight && OnMatchEnded != null)
		{
			OnMatchEnded();
		}
		if (LastWinner != null && LastWinner.HasControl)
		{
			int value = SteamStatsAndAchievements.Instance.TransientMemory.Copy<int>("DeathsByFalling").GetValue(0);
			if (value >= ControllerHandler.Instance.ActivePlayers.Count - 1 && ControllerHandler.Instance.ActivePlayers.Count > 1)
			{
				SteamStatsAndAchievements.Instance.UnlockAchievement(SteamStatsAndAchievements.EAchievement.Walkover);
			}
			if (currentMapInfo != null && currentMapInfo.metaData.Contains("IceWorld"))
			{
				DestructiblePiece[] array = UnityEngine.Object.FindObjectsOfType<DestructiblePiece>();
				if (array.Length > 0)
				{
					bool flag = true;
					DestructiblePiece[] array2 = array;
					foreach (DestructiblePiece destructiblePiece in array2)
					{
						Rigidbody component = destructiblePiece.GetComponent<Rigidbody>();
						if ((bool)component && component.isKinematic)
						{
							flag = false;
							break;
						}
					}
					if (flag)
					{
						SteamStatsAndAchievements.Instance.UnlockAchievement(SteamStatsAndAchievements.EAchievement.IceAge);
					}
				}
			}
		}
		if (LastWinner != null)
		{
			LastWinner.GetComponent<CharacterStats>().wins++;
			WinCounterUI winCounterUI = UnityEngine.Object.FindObjectOfType<WinCounterUI>();
			winCounterUI.IncrementWinCounter(LastWinner);
		}
		EnableObjects();
		inFight = false;
		if (OptionsHolder.weaponsSpawn == 1)
		{
			randomWeaponCounter = UnityEngine.Random.Range(0.5f, 1f);
		}
		if (OptionsHolder.weaponsSpawn == 0)
		{
			randomWeaponCounter = UnityEngine.Random.Range(2f, 4f);
		}
		if (OptionsHolder.weaponsSpawn == 3)
		{
			randomWeaponCounter = UnityEngine.Random.Range(4f, 8f);
		}
		randomWeaponCounter += extraSpawnWeaponTime;
		secondsBeforeSuddendeath = 25f;
		StartCoroutine(StartMapSequence(mapIndex, MovePlayers));
		if (MatchmakingHandler.IsNetworkMatch)
		{
			m_CustomMapInfoHandler.ShowSubscribe();
		}
	}

	public void LoadMapCourotine(MapWrapper map)
	{
		StartCoroutine(LoadMap(map));
	}

	private IEnumerator StartMapSequence(MapWrapper mapIndex, bool MovePlayers = true)
	{
		Debug.Log("StartMapSequence0");
		while (TimeHandler.managerTime > 0.01f)
		{
			TimeHandler.managerTime = Mathf.Lerp(TimeHandler.managerTime, 0f, Time.unscaledDeltaTime * 3f);
			yield return null;
		}
		SteamStatsAndAchievements.Instance.CleanUpAndStoreStats();
		Rigidbody[] rigs = UnityEngine.Object.FindObjectsOfType<Rigidbody>();
		Rigidbody[] array = rigs;
		foreach (Rigidbody rigidbody in array)
		{
			if (rigidbody.gameObject.scene.name != "MainScene")
			{
				rigidbody.isKinematic = true;
			}
		}
		foreach (Controller player in controllerHandler.players)
		{
			if (!(player == null))
			{
				player.OnUnloadMap();
				Rigidbody[] componentsInChildren = player.transform.root.GetComponentsInChildren<Rigidbody>(true);
				foreach (Rigidbody rigidbody2 in componentsInChildren)
				{
					rigidbody2.isKinematic = true;
				}
			}
		}
		TimeHandler.managerTime = 1f;
		RemoveOnLevelChange[] array2 = UnityEngine.Object.FindObjectsOfType<RemoveOnLevelChange>();
		foreach (RemoveOnLevelChange removeOnLevelChange in array2)
		{
			removeOnLevelChange.gameObject.AddComponent<ShrinkOverTime>();
		}
		Collider[] componentsInChildren2 = GetComponentsInChildren<Collider>();
		foreach (Collider collider in componentsInChildren2)
		{
			collider.enabled = false;
		}
		isLoading = true;
		if (!levelSelector.IsDownloadingMaps() && !levelSelector.LastMapNeededDownloading())
		{
			LoadMapCourotine(mapIndex);
		}
		Debug.Log("isLoading0");
		yield return new WaitWhile(() => isLoading);
		yield return null;
		Debug.Log("isLoading1");
		if (!loadSuccessful)
		{
			Debug.LogWarning("StartMapSequence load failed");
			yield break;
		}
		if (MatchmakingHandler.IsNetworkMatch)
		{
			MultiplayerManager multiplayerManager = UnityEngine.Object.FindObjectOfType<MultiplayerManager>();
			multiplayerManager.CheckForGroundWeapons();
			multiplayerManager.UpdateUI();
		}
		for (byte b = 0; b < controllerHandler.players.Count; b++)
		{
			Controller controller = controllerHandler.players[b];
			if (!(controller == null))
			{
				new Vector3(0f, 12f, 0f);
				Vector3 targetPosition;
				if (MatchmakingHandler.IsNetworkMatch && controller.HasControl && !MovePlayers)
				{
					targetPosition = ((!controller.GetComponent<HealthHandler>().FirstDeathFlag) ? new Vector3(0f, 12f, 0f) : new Vector3(0f, -100f, 0f));
				}
				else
				{
					int num = b;
					if (b < 0 || b >= currentMapInfo.spawnPoints.Length)
					{
						Debug.LogWarning("Trying to use invalid spawnpoint");
						num = 0;
					}
					targetPosition = currentMapInfo.spawnPoints[num].localPosition;
				}
				StartCoroutine(MovePlayer(controller.GetComponentInChildren<Hip>().GetComponent<Rigidbody>(), targetPosition));
			}
		}
		if (stillInMenu)
		{
			stillInMenu = false;
			UnityEngine.Object.Destroy(base.transform.Find("Map").gameObject);
			Rigidbody[] componentsInChildren3 = GetComponentsInChildren<Rigidbody>();
			foreach (Rigidbody rigidbody3 in componentsInChildren3)
			{
				rigidbody3.gameObject.AddComponent<ConstantForce>().force = Vector3.forward * 1000000f;
				rigidbody3.gameObject.AddComponent<RemoveAfterSeconds>();
			}
		}
		else
		{
			StartCoroutine(RemoveMap(lastMapNumber));
		}
		m_CustomMapInfoHandler.HideSubscribe();
		m_CustomMapInfoHandler.AssignNewMap(mapIndex);
		yield return new WaitForSecondsRealtime(1.1f);
		mapAnimation.state1 = false;
		if (currentMapInfo != null)
		{
			currentMapInfo.dontFollowTheSwoosher = false;
			currentMapInfo.gameObject.SetActive(true);
			StartCoroutine(PrepareMapForTravel(currentMapInfo.gameObject, true));
		}
		else
		{
			Debug.LogError("Missing currentMapInfo");
		}
		lastMapNumber = mapIndex;
		Debug.Log("StartMapSequence1");
	}

	private void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
	{
		Debug.Log("OnLevelFinishedLoading");
		isLoadingInternal = false;
	}

	private void OnMapSizeChanged(float newSize)
	{
		m_CameraAspectFix.SetMapSize(newSize);
		float num = newSize / 10f;
		BoxCollider[] array = UnityEngine.Object.FindObjectsOfType<BoxCollider>();
		foreach (BoxCollider boxCollider in array)
		{
			if (boxCollider.name == "Death")
			{
				boxCollider.transform.root.localScale = new Vector3(1f, num, num);
				break;
			}
		}
		BarsHandler[] array2 = UnityEngine.Object.FindObjectsOfType<BarsHandler>();
		foreach (BarsHandler barsHandler in array2)
		{
			barsHandler.transform.localScale = new Vector3(1f, num, num);
		}
		BackGround[] array3 = UnityEngine.Object.FindObjectsOfType<BackGround>();
		foreach (BackGround backGround in array3)
		{
			backGround.transform.localScale = backGround.StartScale * num;
		}
		winText.gameObject.SetActive(false);
		Vector3 localScale = GameCanvas.transform.localScale;
		Vector3 localScale2 = new Vector3(localScale.x / LastAppliedScale * num, localScale.y / LastAppliedScale * num, localScale.z);
		GameCanvas.transform.localScale = localScale2;
		LastAppliedScale = num;
	}

	private IEnumerator LoadMap(MapWrapper mapIndex)
	{
		loadSuccessful = true;
		isLoadingInternal = true;
		if (mapIndex.MapType != 0)
		{
			bool flag = levelSelector.LoadCustomLevel(mapIndex, OnMapSizeChanged);
			isLoadingInternal = false;
			if (!flag)
			{
				loadSuccessful = false;
				UnityEngine.Object.FindObjectOfType<LoadingScreenManager>().LoadThenFail(ConnectionErrorType.DownloadFailure, string.Empty);
				yield break;
			}
			Debug.Log("Loaded custom map: " + mapIndex);
		}
		else
		{
			int num;
			using (MemoryStream input = new MemoryStream(mapIndex.MapData))
			{
				using (BinaryReader binaryReader = new BinaryReader(input))
				{
					num = binaryReader.ReadInt32();
				}
			}
			Debug.Log("Loading Scene: " + SceneManager.GetSceneByBuildIndex(num).name);
			SceneManager.sceneLoaded += OnLevelFinishedLoading;
			SceneManager.LoadScene(num, LoadSceneMode.Additive);
			OnMapSizeChanged(10f);
		}
		if ((bool)currentMapInfo)
		{
			oldMap = currentMapInfo;
		}
		Debug.Log("LoadMapWait0");
		yield return new WaitWhile(() => isLoadingInternal);
		yield return null;
		Debug.Log("LoadMapWait1");
		MapInfo[] array = UnityEngine.Object.FindObjectsOfType<MapInfo>();
		foreach (MapInfo mapInfo in array)
		{
			if (!oldMap || mapInfo != oldMap)
			{
				currentMapInfo = mapInfo;
				BackGround componentInChildren = mapInfo.GetComponentInChildren<BackGround>();
				if ((bool)componentInChildren)
				{
					componentInChildren.enabled = true;
				}
				mapInfo.gameObject.SetActive(false);
			}
		}
		if (currentMapInfo != null)
		{
			dontSpawnItems = currentMapInfo.dontSpawnItems;
			extraSpawnWeaponTime = currentMapInfo.extraWeaponSpawnTime;
		}
		isLoading = false;
	}

	private IEnumerator RemoveMap(MapWrapper mapIndex)
	{
		mapAnimation.state1 = true;
		StartCoroutine(PrepareMapForTravel(oldMap.gameObject, false));
		BackGround bg = oldMap.GetComponentInChildren<BackGround>();
		if ((bool)bg)
		{
			bg.FadeOut();
		}
		yield return new WaitForSecondsRealtime(1f);
		Debug.Log("Trying to unload scene nr: " + mapIndex);
		if (mapIndex.MapType == 0)
		{
			int sceneBuildIndex = BitConverter.ToInt32(mapIndex.MapData, 0);
			SceneManager.UnloadScene(sceneBuildIndex);
		}
		else
		{
			UnityEngine.Object.Destroy(oldMap.gameObject);
		}
	}

	private IEnumerator PrepareMapForTravel(GameObject map, bool comingIn)
	{
		Rigidbody[] componentsInChildren = map.gameObject.GetComponentsInChildren<Rigidbody>();
		foreach (Rigidbody rigidbody in componentsInChildren)
		{
			rigidbody.isKinematic = true;
		}
		ConfigurableJoint[] componentsInChildren2 = map.gameObject.GetComponentsInChildren<ConfigurableJoint>(true);
		foreach (ConfigurableJoint configurableJoint in componentsInChildren2)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(configurableJoint.gameObject, configurableJoint.transform.position, configurableJoint.transform.rotation);
			gameObject.AddComponent<JointDummy>();
			gameObject.transform.parent = configurableJoint.transform.parent;
			ConfigurableJoint[] components = gameObject.GetComponents<ConfigurableJoint>();
			foreach (ConfigurableJoint obj in components)
			{
				UnityEngine.Object.Destroy(obj);
			}
			ConstantForce[] components2 = gameObject.GetComponents<ConstantForce>();
			foreach (ConstantForce obj2 in components2)
			{
				UnityEngine.Object.Destroy(obj2);
			}
			UnityEngine.Object.Destroy(gameObject.GetComponent<Rigidbody>());
			configurableJoint.gameObject.SetActive(false);
		}
		yield return new WaitForSecondsRealtime(1f);
		while (mapAnimation.isAnimating)
		{
			yield return null;
		}
		JointDummy[] array = UnityEngine.Object.FindObjectsOfType<JointDummy>();
		foreach (JointDummy jointDummy in array)
		{
			UnityEngine.Object.Destroy(jointDummy.gameObject);
		}
		if ((bool)map)
		{
			ConfigurableJoint[] componentsInChildren3 = map.gameObject.GetComponentsInChildren<ConfigurableJoint>(true);
			foreach (ConfigurableJoint configurableJoint2 in componentsInChildren3)
			{
				configurableJoint2.gameObject.SetActive(true);
			}
		}
		if ((bool)map)
		{
			Rigidbody[] componentsInChildren4 = map.gameObject.GetComponentsInChildren<Rigidbody>();
			foreach (Rigidbody rigidbody2 in componentsInChildren4)
			{
				DestructiblePiece component = rigidbody2.GetComponent<DestructiblePiece>();
				if ((!component || component.simpleDestruction || component.eventDestruction) && !rigidbody2.GetComponent<DontEnableRig>())
				{
					rigidbody2.isKinematic = false;
				}
			}
		}
		if (MatchmakingHandler.IsNetworkMatch)
		{
			P2PStatistics.StartRecording();
		}
		if (!comingIn)
		{
			yield break;
		}
		if (MatchmakingHandler.IsNetworkMatch)
		{
			mMultiplayerManager.InitMapDataObjects();
			TimeHandler.managerTime = 0f;
			mMultiplayerManager.ReadyUp();
			yield return new WaitForSecondsRealtime(0.5f);
			mMultiplayerManager.InitSyncedObjects();
			StartCoroutine(StartCountdownAfterSeconds(MultiplayerManager.k_MAX_SECONDS_UNTIL_AUTO_START));
			{
				foreach (Controller player in controllerHandler.players)
				{
					if (!(player == null))
					{
						Rigidbody[] componentsInChildren5 = player.transform.root.GetComponentsInChildren<Rigidbody>(true);
						foreach (Rigidbody rigidbody3 in componentsInChildren5)
						{
							rigidbody3.isKinematic = false;
						}
					}
				}
				yield break;
			}
		}
		yield return new WaitForSecondsRealtime(1f);
		m_CustomMapInfoHandler.HideMapInfo();
	}

	private IEnumerator StartCountdownAfterSeconds(float seconds)
	{
		float timer = 0f;
		bool play = true;
		while (timer < seconds)
		{
			timer += Time.unscaledDeltaTime;
			if (inFight)
			{
				play = false;
				mWaitTextAnimator.state1 = false;
				break;
			}
			if (timer >= 1f && !mPlayingCountdown)
			{
				mWaitTextAnimator.state1 = true;
			}
			yield return 0;
		}
		if (play)
		{
			StartCountDown();
		}
	}

	public void StartCountDown()
	{
		mWaitTextAnimator.state1 = false;
		StartCoroutine(CountDownCoroutine());
	}

	private IEnumerator CountDownCoroutine()
	{
		if (!mPlayingCountdown)
		{
			m_CustomMapInfoHandler.HideMapInfo();
			mPlayingCountdown = true;
			TimeHandler.managerTime = 0f;
			mCountDownHandler.Countdown();
			yield return new WaitForSecondsRealtime(1f);
			inFight = true;
			TimeHandler.managerTime = 1f;
			mPlayingCountdown = false;
		}
	}

	private IEnumerator MovePlayer(Rigidbody player, Vector3 targetPosition)
	{
		if (!player)
		{
			yield break;
		}
		playersBeingMoved++;
		Rigidbody[] componentsInChildren = player.transform.root.GetComponentsInChildren<Rigidbody>();
		foreach (Rigidbody rigidbody in componentsInChildren)
		{
			rigidbody.isKinematic = true;
		}
		player.transform.root.GetComponent<SetMovementAbility>().Reset();
		DissarmPlayers();
		player.transform.root.GetComponent<Controller>().SetCollision(false);
		Vector3 movementDirection = targetPosition + Vector3.up * 1f - player.position;
		float t = 0f;
		float lastStep = 0f;
		while (t < movePlayerTime)
		{
			if (!player)
			{
				yield break;
			}
			t += Time.deltaTime;
			float newPosition = movePlayerCurve.Evaluate(t / movePlayerTime);
			float oldPosition = movePlayerCurve.Evaluate(lastStep / movePlayerTime);
			float deltaPosition = newPosition - oldPosition;
			Rigidbody[] componentsInChildren2 = player.transform.root.GetComponentsInChildren<Rigidbody>();
			foreach (Rigidbody rigidbody2 in componentsInChildren2)
			{
				rigidbody2.transform.position += deltaPosition * movementDirection;
			}
			lastStep = t;
			yield return null;
		}
		while (mapAnimation.isAnimating)
		{
			if (!player)
			{
				yield break;
			}
			Vector3 remainingDistance = targetPosition + Vector3.up * 1f - player.transform.position;
			Rigidbody[] componentsInChildren3 = player.transform.root.GetComponentsInChildren<Rigidbody>();
			foreach (Rigidbody rigidbody3 in componentsInChildren3)
			{
				if ((bool)rigidbody3)
				{
					rigidbody3.transform.position += remainingDistance;
				}
			}
			yield return null;
		}
		if ((bool)player)
		{
			if (!MatchmakingHandler.IsNetworkMatch)
			{
				Rigidbody[] componentsInChildren4 = player.transform.root.GetComponentsInChildren<Rigidbody>(true);
				foreach (Rigidbody rigidbody4 in componentsInChildren4)
				{
					rigidbody4.isKinematic = false;
				}
			}
			player.transform.root.GetComponent<Controller>().SetCollision(true);
		}
		ReviveAllPlayers();
		if (!MatchmakingHandler.IsNetworkMatch)
		{
			inFight = true;
		}
	}

	public void NetworkAllPlayersDiedButOne(MapWrapper newlevel, byte winner)
	{
		onlineRoom.hasStarted = true;
		P2PStatistics.StopRecording();
		List<Controller> playerControllers = mNetworkManager.PlayerControllers;
		if (winner != byte.MaxValue)
		{
			Controller controller = null;
			if (winner < playerControllers.Count)
			{
				controller = playerControllers[winner];
			}
			if (controller != null)
			{
				crown.SetNewKing(controller, true);
				LastWinner = controller;
				winText.text = vicotory.GetRandomWinText();
				winText.color = controller.GetComponentInChildren<SpriteRenderer>().GetComponent<SetColorWhenDamaged>().startColor;
				winText.gameObject.SetActive(true);
			}
		}
		if ((bool)au)
		{
			au.PlayOneShot(winClips[UnityEngine.Random.Range(0, winClips.Length)]);
		}
		foreach (Controller item in playerControllers)
		{
			if (!(item == null))
			{
				item.GetComponent<NetworkPlayer>().SetActive(false);
			}
		}
		StartMatch(newlevel);
	}

	public byte FindWeaponIdByName(string weaponName)
	{
		int num = weapons.Length;
		for (byte b = 0; b < num; b++)
		{
			string text = weapons[b].name;
			if (weaponName.Split('(')[0].ToLower() == text.ToLower())
			{
				return b;
			}
		}
		num = weaponsT2.Length;
		for (byte b2 = 0; b2 < num; b2++)
		{
			string text = weaponsT2[b2].name;
			if (weaponName.Split('(')[0].ToLower() == text.ToLower())
			{
				return (byte)(b2 + weapons.Length);
			}
		}
		num = weaponsT3.Length;
		for (byte b3 = 0; b3 < num; b3++)
		{
			string text = weaponsT3[b3].name;
			if (weaponName.Split('(')[0].ToLower() == text.ToLower())
			{
				return (byte)(b3 + weaponsT2.Length);
			}
		}
		Debug.LogError("Could not find weapon named: " + weaponName);
		return byte.MaxValue;
	}

	public GameObject GetWeaponWithIndexAndOverFlow(byte weaponID)
	{
		Debug.Log("Trying to find weapon with overflow: " + weaponID);
		int num = weapons.Length;
		int num2 = weapons.Length + weaponsT2.Length;
		int num3 = num2 + weaponsT3.Length;
		int num4 = weaponID;
		GameObject[] array = weapons;
		if (weaponID >= num)
		{
			array = weaponsT2;
			if (weaponID >= num2)
			{
				array = weaponsT3;
				if (weaponID >= num3)
				{
					array = weaponSpeciaTier;
					num4 = weaponID - num3;
					Debug.Log("Found Weapon In Special Tier! Real ID" + num4);
					return array[num4];
				}
				num4 = weaponID - num2;
				Debug.Log("Found Weapon In Tier 3! Real ID" + num4);
				return array[num4];
			}
			num4 = weaponID - num;
			Debug.Log("Found Weapon In Tier 2! Real ID" + num4);
			return array[num4];
		}
		Debug.Log("Found Weapon In Tier 1! Real ID" + num4);
		return array[num4];
	}

	public CharacterActions GetNextSavedDeviceForNetwork()
	{
		if (mSavedDevicesForNetwork == null || mSavedDevicesForNetwork.Count <= 1)
		{
			return GetDefaultBindings();
		}
		CharacterActions characterActions = mSavedDevicesForNetwork[0];
		mSavedDevicesForNetwork[0] = null;
		if (characterActions == null)
		{
			for (int i = 1; i < mSavedDevicesForNetwork.Count; i++)
			{
				characterActions = mSavedDevicesForNetwork[i];
				if (characterActions != null)
				{
					mSavedDevicesForNetwork[i] = null;
					break;
				}
			}
		}
		if (characterActions == null)
		{
			return GetDefaultBindings();
		}
		return characterActions;
	}

	private CharacterActions GetDefaultBindings()
	{
		return CharacterActions.CreateWithAnyBindings();
	}

	public void DisableAllPlayers()
	{
		foreach (Controller item in playersAlive)
		{
			if (item != null)
			{
				item.gameObject.SetActive(false);
			}
		}
	}

	public void SaveDevicesForNextGame(List<Controller> players)
	{
		mSavedDevicesForNetwork.Clear();
		if (players == null)
		{
			return;
		}
		foreach (Controller player in players)
		{
			if (!(player == null) && player.HasControl)
			{
				mSavedDevicesForNetwork.Add(player.PlayerActions);
			}
		}
	}

	public bool IsInLobby()
	{
		MapType mapType = (MapType)lastMapNumber.MapType;
		byte[] mapData = lastMapNumber.MapData;
		return mapType == MapType.Landfall && BitConverter.ToInt32(mapData, 0) == 0;
	}
}
