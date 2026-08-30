using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LevelEditor
{
	public class ThemeButtonsUI : EditorUIBase
	{
		[SerializeField]
		private Button m_ThemeButton;

		[SerializeField]
		private Button m_GoLeftButton;

		[SerializeField]
		private Button m_GoRightButton;

		private string[] m_Themes;

		private int m_ThemeIndex;

		private ThemeHandler m_ThemeHandler;

		private Action m_OnThemeChangedAction;

		private static ThemeButtonsUI _instance;

		public static ThemeButtonsUI Instance
		{
			get
			{
				return _instance;
			}
		}

		private void Awake()
		{
			_instance = this;
			InitListeners();
		}

		private void Start()
		{
			m_ThemeHandler = ThemeHandler.Instance;
			InitThemes();
			UpdateThemeText();
		}

		public void AddOnThemeChangedAction(Action a)
		{
			m_OnThemeChangedAction = (Action)Delegate.Combine(m_OnThemeChangedAction, a);
		}

		private void InitListeners()
		{
			AddPointerEventTrigger(m_ThemeButton.gameObject, () =>
			{
				Validate(OnThemeButtonClicked);
			});
			AddPointerEventTrigger(m_GoLeftButton.gameObject, () =>
			{
				Validate(OnLeftButtonClicked);
			});
			AddPointerEventTrigger(m_GoRightButton.gameObject, () =>
			{
				Validate(OnRightButtonClicked);
			});
		}

		private void AddPointerEventTrigger(GameObject obj, Action func)
		{
			EventTrigger eventTrigger = obj.AddComponent<EventTrigger>();
			EventTrigger.Entry entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerClick;
			entry.callback.AddListener((BaseEventData hej) =>
			{
				func();
			});
			eventTrigger.triggers.Add(entry);
		}

		private void InitThemes()
		{
			ResourcesManager instance = ResourcesManager.Instance;
			int numberOfThemes = instance.GetNumberOfThemes();
			m_Themes = new string[numberOfThemes];
			for (int i = 0; i < numberOfThemes; i++)
			{
				m_Themes[i] = instance.GetThemeName(i);
			}
		}

		public void SetNewTheme(int index)
		{
			m_ThemeIndex = index;
			UpdateThemeText();
			m_ThemeHandler.SetNewBackground(m_ThemeIndex);
		}

		private void UpdateThemeText()
		{
			string text = m_Themes[m_ThemeIndex];
			m_ThemeButton.GetComponentInChildren<TextMeshProUGUI>().text = text;
		}

		public void OnThemeButtonClicked()
		{
			m_ThemeHandler.GenerateNewThemeProps();
		}

		public void OnRightButtonClicked()
		{
			IncrementIndex();
			UpdateThemeText();
			SetNewTheme();
		}

		public void OnLeftButtonClicked()
		{
			DecrementIndex();
			UpdateThemeText();
			SetNewTheme();
		}

		private void IncrementIndex()
		{
			m_ThemeIndex = ++m_ThemeIndex % m_Themes.Length;
		}

		private void DecrementIndex()
		{
			m_ThemeIndex = ((m_ThemeIndex - 1 >= 0) ? (m_ThemeIndex - 1) : (m_Themes.Length - 1));
		}

		private void SetNewTheme()
		{
			m_ThemeHandler.ChangeBackground(m_ThemeIndex);
			if (m_OnThemeChangedAction != null)
			{
				m_OnThemeChangedAction();
			}
		}
	}
}
