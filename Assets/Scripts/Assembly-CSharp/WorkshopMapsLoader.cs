using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using LevelEditor;
using Steamworks;
using UnityEngine;

public class WorkshopMapsLoader : MonoBehaviour
{
	public bool m_lastMapNeededDownloading;

	private Callback<DownloadItemResult_t> m_DownloadItemCallResult;

	private CallResult<RemoteStorageSubscribePublishedFileResult_t> m_SubbedCallResult;

	private CallResult<RemoteStorageUnsubscribePublishedFileResult_t> m_UnSubbedCallResult;

	private CallResult<SteamUGCQueryCompleted_t> m_UGCHandleQueryCompleted;

	private const string m_WorkshopPathRelativeToGameDirectory = "/../../../Workshop/Content/674940/";

	private string m_LocalSavePath;

	private string m_WorkshopPath;

	private DirectoryInfo m_WorkshopDirectory;

	private DirectoryInfo m_LocalMapsDirectory;

	private Action m_OnSubscribdedAction;

	private Action m_CallBack;

	private List<PublishedFileId_t> m_RegisteredMaps = new List<PublishedFileId_t>();

	private int m_TotalRegisteredMaps;

	private bool m_IsDownloading;

	private LocalWorkshopWrapper[] m_AllLocalLevels;

	private static WorkshopMapsLoader _instance;

	private bool m_DownloadFailed;

	private List<WorkshopMapWrapper> m_loadedCustomLevels;

	private Action m_OnUnSubbedAction;

	public string WorkshopPath
	{
		get
		{
			return m_WorkshopPath;
		}
	}

	public bool IsDownloading
	{
		get
		{
			return m_IsDownloading;
		}
	}

	public LocalWorkshopWrapper[] AllLocalLevels
	{
		get
		{
			return m_AllLocalLevels;
		}
	}

	public static WorkshopMapsLoader Instance
	{
		get
		{
			return _instance;
		}
	}

	public List<WorkshopMapWrapper> LoadedCustomLevels
	{
		get
		{
			return m_loadedCustomLevels;
		}
	}

	private void Awake()
	{
		_instance = this;
		InitStuff();
		ValidateStuff();
	}

	private void Start()
	{
		CheckDownloadedItems();
		CheckSubscribedItems();
	}

	private void InitStuff()
	{
		m_DownloadItemCallResult = Callback<DownloadItemResult_t>.Create(OnItemDownloaded);
		m_SubbedCallResult = CallResult<RemoteStorageSubscribePublishedFileResult_t>.Create(OnSubscribed);
		m_UnSubbedCallResult = CallResult<RemoteStorageUnsubscribePublishedFileResult_t>.Create(OnUnSubscribed);
		m_UGCHandleQueryCompleted = CallResult<SteamUGCQueryCompleted_t>.Create(OnSteamUGCQueryCompleted);
		string localWorkshopPath = StickFightDirectoryPaths.Instance.LocalWorkshopPath;
		m_WorkshopPath = ((!Application.isEditor) ? (Application.dataPath + "/../../../Workshop/Content/674940/") : (localWorkshopPath + "/"));
		m_LocalSavePath = Application.persistentDataPath + "/CustomLevels";
		m_WorkshopDirectory = new DirectoryInfo(m_WorkshopPath);
		m_LocalMapsDirectory = new DirectoryInfo(m_LocalSavePath);
		m_AllLocalLevels = LoadAllLocalCustomLevels();
	}

	private void ValidateStuff()
	{
		if (!m_WorkshopDirectory.Exists)
		{
			m_WorkshopDirectory.Create();
		}
		if (!m_LocalMapsDirectory.Exists)
		{
			m_LocalMapsDirectory.Create();
		}
	}

	private void CheckDownloadedItems()
	{
		DirectoryInfo[] directories = m_WorkshopDirectory.GetDirectories();
		int num = directories.Length;
		List<PublishedFileId_t> list = new List<PublishedFileId_t>();
		for (int i = 0; i < num; i++)
		{
			ulong result;
			if (ulong.TryParse(directories[i].Name, out result))
			{
				PublishedFileId_t publishedFileId_t = new PublishedFileId_t(result);
				if (IsSubbedToItem(publishedFileId_t))
				{
					list.Add(publishedFileId_t);
				}
			}
		}
		Debug.Log("Sending UGCQuery With: " + list.Count + " IDs!");
		UGCQueryHandle_t handle = SteamUGC.CreateQueryUGCDetailsRequest(list.ToArray(), (uint)list.Count);
		SteamAPICall_t hAPICall = SteamUGC.SendQueryUGCRequest(handle);
		m_UGCHandleQueryCompleted.Set(hAPICall);
	}

