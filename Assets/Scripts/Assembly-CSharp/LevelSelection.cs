using System;
using System.Collections.Generic;
using System.IO;
using LevelEditor;
using Steamworks;
using UnityEngine;

public class LevelSelection : MonoBehaviour
{
	[Serializable]
	public class ImageData
	{
		public List<SingleImageData> ImageFiles = new List<SingleImageData>();
	}

	[Serializable]
	public class SingleImageData
	{
		public int Index;

		public Texture2D MapImage;
	}

	[SerializeField]
	private Dictionary<int, byte[]> m_ImageDictionary = new Dictionary<int, byte[]>();

	[SerializeField]
	private ImageData m_ImageData;

	private int numberOfMaps;

	private int currentMap;

	private int currentWorld;

	public int ForceID = 89;

	private const int statsMapIndex = 102;

	private int completedLevelsSinceLastStats;

	public int[] forestLevels;

	private int currentForestLevel;

	public int[] desertLevels;

	private int currentDesertLevel;

	public int[] factoryLevels;

	private int currentFactoryLevels;

	public int[] castleLevels;

	private int currentCastleLevel;

	public int[] winterLevels;

	private int currentWinterLevel;

	public int[] westernLevels;

	private int currentWesternLevel;

	public int[] laserLevels;

	private int currentLaserLevel;

	public int[] lavaLevels;

	private int currentLavaLevel;

	public int[] xmasLevels;

	public int[] halloweenLevels;

	private int currentHalloweenLevel;

	public int[] rareLevels;

	private int currentRareLevel;

	public int[] godLevels;

	public int[] multiplayerBlackList;

	private WorkshopMapsLoader m_WorkshopLoader;

	private void Awake()
	{
	}

	private void Start()
	{
		numberOfMaps = Application.levelCount - 2;
		m_WorkshopLoader = WorkshopMapsLoader.Instance;
		if (!Application.isEditor)
		{
			ForceID = 0;
		}
	}

	public void MakeNewWorkshopLevelCycle()
	{
	}

	public void RemoveAllLocalMapsFromWorkshopCycle()
	{
		UnityEngine.Object.FindObjectOfType<MapSelectionHandler>().DisableCategory(MapWorldsEnum.CustomLocal);
	}

	public PlayableWorkshopLevel[] GetAllCustomMapsActive(bool includeLocalMaps)
	{
		List<PlayableWorkshopLevel> list = new List<PlayableWorkshopLevel>();
		List<SingleMapUI> maps = UnityEngine.Object.FindObjectOfType<MapSelectionHandler>().GetMaps(MapWorldsEnum.CustomOnline, true);
		int count = maps.Count;
		for (int i = 0; i < count; i++)
		{
			list.Add(new PlayableWorkshopLevel
			{
				MapID = ulong.Parse(maps[i].MapIndex)
			});
		}
		return list.ToArray();
	}

