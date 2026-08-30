using System;
using UnityEngine;
using UnityEngine.UI;

namespace LevelEditor
{
	public class EditorWeaponUI : EditorUIBase
	{
		[SerializeField]
		private GameObject m_GunMenu;

		[SerializeField]
		private GameObject m_Cell;

		private Transform m_Panel;

		private SelectableWeapon[] m_AllTheGuns;

		public static int CurrentSelectedWeapon { get; set; }

		private void Awake()
		{
			m_Panel = m_GunMenu.transform.Find("Panel");
		}

		private void Start()
		{
			m_AllTheGuns = WeaponSelectionHandler.SelectableWeapons.ToArray();
			GameObject toolButton;
			if (PopulateToolButtons.GetToolButtonFromName("weapon", out toolButton))
			{
				Button component = toolButton.GetComponent<Button>();
				SendGunButton(component);
				Populate();
				return;
			}
			throw new Exception("Could not find gun button!");
		}

		private void SendGunButton(Button gun)
		{
			gun.onClick.AddListener(() =>
			{
				Validate(OpenGunMenu, WindowOpen.GunMenu);
			});
		}

		private void CloseGunMenu()
		{
			m_GunMenu.SetActive(false);
			LevelEditorInputManager.SetNewMouseInputState(true);
		}

		private void OpenGunMenu()
		{
			if (m_GunMenu.activeInHierarchy)
			{
				CloseGunMenu();
				return;
			}
			LevelEditorInputManager.SetNewMouseInputState(false);
			m_GunMenu.SetActive(true);
		}

		private void Populate()
		{
			int num = m_AllTheGuns.Length;
			for (int i = 0; i < num; i++)
			{
				SelectableWeapon selectableWeapon = m_AllTheGuns[i];
				GameObject gameObject = UnityEngine.Object.Instantiate(m_Cell);
				gameObject.FetchComponent<GunButtonUI>().Init(selectableWeapon.WeaponName, selectableWeapon.Index, OnGunSelected);
				gameObject.transform.SetParent(m_Panel, false);
				gameObject.SetActive(true);
			}
		}

		private void OnGunSelected(string weaponName, int index)
		{
			Debug.Log("Gun Selected! " + weaponName + " INdex: " + index);
			CurrentSelectedWeapon = index;
			CloseGunMenu();
			UnityEngine.Object.FindObjectOfType<LevelCreator>().OnWeaponBrushChanged();
		}
	}
}