	private bool IsSubbedToItem(PublishedFileId_t id)
	{
		EItemState itemState = (EItemState)SteamUGC.GetItemState(id);
		Debug.Log("State for: " + id.ToString() + itemState);
		return (itemState & EItemState.k_EItemStateSubscribed) != 0;
	}

	private void CheckSubscribedItems()
	{
		uint numSubscribedItems = SteamUGC.GetNumSubscribedItems();
		PublishedFileId_t[] array = new PublishedFileId_t[numSubscribedItems];
		uint subscribedItems = SteamUGC.GetSubscribedItems(array, numSubscribedItems);
		Debug.Log("Getting Subbed Items: " + numSubscribedItems + " Result: " + subscribedItems);
		PublishedFileId_t[] array2 = array;
		foreach (PublishedFileId_t publishedFileId_t in array2)
		{
			EItemState itemState = (EItemState)SteamUGC.GetItemState(publishedFileId_t);
			Debug.Log(string.Concat("State For Item: ", publishedFileId_t, " : ", itemState.ToString()));
			switch (itemState)
			{
			case EItemState.k_EItemStateNeedsUpdate:
				SteamUGC.DownloadItem(publishedFileId_t, false);
				break;
			case EItemState.k_EItemStateNone:
				SteamUGC.DownloadItem(publishedFileId_t, false);
				break;
			}
		}
	}

	private void SubscribeToItem(PublishedFileId_t item)
	{
		SteamAPICall_t hAPICall = SteamUGC.SubscribeItem(item);
		m_SubbedCallResult.Set(hAPICall);
	}

	private void UnSubscribeToItem(PublishedFileId_t item)
	{
		SteamAPICall_t hAPICall = SteamUGC.UnsubscribeItem(item);
		m_UnSubbedCallResult.Set(hAPICall);
	}

	public PlayableWorkshopLevel[] GetAllWorkshopMaps()
	{
		List<PlayableWorkshopLevel> list = new List<PlayableWorkshopLevel>();
		DirectoryInfo[] directories = m_WorkshopDirectory.GetDirectories();
		DirectoryInfo[] directories2 = m_LocalMapsDirectory.GetDirectories();
		int num = directories.Length;
		int num2 = directories2.Length;
		if (num + num2 <= 0)
		{
			return new PlayableWorkshopLevel[0];
		}
		int num3 = 0;
		int num4 = 0;
		bool flag = false;
		bool flag2 = num2 > 0;
		for (int i = 0; i < num2; i++)
		{
			DirectoryInfo directoryInfo = directories2[i];
			list.Add(new PlayableWorkshopLevel
			{
				Path = directoryInfo.FullName,
				MapID = ulong.MaxValue
			});
		}
		for (int j = 0; j < num; j++)
		{
			DirectoryInfo directoryInfo = directories[j];
			if (directoryInfo != null)
			{
				FileInfo fileInfo = new FileInfo(directoryInfo.FullName + "/Level.bin");
				if (!fileInfo.Exists)
				{
					directories[j] = null;
					Debug.Log("No Level File was found in folder: " + directoryInfo.FullName);
					continue;
				}
				ulong mapID = ulong.Parse(directoryInfo.Name);
				list.Add(new PlayableWorkshopLevel
				{
					MapID = mapID,
					Path = directoryInfo.FullName
				});
				Debug.Log("Adding Workshop Map: " + mapID);
			}
		}
		return list.ToArray();
	}

	public bool CheckNewWorkshopMaps(ulong map, Action callback, bool silentDownload = false)
	{
		return NewMapCycleLoaded(new ulong[1] { map }, callback, silentDownload);
	}