	private int GetOrderedLevel()
	{
		currentMap++;
		int result = 1;
		if (currentWorld == 0)
		{
			if (currentMap >= forestLevels.Length)
			{
				currentMap = 0;
				currentWorld++;
			}
			result = forestLevels[currentMap];
		}
		if (currentWorld == 1)
		{
			if (currentMap >= desertLevels.Length)
			{
				currentMap = 0;
				currentWorld++;
			}
			result = desertLevels[currentMap];
		}
		if (currentWorld == 2)
		{
			if (currentMap >= factoryLevels.Length)
			{
				currentMap = 0;
				currentWorld++;
			}
			result = factoryLevels[currentMap];
		}
		if (currentWorld == 3)
		{
			if (currentMap >= castleLevels.Length)
			{
				currentMap = 0;
				currentWorld++;
			}
			result = castleLevels[currentMap];
		}
		if (currentWorld == 4)
		{
			if (currentMap >= winterLevels.Length)
			{
				currentMap = 0;
				currentWorld++;
			}
			result = winterLevels[currentMap];
		}
		if (currentWorld == 5)
		{
			if (currentMap >= westernLevels.Length)
			{
				currentMap = 0;
				currentWorld++;
			}
			result = westernLevels[currentMap];
		}
		if (currentWorld == 6)
		{
			if (currentMap >= laserLevels.Length)
			{
				currentMap = 0;
				currentWorld++;
			}
			result = laserLevels[currentMap];
		}
		if (currentWorld == 7)
		{
			if (currentMap >= lavaLevels.Length)
			{
				currentMap = 0;
				currentWorld++;
			}
			result = lavaLevels[currentMap];
		}
		if (currentWorld == 8)
		{
			if (currentMap >= xmasLevels.Length)
			{
				currentMap = 0;
				currentWorld++;
			}
			result = xmasLevels[currentMap];
		}
		if (currentWorld == 9)
		{
			if (currentMap >= halloweenLevels.Length)
			{
				currentMap = 0;
				currentWorld = 0;
				result = forestLevels[currentMap];
				if (PlayerPrefs.GetInt("HasChangedLevelPref") == 0)
				{
					OptionsHolder.maps = 1;
				}
			}
			else
			{
				result = halloweenLevels[currentMap];
			}
		}
		return result;
	}

	public MapWrapper GetNextLevel()
	{
		completedLevelsSinceLastStats++;
		int num = 0;
		if (completedLevelsSinceLastStats > 30 && !MatchmakingHandler.IsNetworkMatch)
		{
			completedLevelsSinceLastStats = 0;
			num = 102;
			MapWrapper mapWrapper = new MapWrapper();
			mapWrapper.MapType = 0;
			mapWrapper.MapData = BitConverter.GetBytes(num);
			Debug.LogError("Fixa Här");
			return mapWrapper;
		}
		List<string> list = new List<string>();
		if (MatchmakingHandler.IsNetworkMatch)
		{
			int[] array = multiplayerBlackList;
			for (int i = 0; i < array.Length; i++)
			{
				int num2 = array[i];
				list.Add(num2.ToString());
			}
			if (m_WorkshopLoader != null && m_WorkshopLoader.LoadedCustomLevels != null)
			{
				foreach (WorkshopMapWrapper loadedCustomLevel in m_WorkshopLoader.LoadedCustomLevels)
				{
					if (loadedCustomLevel.Visibility != ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPublic)
					{
						list.Add(loadedCustomLevel.PublishID.m_PublishedFileId.ToString());
					}
				}
			}
		}
		SingleMapUI randomLevel = UnityEngine.Object.FindObjectOfType<MapSelectionHandler>().GetRandomLevel(true, list);
		return MakeMapWrapperOf(randomLevel);
	}

	private MapWrapper MakeMapWrapperOf(SingleMapUI map)
	{
		MapWrapper mapWrapper = new MapWrapper();
		mapWrapper.MapType = (byte)map.MapTypeEnum;
		MapWrapper mapWrapper2 = mapWrapper;
		switch (map.MapTypeEnum)
		{
		case MapType.Landfall:
		{
			byte[] array = new byte[4];
			using (MemoryStream output2 = new MemoryStream(array))
			{
				using (BinaryWriter binaryWriter2 = new BinaryWriter(output2))
				{
					binaryWriter2.Write(int.Parse(map.MapIndex));
				}
			}
			mapWrapper2.MapData = array;
			break;
		}
		case MapType.CustomLocal:
		{
			int length = map.MapIndex.Length;
			byte[] array = new byte[length * 2];
			using (MemoryStream output3 = new MemoryStream(array))
			{
				using (BinaryWriter binaryWriter3 = new BinaryWriter(output3))
				{
					binaryWriter3.Write(map.MapIndex);
				}
			}
			mapWrapper2.MapData = array;
			break;
		}
		case MapType.CustomOnline:
		{
			byte[] array = new byte[8];
			using (MemoryStream output = new MemoryStream(array))
			{
				using (BinaryWriter binaryWriter = new BinaryWriter(output))
				{
					binaryWriter.Write(ulong.Parse(map.MapIndex));
				}
			}
			mapWrapper2.MapData = array;
			break;
		}
		}
		return mapWrapper2;
	}

