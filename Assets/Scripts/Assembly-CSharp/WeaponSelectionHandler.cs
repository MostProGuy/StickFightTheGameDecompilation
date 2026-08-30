using System;
using System.Collections.Generic;
using LevelEditor;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WeaponSelectionHandler : MonoBehaviour
{
	public class SavedWeapons
	{
		public bool[] Actives;
	}

	public class WeaponCategoryUI
	{
		public bool IsActive;

		public string Category;

		public byte Index;

		public WeaponCategoryUI(string categoryName, byte index)
		{
			Category = categoryName;
			Index = index;
			IsActive = true;
		}
	}

	[Serializable]
	public class SingleWeaponUI
	{
		public bool IsLocallyActive;

		public string WeaponName;

		public string Category;

		public byte WeaponIndex;

		public byte Rarity;

		public SingleWeaponUI(string weaponName, string category, byte index, byte rarity)
		{
			WeaponName = weaponName;
			Category = category;
			WeaponIndex = index;
			Rarity = rarity;
			IsLocallyActive = true;
		}
	}

	[SerializeField]
	private Transform m_Selector;

	private GameObject[] m_WeaponObjects;

	private List<SingleWeaponUI> m_Weapons = new List<SingleWeaponUI>();

	private List<WeaponCategoryUI> m_Categories = new List<WeaponCategoryUI>();

	private GameObject m_PlayerObject;

	private SavedWeapons m_Saved = new SavedWeapons();

	private bool m_HasSavedData;

	private int m_NumberOfWeapons;

	private int m_NumberOfCategories;

	private List<byte> m_WeaponRaritiesArray;

	[SerializeField]
	private Color m_CategoryActiveColor;

	[SerializeField]
	private Color m_CategoryDeactiveColor;

	public static List<SelectableWeapon> SelectableWeapons { get; private set; }

	private void Start()
	{
		m_PlayerObject = ResourcesManager.Instance.CharacterObject;
		GetSavedData();
		InitWeaponList();
		UpdateSavedData();
		GenerateWeaponRarityArray();
		FillSelectableWeapons();
	}

	public static SelectableWeapon GetSelectableWeaponByIndex(int index)
	{
		foreach (SelectableWeapon selectableWeapon in SelectableWeapons)
		{
			if (selectableWeapon.Index == index)
			{
				return selectableWeapon;
			}
		}
		throw new Exception("Cant find weapon with index: " + index);
	}

	private void FillSelectableWeapons()
	{
		SelectableWeapons = new List<SelectableWeapon>();
		foreach (SingleWeaponUI weapon in m_Weapons)
		{
			string weaponName = weapon.WeaponName;
			int weaponIndex = weapon.WeaponIndex;
			GameObject pickup = m_WeaponObjects[weaponIndex];
			SelectableWeapon item = new SelectableWeapon(weaponIndex, weaponName, pickup);
			SelectableWeapons.Add(item);
		}
	}

	private void GetSavedData()
	{
		string text = PlayerPrefs.GetString("SavedWeapons", string.Empty);
		if (text == string.Empty)
		{
			m_HasSavedData = false;
			return;
		}
		m_HasSavedData = true;
		Debug.Log("Loaded: " + text);
		m_Saved = (SavedWeapons)JsonUtility.FromJson(text, typeof(SavedWeapons));
	}

	private void UpdateSavedData()
	{
		m_Saved.Actives = new bool[m_NumberOfWeapons + m_NumberOfCategories];
		foreach (SingleWeaponUI weapon in m_Weapons)
		{
			m_Saved.Actives[weapon.WeaponIndex] = weapon.IsLocallyActive;
		}
		foreach (WeaponCategoryUI category in m_Categories)
		{
			m_Saved.Actives[m_NumberOfWeapons + category.Index] = category.IsActive;
		}
		string text = JsonUtility.ToJson(m_Saved);
		PlayerPrefs.SetString("SavedWeapons", text);
		Debug.Log("SAVED WEAPONS: Nr Of Elements: " + m_Saved.Actives.Length + " : " + text);
	}

	private void InitWeaponList()
	{
		WeaponCategoryTAG[] componentsInChildren = base.transform.GetComponentsInChildren<WeaponCategoryTAG>();
		byte index = 0;
		m_NumberOfWeapons = m_PlayerObject.transform.Find("Weapons").childCount;
		m_NumberOfCategories = componentsInChildren.Length;
		m_WeaponObjects = new GameObject[m_NumberOfWeapons];
		byte b = 0;
		WeaponCategoryTAG[] array = componentsInChildren;
		foreach (WeaponCategoryTAG weaponCategoryTAG in array)
		{
			string text = weaponCategoryTAG.name;
			WeaponCategoryUI weaponCategoryUI = new WeaponCategoryUI(text, b);
			AddNewCategory(weaponCategoryUI);
			Toggle categoryToggle = weaponCategoryTAG.transform.Find("Toggle").GetComponent<Toggle>();
			if (m_HasSavedData)
			{
				try
				{
					categoryToggle.isOn = m_Saved.Actives[m_NumberOfWeapons + b];
				}
				catch (IndexOutOfRangeException)
				{
					categoryToggle.isOn = true;
				}
				weaponCategoryUI.IsActive = categoryToggle.isOn;
				UpdateCategoryColor(categoryToggle);
			}
			categoryToggle.onValueChanged.AddListener((bool Hej) =>
			{
				OnWeaponCategoryToggle(categoryToggle);
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
			b++;
			Transform transform = weaponCategoryTAG.transform.Find("Grid");
			Toggle[] componentsInChildren2 = transform.GetComponentsInChildren<Toggle>();
			foreach (Toggle toggle in componentsInChildren2)
			{
				string text2 = toggle.GetComponentInChildren<TextMeshProUGUI>().text;
				Weapon weaponObjectFromPlayer = GetWeaponObjectFromPlayer(text2, out index);
				m_WeaponObjects[index] = weaponObjectFromPlayer.weaponDrop;
				byte rarity = weaponObjectFromPlayer.Rarity;
				SingleWeaponUI singleWeaponUI = new SingleWeaponUI(text2, text, index, rarity);
				AddNewWeapon(singleWeaponUI);
				if (m_HasSavedData)
				{
					toggle.isOn = m_Saved.Actives[index];
					singleWeaponUI.IsLocallyActive = toggle.isOn;
				}
				else
				{
					singleWeaponUI.IsLocallyActive = toggle.isOn;
				}
				toggle.onValueChanged.AddListener((bool Hej) =>
				{
					OnSingleWeaponToggle(toggle);
				});
				list = new List<EventTrigger.Entry>();
				entry = new EventTrigger.Entry();
				entry.eventID = EventTriggerType.Select;
				entry.callback.AddListener(OnSelect);
				list.Add(entry);
				entry = new EventTrigger.Entry();
				entry.eventID = EventTriggerType.Deselect;
				entry.callback.AddListener(OnDeselect);
				list.Add(entry);
				eventTrigger = toggle.gameObject.AddComponent<EventTrigger>();
				eventTrigger.triggers = list;
			}
		}
	}

	private bool GetActiveStateForWeapon(SingleWeaponUI w)
	{
		WeaponCategoryUI weaponCategoryUI = FindCategoryByName(w.Category);
		return weaponCategoryUI.IsActive && w.IsLocallyActive;
	}

	private void OnSelect(BaseEventData arg0)
	{
		Debug.Log("OnSelect");
		RectTransform component = arg0.selectedObject.GetComponent<RectTransform>();
		RectTransform component2 = m_Selector.GetComponent<RectTransform>();
		float width = component.rect.width;
		Vector3 position = component.position;
		m_Selector.position = new Vector3(position.x, position.y - 0.65f, position.z);
		component2.SetRectWidth(new Vector2(width, component2.rect.height));
	}

	private void OnDeselect(BaseEventData arg0)
	{
		Debug.Log("OnDeselect");
		RectTransform component = arg0.selectedObject.GetComponent<RectTransform>();
		Vector3 position = component.position;
		m_Selector.position = new Vector3(position.x, 1000f, position.z);
	}

	private Weapon GetWeaponObjectFromPlayer(string weaponName, out byte index)
	{
		Transform transform = m_PlayerObject.transform.Find("Weapons");
		int childCount = transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Transform child = transform.GetChild(i);
			string[] array = child.name.Split(' ');
			string text = child.name.Substring(array[0].Length);
			text = text.Trim().Replace(" ", string.Empty).ToLower();
			string value = weaponName.Trim().Replace(" ", string.Empty).ToLower();
			if (text.Equals(value))
			{
				index = byte.Parse(array[0]);
				return child.GetComponent<Weapon>();
			}
		}
		index = 0;
		Debug.LogError("ERROR: No Weapon: " + weaponName + " Could be found on the player!");
		return null;
	}

	private void AddNewWeapon(SingleWeaponUI w)
	{
		if (Application.isEditor)
		{
			Debug.Log("Adding New Weapon: " + w.WeaponName + " Category: " + w.Category);
		}
		m_Weapons.Add(w);
	}

	private void AddNewCategory(WeaponCategoryUI c)
	{
		if (Application.isEditor)
		{
			Debug.Log("Adding New Category: " + c.Category);
		}
		m_Categories.Add(c);
	}

	public WeaponCategoryUI FindCategoryByName(string categoryName)
	{
		foreach (WeaponCategoryUI category in m_Categories)
		{
			if (category.Category.ToLower() == categoryName.ToLower())
			{
				return category;
			}
		}
		Debug.LogError("No Category: " + categoryName + " Could be found..");
		return null;
	}

	private SingleWeaponUI FindWeaponByName(string weaponName)
	{
		foreach (SingleWeaponUI weapon in m_Weapons)
		{
			if (weapon.WeaponName.ToLower() == weaponName.ToLower())
			{
				return weapon;
			}
		}
		Debug.LogError("No Weapon: " + weaponName + " Could be found..");
		return null;
	}

	private void ToggleCategoryActive(string categoryName, bool isOn)
	{
		WeaponCategoryUI weaponCategoryUI = FindCategoryByName(categoryName);
		weaponCategoryUI.IsActive = isOn;
		Debug.Log("Category: " + categoryName + ((!isOn) ? "DEACTIVATED" : "ACTIVATED! "));
	}

	private void ToggleWeaponActive(string weaponName, bool isOn)
	{
		SingleWeaponUI singleWeaponUI = FindWeaponByName(weaponName);
		singleWeaponUI.IsLocallyActive = isOn;
		Debug.Log("Weapon: " + weaponName + ((!isOn) ? "DEACTIVATED" : "ACTIVATED! "));
	}

	public GameObject GetWeaponByIndex(int index)
	{
		return m_WeaponObjects[index];
	}

	private void GenerateWeaponRarityArray()
	{
		SingleWeaponUI[] array = m_Weapons.FindAll((SingleWeaponUI w) => GetActiveStateForWeapon(w)).ToArray();
		m_WeaponRaritiesArray = new List<byte>();
		int num = array.Length;
		int num2 = 0;
		for (num2 = 0; num2 < num; num2++)
		{
			SingleWeaponUI singleWeaponUI = array[num2];
			byte rarity = singleWeaponUI.Rarity;
			for (int num3 = 0; num3 < rarity; num3++)
			{
				m_WeaponRaritiesArray.Add(singleWeaponUI.WeaponIndex);
			}
		}
		Debug.Log("generated WeaponRarity, Count: " + m_WeaponRaritiesArray.Count);
	}

	public int GetRandomWeaponIndex(bool mustBeActive, out GameObject weaponObject)
	{
		if (m_WeaponRaritiesArray.Count <= 0)
		{
			GenerateWeaponRarityArray();
			if (m_WeaponRaritiesArray.Count <= 0)
			{
				weaponObject = null;
				return -1;
			}
		}
		int index = UnityEngine.Random.Range(0, m_WeaponRaritiesArray.Count);
		byte b = m_WeaponRaritiesArray[index];
		weaponObject = m_WeaponObjects[b];
		m_WeaponRaritiesArray.RemoveAt(index);
		return b;
	}

	private void UpdateCategoryColor(Toggle categoryToggle)
	{
		Image component = categoryToggle.transform.parent.Find("Panel").GetComponent<Image>();
		component.color = ((!categoryToggle.isOn) ? m_CategoryDeactiveColor : m_CategoryActiveColor);
	}

	public void OnSingleWeaponToggle(Toggle weaponToggle)
	{
		GameObject gameObject = weaponToggle.gameObject;
		string text = gameObject.GetComponentInChildren<TextMeshProUGUI>().text;
		bool isOn = gameObject.GetComponent<Toggle>().isOn;
		ToggleWeaponActive(text, isOn);
		GenerateWeaponRarityArray();
		UpdateSavedData();
	}

	public void OnWeaponCategoryToggle(Toggle weaponToggle)
	{
		GameObject gameObject = weaponToggle.gameObject;
		string text = gameObject.GetComponentInChildren<TextMeshProUGUI>().text;
		bool isOn = gameObject.GetComponent<Toggle>().isOn;
		ToggleCategoryActive(text, isOn);
		GenerateWeaponRarityArray();
		UpdateSavedData();
		UpdateCategoryColor(weaponToggle);
	}
}