	public bool NewMapCycleLoaded(ulong[] maps, Action callback, bool silentDownload = false)
	{
		Debug.Log("New Mapcycle loaded: NrOfMaps: " + maps.Length + " TIME: " + Time.unscaledTime);
		if (m_RegisteredMaps.Count > 0)
		{
			Debug.Log("Getting New Mapcycle but there is still Maps that are in the list!? Count: " + m_RegisteredMaps.Count);
		}
		m_RegisteredMaps.Clear();
		bool flag = false;
		m_lastMapNeededDownloading = false;
		m_DownloadFailed = false;
		int num = maps.Length;
		for (int i = 0; i < num; i++)
		{
			PublishedFileId_t publishedFileId_t = new PublishedFileId_t(maps[i]);
			EItemState itemState = GetItemState(publishedFileId_t);
			bool flag2 = (itemState & EItemState.k_EItemStateInstalled) != 0;
			bool flag3 = (itemState & EItemState.k_EItemStateNeedsUpdate) != 0;
			if (!flag2 || (flag2 && flag3))
			{
				m_lastMapNeededDownloading = true;
				Debug.Log("State For Map: " + itemState);
				flag = true;
				if (SteamUGC.DownloadItem(publishedFileId_t, false))
				{
					m_IsDownloading = true;
					RegisterMap(publishedFileId_t);
				}
			}
		}
		m_CallBack = null;
		if (flag)
		{
			m_CallBack = callback;
			if (!silentDownload)
			{
				LoadingScreenManager.Instance.StartLoading();
				LoadingScreenManager.Instance.ChangeLoadingScreenText("Downloading Custom Maps: " + (m_TotalRegisteredMaps - m_RegisteredMaps.Count) + "/" + m_TotalRegisteredMaps);
			}
		}
		return flag;
	}

	private IEnumerator WaitThenPerformAction(float time, Action a)
	{
		for (int i = 0; i < (int)time; i++)
		{
			yield return new WaitForSeconds(1f);
			LoadingScreenManager.Instance.ChangeLoadingScreenText("Downloading Custom Maps: " + i + "/" + (int)time);
		}
		a();
	}

	private void RegisterMap(PublishedFileId_t id)
	{
		if (m_RegisteredMaps.Contains(id))
		{
			Debug.Log("Map: " + id.m_PublishedFileId + " Is already Registered, returning...");
			return;
		}
		m_RegisteredMaps.Add(id);
		m_TotalRegisteredMaps = m_RegisteredMaps.Count;
		Debug.Log("Register Map: " + id);
	}

	private void CheckMap(PublishedFileId_t id)
	{
		if (!m_RegisteredMaps.Contains(id))
		{
			Debug.LogError(string.Concat("Map: ", id, " Was not present in registered Maps!"));
			return;
		}
		m_RegisteredMaps.Remove(id);
		LoadingScreenManager.Instance.ChangeLoadingScreenText("Downloading Custom Maps: " + (m_TotalRegisteredMaps - m_RegisteredMaps.Count) + "/" + m_TotalRegisteredMaps);
		if (m_RegisteredMaps.Count > 0)
		{
			return;
		}
		m_IsDownloading = false;
		if (m_DownloadFailed)
		{
			m_DownloadFailed = false;
			UnityEngine.Object.FindObjectOfType<LoadingScreenManager>().LoadThenFail(ConnectionErrorType.DownloadFailure, string.Empty);
			return;
		}
		Debug.Log("All Registered Maps Have Been Downloaded: Calling Callback...   TIME: " + Time.unscaledTime);
		LoadingScreenManager.Instance.StopLoading();
		if (m_CallBack != null)
		{
			m_CallBack();
		}
	}

	private EItemState GetItemState(PublishedFileId_t id)
	{
		return (EItemState)SteamUGC.GetItemState(id);
	}

	public LocalWorkshopWrapper[] LoadAllLocalCustomLevels(int someFilter = 0)
	{
		Debug.Log("Trying to load all saved custom maps...");
		CheckLocalWorkshopFolder();
		List<LocalWorkshopWrapper> list = new List<LocalWorkshopWrapper>();
		DirectoryInfo[] directories = new DirectoryInfo(m_LocalSavePath).GetDirectories();
		Debug.Log("Found: " + directories.Length + " At Dir: " + m_LocalSavePath);
		CustomLevel customLevel = null;
		bool flag = true;
		DirectoryInfo[] array = directories;
		foreach (DirectoryInfo directoryInfo in array)
		{
			flag = true;
			IFormatter formatter = new BinaryFormatter();
			try
			{
				Stream stream = new FileStream(directoryInfo.FullName + "/Level.bin", FileMode.Open, FileAccess.Read, FileShare.None);
				customLevel = (CustomLevel)formatter.Deserialize(stream);
				stream.Close();
			}
			catch (Exception ex)
			{
				Debug.LogError("An error occured while trying to load workshop maps: " + ex.Message);
				flag = false;
			}
			if (flag)
			{
				string mapName = directoryInfo.Name;
				string fullName = directoryInfo.FullName;
				string fileName = directoryInfo.FullName + "/ScreenShot.png";
				FileInfo fileInfo = new FileInfo(fileName);
				byte[] imageData = null;
				if (fileInfo.Exists)
				{
					imageData = File.ReadAllBytes(fileInfo.FullName);
				}
				list.Add(new LocalWorkshopWrapper(mapName, fullName, imageData));
				Debug.Log("Successfully loaded workshop map: " + directoryInfo.Name);
			}
		}
		return list.ToArray();
	}