	public void SetNewCustomMapCycle(ulong[] maps)
	{
		Debug.LogError("Nothing here");
	}

	public bool LoadCustomLevel(MapWrapper index, Action<float> mapSizeAction)
	{
		if (m_WorkshopLoader.IsDownloading)
		{
			Debug.Log("Maps Is Still Downloading...");
			return true;
		}
		CustomLevel customLevel = null;
		byte[] mapData = index.MapData;
		switch ((MapType)index.MapType)
		{
		case MapType.Landfall:
			throw new Exception("Impossible");
		case MapType.CustomLocal:
		{
			string text;
			using (MemoryStream input2 = new MemoryStream(mapData))
			{
				using (BinaryReader binaryReader2 = new BinaryReader(input2))
				{
					text = binaryReader2.ReadString();
				}
			}
			text += "/Level.bin";
			customLevel = WorkshopMapsLoader.Instance.GetWorkshopMapOnDisk(text);
			break;
		}
		case MapType.CustomOnline:
		{
			ulong mapID;
			using (MemoryStream input = new MemoryStream(mapData))
			{
				using (BinaryReader binaryReader = new BinaryReader(input))
				{
					mapID = binaryReader.ReadUInt64();
				}
			}
			customLevel = WorkshopMapsLoader.Instance.GetWorkshopMapOnDisk(mapID);
			break;
		}
		}
		if (customLevel == null)
		{
			return false;
		}
		mapSizeAction((customLevel.MapSize != 0f) ? customLevel.MapSize : 10f);
		WorkshopLevelManager.SetNewLoadedLevel(customLevel, index);
		return WorkshopLevelManager.InitCurrentLoadedLevel();
	}

	public int[] GetLevelsWithWorld(MapWorldsEnum world)
	{
		switch (world)
		{
		case MapWorldsEnum.Woods:
			return forestLevels;
		case MapWorldsEnum.Desert:
			return desertLevels;
		case MapWorldsEnum.Factory:
			return factoryLevels;
		case MapWorldsEnum.Castle:
			return castleLevels;
		case MapWorldsEnum.Winter:
			return winterLevels;
		case MapWorldsEnum.Laser:
			return laserLevels;
		case MapWorldsEnum.Western:
			return westernLevels;
		case MapWorldsEnum.Lava:
			return lavaLevels;
		case MapWorldsEnum.Xmas:
			return xmasLevels;
		case MapWorldsEnum.Halloween:
			return halloweenLevels;
		default:
			return new int[0];
		}
	}

	public byte[] GetImageDataForMapWithIndex(int index)
	{
		foreach (SingleImageData imageFile in m_ImageData.ImageFiles)
		{
			if (imageFile.Index == index)
			{
				return imageFile.MapImage.EncodeToJPG();
			}
		}
		if (!m_ImageDictionary.ContainsKey(index))
		{
			return new byte[0];
		}
		return m_ImageDictionary[index];
	}

	public void AddMapImage(int index, Texture2D image)
	{
		SingleImageData singleImageData = m_ImageData.ImageFiles.Find((SingleImageData Hej) => Hej.Index == index);
		if (singleImageData != null)
		{
			Debug.LogError("Removing Map: " + singleImageData.Index);
			m_ImageData.ImageFiles.Remove(singleImageData);
		}
		m_ImageData.ImageFiles.Add(new SingleImageData
		{
			Index = index,
			MapImage = image
		});
	}

	public void Finished()
	{
	}

	public bool IsDownloadingMaps()
	{
		return m_WorkshopLoader.IsDownloading;
	}

	public bool LastMapNeededDownloading()
	{
		return m_WorkshopLoader.m_lastMapNeededDownloading;
	}
}
