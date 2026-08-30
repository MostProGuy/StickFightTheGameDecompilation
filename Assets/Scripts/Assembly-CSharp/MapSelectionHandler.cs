using System;
using System.Collections.Generic;
using InControl;
using LevelEditor;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapSelectionHandler : MonoBehaviour
{
	public class MapCategoryUI
	{
		public Transform CategoryGrid;

		public bool IsActive;

		public string CategoryName;

		public int Index;

		public List<SingleMapUI> CategoryMaps = new List<SingleMapUI>();

		public Toggle CategoryToggle;

		public MapCategoryUI(string categoryName, int index, Transform t, Toggle toggle)
		{
			CategoryName = categoryName;
			Index = index;
			IsActive = true;
			CategoryGrid = t;
			CategoryToggle = toggle;
		}

		public void ChangeName(string newName)
		{
			CategoryName = newName;
		}

		public void AddMapToCategory(SingleMapUI newMap)
		{
			CategoryMaps.Add(newMap);
		}

		public SingleMapUI[] GetMapsForPage(int page, int nrOfMaps)
		{
			List<SingleMapUI> list = new List<SingleMapUI>();
			if (page * nrOfMaps > CategoryMaps.Count)
			{
				return list.ToArray();
			}
			int num = page * nrOfMaps;
			for (int i = num; i < num + nrOfMaps && i < CategoryMaps.Count; i++)
			{
				list.Add(CategoryMaps[i]);
			}
			return list.ToArray();
		}
	}

	[Serializable]
	public class PresetMapCategoryUI
	{
		public MapWorldsEnum WorldEnum;

		public List<PresetSingleMapUI> Maps;
	}

	[Serializable]
	public struct PresetSingleMapUI
	{
		public string MapName;
	}

	[Serializable]
	public class SaveableCategory
	{
		public string CategoryName;

		public bool CategoryActive;

		public bool[] Actives;

		public string[] Indexes;
	}

	[Serializable]
	public class SavedMaps
	{
		public bool RandomOrderedMaps;

		public SaveableCategory[] SaveableCategories;
	}

	[SerializeField]
	private MapSelectionPreviewUI m_PreviewUI;

	[SerializeField]
	private Texture2D m_DefaultPreviewTexture;

	[SerializeField]
	private PresetMapCategoryUI[] m_PresetMaps;

	[SerializeField]
	private GameObject m_MapCell;

	[SerializeField]
	private Button m_GoRightArrow;

	[SerializeField]
	private Button m_GoLeftArrow;

	[SerializeField]
	private Toggle m_RandomOrderMapsToggle;

	[SerializeField]
	private RectTransform m_Selector;

	[SerializeField]
	private Color m_CategoryActiveColor;

	[SerializeField]
	private Color m_CategoryDeactiveColor;

	private List<MapCategoryUI> m_Categories = new List<MapCategoryUI>();

	private WorkshopMapWrapper[] m_WorkshopMaps;

	private bool m_RandomOrderMaps;

	private int m_MaxPages = 1;

	private int m_PageIndex;

	private int m_MapsPerPage = 8;

	private GameManager m_Manager;

	private LevelSelection m_LevelSelection;

	private WorkshopMapsLoader m_WorkshopMapsLoader;

	private SingleMapUI m_LastPlayedMap;

	private SingleMapUI m_LastSelectedMap;

	private SavedMaps m_Saved;

	private bool m_HasSavedData;

	private bool m_Active;

	public bool Active
	{
		get
		{
			return m_Active;
		}
	}

	public static MapSelectionHandler Instance { get; private set; }

	public List<SingleMapUI> GetMaps(MapWorldsEnum type, bool hasToBeActive)
	{
		MapCategoryUI mapCategoryUI = m_Categories.Find((MapCategoryUI Cat) => Cat.CategoryName.ToLower() == type.ToString().ToLower());
		if (!mapCategoryUI.IsActive)
		{
			return new List<SingleMapUI>();
		}
		return mapCategoryUI.CategoryMaps.FindAll((SingleMapUI Map) => Map.IsLocallyActive);
	}

	private void Awake()
	{
		Instance = this;
		InitReferences();
		InitListeners();
	}

	private void Start()
	{
		InitInstances();
		GetSavedData();
		InitMapList();
		PopulatePage();
		UpdateUINavigation();
		UpdateSavedData();
		CheckMaxPages();
	}

	private void Update()
	{
		ListenForControllerInput();
	}

	private void ListenForControllerInput()
	{
		if (m_Active)
		{
			InputDevice activeDevice = InputManager.ActiveDevice;
			if (activeDevice.LeftBumper.WasPressed)
			{
				OnLeftArrowClicked();
			}
			else if (activeDevice.RightBumper.WasPressed)
			{
				OnRightArrowClicked();
			}
			else if (activeDevice.Action3.WasPressed)
			{
				m_RandomOrderMapsToggle.isOn = !m_RandomOrderMapsToggle.isOn;
			}
		}
	}

	private void InitInstances()
	{
		m_WorkshopMapsLoader = WorkshopMapsLoader.Instance;
	}

	private void InitReferences()
	{
		m_Manager = GetComponent<GameManager>();
		m_LevelSelection = GetComponent<LevelSelection>();
	}

	private void InitListeners()
	{
		m_GoRightArrow.onClick.AddListener(OnRightArrowClicked);
		m_GoLeftArrow.onClick.AddListener(OnLeftArrowClicked);
		m_RandomOrderMapsToggle.onValueChanged.AddListener(OnRandomOrderMapsToggleClicked);
		List<EventTrigger.Entry> list = new List<EventTrigger.Entry>();
		EventTrigger.Entry entry = new EventTrigger.Entry();
		entry.eventID = EventTriggerType.Select;
		entry.callback.AddListener(OnSelect);
		list.Add(entry);
		entry = new EventTrigger.Entry();
		entry.eventID = EventTriggerType.Deselect;
		entry.callback.AddListener(OnDeselect);
		list.Add(entry);
		EventTrigger eventTrigger = m_RandomOrderMapsToggle.gameObject.FetchComponent<EventTrigger>();
		eventTrigger.triggers.Clear();
		eventTrigger.triggers = list;
		PauseManager pauseManager = UnityEngine.Object.FindObjectOfType<PauseManager>();
		pauseManager.AddOnMapSelectOpenAction(() =>
		{
			Activate();
			PopulatePage();
			UpdateUINavigation();
		});
		pauseManager.AddOnMapSelectCloseAction(DeActivate);
	}

	private void Activate()
	{
		m_Active = true;
	}

	private void DeActivate()
	{
		m_Active = false;
	}

	private void GetSavedData()
	{
		string text = PlayerPrefs.GetString("SavedMaps", string.Empty);
		if (text == string.Empty)
		{
			m_HasSavedData = false;
			return;
		}
		m_HasSavedData = true;
		Debug.Log("Loaded: " + text);
		m_Saved = (SavedMaps)JsonUtility.FromJson(text, typeof(SavedMaps));
		if (m_Saved.SaveableCategories == null)
		{
			m_HasSavedData = false;
		}
		m_RandomOrderMaps = m_Saved.RandomOrderedMaps;
		m_RandomOrderMapsToggle.isOn = m_RandomOrderMaps;
	}

	private void UpdateSavedData()
	{
		int count = m_Categories.Count;
		if (m_Saved == null)
		{
			m_Saved = new SavedMaps();
		}
		if (m_Saved.SaveableCategories == null)
		{
			m_Saved.SaveableCategories = new SaveableCategory[count];
		}
		if (m_Saved.SaveableCategories.Length < count)
		{
			m_Saved.SaveableCategories = new SaveableCategory[count];
		}
		m_Saved.RandomOrderedMaps = m_RandomOrderMaps;
		for (int i = 0; i < count; i++)
		{
			if (m_Categories[i].CategoryMaps.Count > 0)
			{
				SaveableCategory saveableCategory = m_Saved.SaveableCategories[i];
				if (saveableCategory == null)
				{
					saveableCategory = new SaveableCategory();
				}
				saveableCategory.CategoryActive = m_Categories[i].IsActive;
				saveableCategory.CategoryName = m_Categories[i].CategoryName;
				int count2 = m_Categories[i].CategoryMaps.Count;
				saveableCategory.Actives = new bool[count2];
				saveableCategory.Indexes = new string[count2];
				for (int j = 0; j < count2; j++)
				{
					SingleMapUI singleMapUI = m_Categories[i].CategoryMaps[j];
					saveableCategory.Actives[j] = singleMapUI.IsLocallyActive;
					saveableCategory.Indexes[j] = singleMapUI.MapIndex;
				}
				m_Saved.SaveableCategories[i] = saveableCategory;
			}
		}
		string text = JsonUtility.ToJson(m_Saved);
		PlayerPrefs.SetString("SavedMaps", text);
		Debug.Log("SAVED MAPS: " + text);
	}

	private PresetMapCategoryUI GetPresetMapCategoryFromName(string categoryName)
	{
		Debug.Log("Trying to parse Enum: " + categoryName);
		if (!Enum.IsDefined(typeof(MapWorldsEnum), categoryName))
		{
			return null;
		}
		MapWorldsEnum mapWorldsEnum = (MapWorldsEnum)Enum.Parse(typeof(MapWorldsEnum), categoryName);
		PresetMapCategoryUI[] presetMaps = m_PresetMaps;
		foreach (PresetMapCategoryUI presetMapCategoryUI in presetMaps)
		{
			if (presetMapCategoryUI.WorldEnum == mapWorldsEnum)
			{
				return presetMapCategoryUI;
			}
		}
		return null;
	}

	private void ClearPage()
	{
		foreach (MapCategoryUI category in m_Categories)
		{
			Transform categoryGrid = category.CategoryGrid;
			int childCount = categoryGrid.childCount;
			for (int i = 0; i < childCount; i++)
			{
				UnityEngine.Object.Destroy(categoryGrid.GetChild(i).gameObject);
			}
		}
	}

	private void CheckMaxPages()
	{
		int num = 0;
		foreach (MapCategoryUI category in m_Categories)
		{
			if (category.CategoryMaps.Count > num)
			{
				num = category.CategoryMaps.Count;
			}
		}
		float num2 = (float)num / (float)m_MapsPerPage;
		m_MaxPages = Mathf.CeilToInt(num2);
		Debug.Log("Max Pages: " + m_MaxPages + " NrOfmaps: " + num + "div: " + num2);
	}

	private void InitMapList()
	{
		MapCategoryTAG[] componentsInChildren = GetComponentsInChildren<MapCategoryTAG>();
		int num = componentsInChildren.Length;
		for (int i = 0; i < num; i++)
		{
			MapCategoryTAG mapCategoryTAG = componentsInChildren[i];
			Toggle component = mapCategoryTAG.transform.Find("Toggle").GetComponent<Toggle>();
			string text = mapCategoryTAG.name;
			Transform t = mapCategoryTAG.transform.Find("Grid");
			MapCategoryUI mapCategoryUI = new MapCategoryUI(text, i, t, component);
			AddNewCategory(mapCategoryUI);
			if (m_HasSavedData)
			{
				try
				{
					component.isOn = m_Saved.SaveableCategories[i].CategoryActive;
				}
				catch (IndexOutOfRangeException)
				{
					component.isOn = true;
				}
				mapCategoryUI.IsActive = component.isOn;
				UpdateCategoryColor(component);
			}
			AddRuntimeListenersForCategoryToggle(component);
			PresetMapCategoryUI presetMapCategoryUI = GetPresetMapCategoryFromName(text);
			if (presetMapCategoryUI == null)
			{
				presetMapCategoryUI = new PresetMapCategoryUI();
				presetMapCategoryUI.Maps = new List<PresetSingleMapUI>();
				MapWorldsEnum mapWorldsEnum = FindFreeWorldEnum(text);
				if (mapWorldsEnum == MapWorldsEnum.Max)
				{
					continue;
				}
				Debug.Log("Could not find: " + text + " Switched to: " + mapWorldsEnum);
				presetMapCategoryUI.WorldEnum = mapWorldsEnum;
				text = mapWorldsEnum.ToString();
				mapCategoryUI.ChangeName(text);
				component.GetComponentInChildren<TextMeshProUGUI>().text = text.ToUpper();
			}
			mapCategoryTAG.gameObject.name = presetMapCategoryUI.WorldEnum.ToString();
			if (presetMapCategoryUI.WorldEnum == MapWorldsEnum.CustomLocal)
			{
				LocalWorkshopWrapper[] allLocalLevels = m_WorkshopMapsLoader.AllLocalLevels;
				int num2 = allLocalLevels.Length;
				for (int j = 0; j < num2; j++)
				{
					string mapPath = allLocalLevels[j].MapPath;
					string mapName = allLocalLevels[j].MapName;
					byte[] imageData = allLocalLevels[j].ImageData;
					bool isOn = true;
					if (m_HasSavedData)
					{
						try
						{
							int num3 = FindIndexOfMapIndex(i, mapPath);
							if (num3 > -1)
							{
								isOn = m_Saved.SaveableCategories[i].Actives[num3];
							}
							Debug.Log("Found Saved Data for map: " + mapPath + " Using Data For: " + m_Saved.SaveableCategories[i].Indexes[j]);
						}
						catch (Exception)
						{
							isOn = true;
						}
					}
					SingleMapUI newMap = new SingleMapUI(mapName, text, mapPath, isOn, imageData);
					AddNewMap(mapCategoryUI, newMap);
				}
				Debug.Log("Added: " + num2 + " To Category: " + text);
				continue;
			}
			int[] levelsWithWorld = m_LevelSelection.GetLevelsWithWorld(presetMapCategoryUI.WorldEnum);
			int num4 = levelsWithWorld.Length;
			for (int k = 0; k < num4; k++)
			{
				int num5 = levelsWithWorld[k];
				string mapName2 = text + (k + 1);
				if (presetMapCategoryUI.Maps.Count > k)
				{
					mapName2 = presetMapCategoryUI.Maps[k].MapName;
				}
				bool isOn2 = true;
				if (m_HasSavedData)
				{
					try
					{
						isOn2 = m_Saved.SaveableCategories[i].Actives[k];
					}
					catch (Exception)
					{
						isOn2 = true;
					}
				}
				byte[] imageDataForMapWithIndex = m_LevelSelection.GetImageDataForMapWithIndex(num5);
				SingleMapUI newMap2 = new SingleMapUI(mapName2, text, num5, isOn2, imageDataForMapWithIndex);
				AddNewMap(mapCategoryUI, newMap2);
			}
			Debug.Log("Added: " + num4 + " To Category: " + text);
		}
	}

	private MapWorldsEnum FindFreeWorldEnum(string current)
	{
		bool flag = true;
		Array values = Enum.GetValues(typeof(MapWorldsEnum));
		foreach (object item in values)
		{
			flag = true;
			foreach (MapCategoryUI category in m_Categories)
			{
				if (category.CategoryName.ToLower() == ((MapWorldsEnum)item/*cast due to constrained. prefix*/).ToString().ToLower() && category.CategoryName.ToLower() != current.ToLower())
				{
					Debug.Log("FAILED: " + category.CategoryName);
					flag = false;
					break;
				}
			}
			if (flag)
			{
				Debug.Log("Returning: " + (MapWorldsEnum)item);
				return (MapWorldsEnum)item;
			}
		}
		return MapWorldsEnum.Max;
	}

	private void PopulatePage()
	{
		ClearPage();
		foreach (MapCategoryUI category in m_Categories)
		{
			Transform categoryGrid = category.CategoryGrid;
			SingleMapUI[] mapsForPage = category.GetMapsForPage(m_PageIndex, m_MapsPerPage);
			SingleMapUI[] array = mapsForPage;
			foreach (SingleMapUI singleMapUI in array)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(m_MapCell);
				gameObject.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = singleMapUI.MapName;
				gameObject.GetComponent<SingleWeaponCellUI>().Init(singleMapUI, OnPreview);
				Toggle component = gameObject.GetComponent<Toggle>();
				singleMapUI.AddToggle(component);
				component.isOn = singleMapUI.IsLocallyActive;
				AddRuntimeListenersForSingleMapToggle(component);
				gameObject.transform.SetParent(categoryGrid, false);
				gameObject.SetActive(true);
			}
		}
	}

	private void AddRuntimeListenersForSingleMapToggle(Toggle mapToggle)
	{
		mapToggle.onValueChanged.RemoveAllListeners();
		mapToggle.onValueChanged.AddListener((bool Hej) =>
		{
			OnSingleMapToggle(mapToggle);
		});
		List<EventTrigger.Entry> list = new List<EventTrigger.Entry>();
		EventTrigger.Entry entry = new EventTrigger.Entry();
		entry.eventID = EventTriggerType.Select;
		entry.callback.AddListener(OnSelect);
		entry.callback.AddListener(OnPreview);
		list.Add(entry);
		entry = new EventTrigger.Entry();
		entry.eventID = EventTriggerType.Deselect;
		entry.callback.AddListener(OnDeselect);
		list.Add(entry);
		EventTrigger eventTrigger = mapToggle.gameObject.FetchComponent<EventTrigger>();
		eventTrigger.triggers.Clear();
		eventTrigger.triggers = list;
	}

	private void OnPreview(SingleWeaponCellUI arg0)
	{
		Debug.Log("Switcing Preview to: " + arg0.MapIndex);
		Texture2D texture2D = new Texture2D(40, 40);
		if (arg0.ImageData == null || arg0.ImageData.Length == 0)
		{
			ApplyNewPreviewTexture(m_DefaultPreviewTexture, arg0);
		}
		else if (texture2D.LoadImage(arg0.ImageData, false))
		{
			ApplyNewPreviewTexture(texture2D, arg0);
		}
	}

	private void OnPreview(BaseEventData arg0)
	{
		GameObject selectedObject = arg0.selectedObject;
		SingleWeaponCellUI component = selectedObject.GetComponent<SingleWeaponCellUI>();
		Texture2D texture2D = new Texture2D(40, 40);
		if (component.ImageData == null || component.ImageData.Length == 0)
		{
			ApplyNewPreviewTexture(m_DefaultPreviewTexture, component);
		}
		else if (texture2D.LoadImage(component.ImageData, false))
		{
			ApplyNewPreviewTexture(texture2D, component);
		}
	}

	private void ApplyNewPreviewTexture(Texture2D tex, SingleWeaponCellUI map)
	{
		m_PreviewUI.AssignNewPreview(tex, map);
	}

	private void AddRuntimeListenersForCategoryToggle(Toggle categoryToggle)
	{
		categoryToggle.onValueChanged.AddListener((bool Hej) =>
		{
			OnMapCategoryToggle(categoryToggle);
		});
		List<EventTrigger.Entry> list = new List<EventTrigger.Entry>();
		EventTrigger.Entry entry = new EventTrigger.Entry();
		entry.eventID = EventTriggerType.Select;
		entry.callback.AddListener(OnSelect);
		list.Add(entry);
		entry = new EventTrigger.Entry();
		entry.eventID = EventTriggerType.Deselect;
		entry.callback.AddListener(OnDeselect);
		list.Add(entry);
		EventTrigger eventTrigger = categoryToggle.gameObject.AddComponent<EventTrigger>();
		eventTrigger.triggers = list;
	}

	private MapCategoryUI FindCategoryByName(string categoryName)
	{
		foreach (MapCategoryUI category in m_Categories)
		{
			if (category.CategoryName.ToLower() == categoryName.ToLower())
			{
				return category;
			}
		}
		Debug.LogError("No Category: " + categoryName + " Could be found..");
		return null;
	}

	private void AddNewMap(MapCategoryUI category, SingleMapUI newMap)
	{
		category.AddMapToCategory(newMap);
	}

	private void AddNewMap(MapWorldsEnum category, SingleMapUI newMap)
	{
		MapCategoryUI mapCategoryUI = FindCategoryByName(category.ToString());
		if (mapCategoryUI != null)
		{
			mapCategoryUI.AddMapToCategory(newMap);
		}
		else
		{
			Debug.LogError("Cant find category: " + category);
		}
	}

	private void AddNewCategory(MapCategoryUI newCategory)
	{
		m_Categories.Add(newCategory);
	}

	private void ToggleCategoryActive(string categoryName, bool on)
	{
		MapCategoryUI mapCategoryUI = FindCategoryByName(categoryName);
		mapCategoryUI.IsActive = on;
		Debug.Log("Category: " + categoryName + ((!on) ? "DEACTIVATED" : "ACTIVATED! "));
	}

	private void UpdateCategoryColor(Toggle categoryToggle)
	{
		Image component = categoryToggle.transform.parent.Find("Panel").GetComponent<Image>();
		component.color = ((!categoryToggle.isOn) ? m_CategoryDeactiveColor : m_CategoryActiveColor);
	}

	public void CustomMapsWasLoaded(List<WorkshopMapWrapper> loadedCustomLevels)
	{
		m_WorkshopMaps = loadedCustomLevels.ToArray();
		InjectCustomMaps();
		CheckMaxPages();
		try
		{
			UpdateUINavigation();
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to UpdateUINavigation after CustomMapsWasLoaded: " + ex.Message);
		}
	}

	private void InjectCustomMaps()
	{
		int num = m_WorkshopMaps.Length;
		MapWorldsEnum mapWorldsEnum = MapWorldsEnum.CustomOnline;
		for (int i = 0; i < num; i++)
		{
			WorkshopMapWrapper workshopMapWrapper = m_WorkshopMaps[i];
			bool isOn = true;
			if (m_HasSavedData)
			{
				try
				{
					int num2 = FindIndexOfMapIndex(mapWorldsEnum, workshopMapWrapper.PublishID.m_PublishedFileId.ToString());
					if (num2 > -1)
					{
						int num3 = FindSavedCategoryByName(mapWorldsEnum);
						if (num3 > -1)
						{
							isOn = m_Saved.SaveableCategories[num3].Actives[num2];
						}
					}
				}
				catch (Exception)
				{
					isOn = true;
				}
			}
			SingleMapUI newMap = new SingleMapUI(workshopMapWrapper, mapWorldsEnum.ToString(), isOn);
			AddNewMap(mapWorldsEnum, newMap);
		}
	}

	public SingleMapUI FindSingleMapByIndex(string mapIndex)
	{
		foreach (MapCategoryUI category in m_Categories)
		{
			foreach (SingleMapUI categoryMap in category.CategoryMaps)
			{
				if (categoryMap.MapIndex == mapIndex)
				{
					return categoryMap;
				}
			}
		}
		Debug.LogError("No Map with index: " + mapIndex + " Could be found..");
		return null;
	}

	public SingleMapUI GetRandomLevel(bool mustBeActive, List<string> blacklistArg = null)
	{
		List<SingleMapUI> list = new List<SingleMapUI>();
		List<string> blacklist = blacklistArg ?? new List<string>();
		List<MapCategoryUI> list2 = m_Categories.FindAll((MapCategoryUI Hej) => ((Hej.IsActive && mustBeActive) || !mustBeActive) && (!MatchmakingHandler.IsNetworkMatch || (MatchmakingHandler.IsNetworkMatch && Hej.CategoryName.ToLower() != MapWorldsEnum.CustomLocal.ToString().ToLower())));
		foreach (MapCategoryUI item in list2)
		{
			List<SingleMapUI> collection = item.CategoryMaps.FindAll((SingleMapUI map) => (map.IsLocallyActive || !mustBeActive) && !blacklist.Contains(map.MapIndex));
			list.AddRange(collection);
			Debug.Log("Searching category: " + item.CategoryName);
		}
		if (list.Count <= 0)
		{
			return GetRandomLevel(false, blacklist);
		}
		int num = UnityEngine.Random.Range(0, list.Count);
		if (!m_RandomOrderMaps)
		{
			if (m_LastPlayedMap == null)
			{
				num = 0;
			}
			else
			{
				int num2 = list.IndexOf(m_LastPlayedMap);
				if (num2 > -1)
				{
					num = num2 + 1;
					if (num >= list.Count)
					{
						num = 0;
					}
					Debug.Log("Ordered: New Index: " + num);
				}
				else
				{
					MapCategoryUI mapCategoryUI = FindCategoryByName(m_LastPlayedMap.CategoryName);
					int num3 = mapCategoryUI.CategoryMaps.IndexOf(m_LastPlayedMap);
					if (num3 < 0)
					{
						Debug.LogError("Last map is not included in the same category as the map? What...");
					}
					num3++;
					int num4 = 1000;
					int num5 = 0;
					while (true)
					{
						if (num3 >= mapCategoryUI.CategoryMaps.Count)
						{
							int categoryIndex = mapCategoryUI.Index + 1;
							if (categoryIndex >= m_Categories.Count)
							{
								categoryIndex = 0;
							}
							mapCategoryUI = m_Categories.Find((MapCategoryUI cat) => cat.Index == categoryIndex);
							num3 = 0;
							Debug.Log("Overflowing: switching category: INdex: " + categoryIndex + " : " + mapCategoryUI.CategoryName);
						}
						if (mapCategoryUI.CategoryMaps.Count == 0)
						{
							num3++;
							continue;
						}
						SingleMapUI singleMapUI = mapCategoryUI.CategoryMaps[num3];
						num3++;
						int num6 = list.IndexOf(singleMapUI);
						if (num6 > -1)
						{
							num = num6;
							break;
						}
						Debug.Log("Searching Map: " + singleMapUI.MapName);
						num5++;
						if (num5 < num4)
						{
							continue;
						}
						break;
					}
				}
			}
		}
		Debug.Log("Found map: " + list[num].MapName + " Cat: " + list[num].CategoryName);
		m_LastPlayedMap = list[num];
		return m_LastPlayedMap;
	}

	private void SelectUI(GameObject arg0)
	{
		RectTransform component = arg0.GetComponent<RectTransform>();
		RectTransform component2 = m_Selector.GetComponent<RectTransform>();
		float width = component.rect.width;
		Vector3 position = component.position;
		if (PauseManager.usedKeyboard)
		{
			m_Selector.position = new Vector3(position.x, -1000f, position.z);
		}
		else
		{
			m_Selector.position = new Vector3(position.x, position.y - 0.4f, position.z);
		}
		component2.SetRectWidth(new Vector2(width, component2.rect.height));
		Toggle t = arg0.GetComponent<Toggle>();
		foreach (MapCategoryUI category in m_Categories)
		{
			SingleMapUI singleMapUI = category.CategoryMaps.Find((SingleMapUI map) => map.MapToggle == t);
			if (singleMapUI != null)
			{
				m_LastSelectedMap = singleMapUI;
			}
		}
		Debug.Log("New Selected Map: " + Time.frameCount, t);
		if (!(arg0 == EventSystem.current.currentSelectedGameObject))
		{
			EventSystem.current.SetSelectedGameObject(null);
			EventSystem.current.SetSelectedGameObject(arg0);
			Debug.Log("Actually Selected Map: " + Time.frameCount, t);
		}
	}

	public void OnSingleMapToggle(Toggle mapToggle)
	{
		GameObject gameObject = mapToggle.gameObject;
		string mapIndex = mapToggle.GetComponent<SingleWeaponCellUI>().MapIndex;
		bool isOn = mapToggle.isOn;
		ToggleMapActive(mapIndex, isOn);
		UpdateSavedData();
	}

	private void ToggleMapActive(string mapIndex, bool isOn)
	{
		SingleMapUI singleMapUI = FindSingleMapByIndex(mapIndex);
		singleMapUI.IsLocallyActive = isOn;
		Debug.Log(string.Concat("Map: ", singleMapUI, (!isOn) ? "DEACTIVATED" : "ACTIVATED! "));
	}

	private void OnMapCategoryToggle(Toggle t)
	{
		GameObject gameObject = t.gameObject;
		string text = gameObject.GetComponentInChildren<TextMeshProUGUI>().text;
		bool isOn = gameObject.GetComponent<Toggle>().isOn;
		ToggleCategoryActive(text, isOn);
		UpdateCategoryColor(t);
		UpdateSavedData();
	}

	private void OnSelect(BaseEventData arg0)
	{
		Debug.Log("OnSelect");
		SelectUI(arg0.selectedObject);
	}

	private void OnDeselect(BaseEventData arg0)
	{
		Debug.Log("OnDeselect");
		RectTransform component = arg0.selectedObject.GetComponent<RectTransform>();
		Vector3 position = component.position;
		m_Selector.position = new Vector3(position.x, 1000f, position.z);
	}

	private void OnRandomOrderMapsToggleClicked(bool arg0)
	{
		m_RandomOrderMaps = arg0;
		UpdateSavedData();
	}

	private void OnRightArrowClicked()
	{
		IncrementIndex();
		IndexWasUpdated();
	}

	private void IndexWasUpdated()
	{
		Debug.Log("New Page Index: " + m_PageIndex);
		PopulatePage();
		UpdateUINavigation();
	}

	private void UpdateUINavigation()
	{
		if (m_Categories == null)
		{
			Debug.LogError("No map categories");
			return;
		}
		int count = m_Categories.Count;
		for (int i = 0; i < count; i++)
		{
			if (m_Categories[i] == null)
			{
				Debug.LogError("Null category");
				continue;
			}
			SingleMapUI[] mapsForPage = m_Categories[i].GetMapsForPage(m_PageIndex, m_MapsPerPage);
			if (mapsForPage.Length == 0)
			{
				continue;
			}
			Navigation navigation = m_Categories[i].CategoryToggle.navigation;
			navigation.selectOnRight = mapsForPage[0].MapToggle;
			m_Categories[i].CategoryToggle.navigation = navigation;
			int num = mapsForPage.Length;
			for (int j = 0; j < num; j++)
			{
				Navigation navigation2 = mapsForPage[j].MapToggle.navigation;
				int num2 = i + 1;
				if (num2 >= count)
				{
					num2 = 0;
				}
				SingleMapUI[] mapsForPage2 = m_Categories[num2].GetMapsForPage(m_PageIndex, m_MapsPerPage);
				if (mapsForPage2.Length > 0)
				{
					int num3 = Mathf.Clamp(j, 0, mapsForPage2.Length - 1);
					Toggle mapToggle = mapsForPage2[num3].MapToggle;
					navigation2.selectOnDown = mapToggle;
				}
				int num4 = i - 1;
				if (num4 < 0)
				{
					num4 = count - 1;
				}
				SingleMapUI[] mapsForPage3 = m_Categories[num4].GetMapsForPage(m_PageIndex, m_MapsPerPage);
				if (mapsForPage3.Length > 0)
				{
					int num5 = Mathf.Clamp(j, 0, mapsForPage3.Length - 1);
					Toggle mapToggle2 = mapsForPage3[num5].MapToggle;
					navigation2.selectOnUp = mapToggle2;
				}
				if (j == 0)
				{
					navigation2.selectOnLeft = m_Categories[i].CategoryToggle;
					if (num > 1)
					{
						navigation2.selectOnRight = mapsForPage[j + 1].MapToggle;
					}
					mapsForPage[j].MapToggle.navigation = navigation2;
					Debug.DrawLine(mapsForPage[j].MapToggle.transform.position, navigation2.selectOnLeft.transform.position, Color.red, 5f);
					continue;
				}
				navigation2.selectOnLeft = mapsForPage[j - 1].MapToggle;
				if (j + 1 < num)
				{
					navigation2.selectOnRight = mapsForPage[j + 1].MapToggle;
				}
				mapsForPage[j].MapToggle.navigation = navigation2;
				if ((bool)navigation2.selectOnLeft)
				{
					Debug.DrawLine(mapsForPage[j].MapToggle.transform.position, navigation2.selectOnLeft.transform.position, Color.red, 5f);
				}
				if ((bool)navigation2.selectOnRight)
				{
					Debug.DrawLine(mapsForPage[j].MapToggle.transform.position, navigation2.selectOnRight.transform.position, Color.green, 5f);
				}
				if ((bool)navigation2.selectOnDown)
				{
					Debug.DrawLine(mapsForPage[j].MapToggle.transform.position, navigation2.selectOnDown.transform.position, Color.blue, 5f);
				}
				if ((bool)navigation2.selectOnUp)
				{
					Debug.DrawLine(mapsForPage[j].MapToggle.transform.position, navigation2.selectOnUp.transform.position, Color.yellow, 5f);
				}
			}
			SingleMapUI[] array = mapsForPage;
		}
		if (m_LastSelectedMap != null)
		{
			MapCategoryUI mapCategoryUI = FindCategoryByName(m_LastSelectedMap.CategoryName);
			if (mapCategoryUI != null)
			{
				int num6 = mapCategoryUI.CategoryMaps.IndexOf(m_LastSelectedMap);
				int num7 = num6 % m_MapsPerPage;
				SingleMapUI[] mapsForPage4 = mapCategoryUI.GetMapsForPage(m_PageIndex, m_MapsPerPage);
				Toggle categoryToggle = mapCategoryUI.CategoryToggle;
				SelectUI(categoryToggle.gameObject);
			}
			else
			{
				Debug.LogError("Could not find previous category");
			}
		}
		else if (m_Categories.Count > 0)
		{
			Toggle categoryToggle2 = m_Categories[0].CategoryToggle;
		}
		else
		{
			Debug.LogError("Missing category");
		}
	}

	private void OnLeftArrowClicked()
	{
		DecrementIndex();
	}

	private void IncrementIndex()
	{
		if (++m_PageIndex >= m_MaxPages)
		{
			m_PageIndex = m_MaxPages - 1;
		}
		else
		{
			IndexWasUpdated();
		}
	}

	private void DecrementIndex()
	{
		if (--m_PageIndex < 0)
		{
			m_PageIndex = 0;
		}
		else
		{
			IndexWasUpdated();
		}
	}

	public int FindIndexOfMapIndex(int categoryIndex, string mapIndex)
	{
		SaveableCategory saveableCategory = m_Saved.SaveableCategories[categoryIndex];
		int num = saveableCategory.Indexes.Length;
		for (int i = 0; i < num; i++)
		{
			if (saveableCategory.Indexes[i] == mapIndex)
			{
				return i;
			}
		}
		return -1;
	}

	public int FindIndexOfMapIndex(MapWorldsEnum categoryEnum, string mapIndex)
	{
		int num = m_Saved.SaveableCategories.Length;
		for (int i = 0; i < num; i++)
		{
			if (m_Saved.SaveableCategories[i].CategoryName.ToLower() == categoryEnum.ToString().ToLower())
			{
				return FindIndexOfMapIndex(i, mapIndex);
			}
		}
		return -1;
	}

	public int FindSavedCategoryByName(MapWorldsEnum categoryEnum)
	{
		int num = m_Saved.SaveableCategories.Length;
		for (int i = 0; i < num; i++)
		{
			if (m_Saved.SaveableCategories[i].CategoryName.ToLower() == categoryEnum.ToString().ToLower())
			{
				return i;
			}
		}
		Debug.LogError("Cant Find Saveable Category: " + categoryEnum);
		return -1;
	}

	public void DisableCategory(MapWorldsEnum customLocal)
	{
		MapCategoryUI mapCategoryUI = m_Categories.Find((MapCategoryUI Category) => Category.CategoryName.ToLower() == customLocal.ToString().ToLower());
		mapCategoryUI.IsActive = false;
	}
}