	public CustomLevel GetWorkshopMapOnDisk(string localPath)
	{
		float unscaledTime = Time.unscaledTime;
		Debug.Log("Getting Workshop Map On Disk: " + localPath);
		DirectoryInfo directoryInfo = new DirectoryInfo(localPath);
		FileInfo fileInfo = directoryInfo.GetFiles()[0];
		string fullName = fileInfo.FullName;
		CustomLevel customLevel = LoadLevel(fullName);
		float unscaledTime2 = Time.unscaledTime;
		Debug.Log("DeserializedMap: " + customLevel.PlacedObjects.Count + " : " + unscaledTime2);
		Debug.Log("Total Time: " + (unscaledTime2 - unscaledTime) + " Seconds");
		return customLevel;
	}

	public CustomLevel GetWorkshopMapOnDisk(ulong mapID)
	{
		float unscaledTime = Time.unscaledTime;
		Debug.Log("Getting Workshop Map On Disk: " + mapID);
		string text = mapID.ToString();
		DirectoryInfo[] directories = m_WorkshopDirectory.GetDirectories();
		int num = directories.Length;
		for (int i = 0; i < num; i++)
		{
			DirectoryInfo directoryInfo = directories[i];
			if (directoryInfo.Name == text)
			{
				if (directoryInfo.GetFiles().Length > 1)
				{
					Debug.LogError("Several Files was found within a workshop Folder...");
				}
				Debug.Log("Trying to get file from directory: " + directoryInfo.FullName);
				FileInfo fileInfo = directoryInfo.GetFiles()[0];
				string fullName = fileInfo.FullName;
				CustomLevel customLevel = LoadLevel(fullName);
				float unscaledTime2 = Time.unscaledTime;
				Debug.Log("DeserializedMap: " + customLevel.PlacedObjects.Count + " : " + unscaledTime2);
				Debug.Log("Total Time: " + (unscaledTime2 - unscaledTime) + " Seconds");
				return customLevel;
			}
		}
		Debug.LogError("Could NOt Find Map: " + mapID + " On Disk, Has it been removed faulty by the user?");
		return null;
	}

	private CustomLevel LoadLevel(string path)
	{
		IFormatter formatter = new BinaryFormatter();
		Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None);
		CustomLevel result = (CustomLevel)formatter.Deserialize(stream);
		stream.Close();
		return result;
	}

	protected void CheckLocalWorkshopFolder()
	{
		if (!Directory.Exists(m_LocalSavePath))
		{
			Directory.CreateDirectory(m_LocalSavePath);
		}
	}

	private void OnSteamUGCQueryCompleted(SteamUGCQueryCompleted_t param, bool bIOFailure)
	{
		if (bIOFailure)
		{
			Debug.Log("Biofail...");
		}
		else
		{
			if (param.m_eResult != EResult.k_EResultOK)
			{
				return;
			}
			Debug.Log("UGCQuery Returned WIth: " + param.m_unNumResultsReturned + " Results");
			m_loadedCustomLevels = new List<WorkshopMapWrapper>();
			for (uint num = 0u; num < param.m_unNumResultsReturned; num++)
			{
				SteamUGCDetails_t pDetails;
				if (SteamUGC.GetQueryUGCResult(param.m_handle, num, out pDetails))
				{
					if (pDetails.m_rgchTitle == string.Empty)
					{
						UnSubscribeToItem(pDetails.m_nPublishedFileId);
						Debug.LogError(string.Concat("Error in title for object: ", pDetails.m_nPublishedFileId, " : ", pDetails.m_rgchURL));
					}
					else
					{
						Debug.Log("Returned UGC Results: " + pDetails.m_rgchTitle);
						m_loadedCustomLevels.Add(new WorkshopMapWrapper(pDetails.m_rgchTitle, pDetails.m_rgchDescription, pDetails.m_rtimeCreated, pDetails.m_nPublishedFileId, pDetails.m_ulSteamIDOwner, pDetails.m_hPreviewFile, pDetails.m_nPreviewFileSize, pDetails.m_eVisibility));
					}
				}
			}
			MapSelectionHandler mapSelectionHandler = UnityEngine.Object.FindObjectOfType<MapSelectionHandler>();
			if (mapSelectionHandler != null)
			{
				mapSelectionHandler.CustomMapsWasLoaded(m_loadedCustomLevels);
			}
		}
	}

	private void OnItemDownloaded(DownloadItemResult_t param)
	{
		if (param.m_unAppID != SteamUtils.GetAppID())
		{
			Debug.Log("A file from another Game was downloaded, Ignoring...");
			return;
		}
		if (param.m_eResult != EResult.k_EResultOK)
		{
			m_DownloadFailed = true;
		}
		Debug.Log(string.Concat("Download Result: For Item: ", param.m_nPublishedFileId, " : ", param.m_eResult));
		if (m_IsDownloading)
		{
			CheckMap(param.m_nPublishedFileId);
		}
	}

	private void OnSubscribdedAction(Action a)
	{
		m_OnSubscribdedAction = (Action)Delegate.Combine(m_OnSubscribdedAction, a);
	}

	private void OnUnSubscribed(RemoteStorageUnsubscribePublishedFileResult_t param, bool bIOFailure)
	{
		Debug.Log(string.Concat("UnSubbed: ", param.m_nPublishedFileId, " : ", param.m_eResult));
		if (m_OnUnSubbedAction != null)
		{
			m_OnUnSubbedAction();
			m_OnUnSubbedAction = null;
		}
	}

	private void OnSubscribed(RemoteStorageSubscribePublishedFileResult_t param, bool bIOFailure)
	{
		Debug.Log(string.Concat("Subbed: ", param.m_nPublishedFileId, " : ", param.m_eResult));
		if (m_OnSubscribdedAction != null)
		{
			m_OnSubscribdedAction();
			m_OnSubscribdedAction = null;
		}
	}

	public DirectoryInfo[] LoadAllLocalMaps()
	{
		Debug.Log("Trying to load all saved custom maps...");
		CheckLocalWorkshopFolder();
		DirectoryInfo[] directories = new DirectoryInfo(m_LocalSavePath).GetDirectories();
		Debug.Log("Found: " + directories.Length + " At Dir: " + m_LocalSavePath);
		return directories;
	}

	public WorkshopMapWrapper[] LoadAllWorkshopMaps()
	{
		if (!SteamManager.Initialized)
		{
			return new WorkshopMapWrapper[0];
		}
		CheckDownloadedItems();
		Debug.Log("Trying To LoadAll Workshop CustomMaps");
		List<WorkshopMapWrapper> list = new List<WorkshopMapWrapper>();
		DirectoryInfo[] directories = new DirectoryInfo(m_WorkshopPath).GetDirectories();
		Debug.Log("Found: " + directories.Length + " At Dir: " + m_WorkshopPath);
		DirectoryInfo[] array = directories;
		foreach (DirectoryInfo directoryInfo in array)
		{
			PublishedFileId_t publishedFileId_t = new PublishedFileId_t(ulong.Parse(directoryInfo.Name));
			EItemState itemState = (EItemState)SteamUGC.GetItemState(publishedFileId_t);
			Debug.Log("Item STate: " + itemState);
			if ((itemState & EItemState.k_EItemStateInstalled) != EItemState.k_EItemStateNone && (itemState & EItemState.k_EItemStateSubscribed) != EItemState.k_EItemStateNone)
			{
				WorkshopMapWrapper workshopMapWrapper = FindWrapperWithID(publishedFileId_t);
				if (workshopMapWrapper != null)
				{
					list.Add(workshopMapWrapper);
				}
			}
		}
		Debug.Log("Returning: " + list.Count + " INstalled workshop maps");
		return list.ToArray();
	}

	private WorkshopMapWrapper FindWrapperWithID(PublishedFileId_t id)
	{
		if (m_loadedCustomLevels == null)
		{
			return null;
		}
		foreach (WorkshopMapWrapper loadedCustomLevel in m_loadedCustomLevels)
		{
			if (loadedCustomLevel.PublishID == id)
			{
				Debug.Log("Found Wrapper With ID: " + id.ToString());
				return loadedCustomLevel;
			}
		}
		return null;
	}

	public void DeleteWorkshopMap(WorkshopMapWrapper wrap, Action a)
	{
		m_OnUnSubbedAction = (Action)Delegate.Combine(m_OnUnSubbedAction, a);
		SteamAPICall_t hAPICall = SteamUGC.UnsubscribeItem(wrap.PublishID);
		m_UnSubbedCallResult.Set(hAPICall);
	}
}
