using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LevelEditor
{
	public class EditorUIBase : MonoBehaviour
	{
		public enum WindowOpen
		{
			None = 0,
			Save = 1,
			Load = 2,
			Upload = 3,
			Clear = 4,
			GunMenu = 5,
			Autosave = 6
		}

		private static Dictionary<WindowOpen, Button> UI_BUTTONS = new Dictionary<WindowOpen, Button>();

		protected WindowOpen m_Window;

		private WindowOpen m_CurrentWindowOpen;

		protected void Validate(Action func, WindowOpen asker = WindowOpen.None)
		{
			if (LevelEditorInputManager.CanUseMouse)
			{
				m_CurrentWindowOpen = asker;
				func();
			}
			else if (m_CurrentWindowOpen != WindowOpen.None && m_CurrentWindowOpen == asker)
			{
				func();
			}
			UpdateUI();
		}

		protected virtual Button[] GetAllButtons()
		{
			return new Button[0];
		}

		private void UpdateUI()
		{
			Color white = Color.white;
			Color black = Color.black;
			bool flag = true;
			foreach (KeyValuePair<WindowOpen, Button> uI_BUTTON in UI_BUTTONS)
			{
				if (!LevelEditorInputManager.CanUseMouse)
				{
					if (uI_BUTTON.Key != m_CurrentWindowOpen)
					{
						flag = false;
					}
					else
					{
						flag = true;
					}
				}
			}
		}

		protected void AddNewButton(Button b, WindowOpen type)
		{
			if (UI_BUTTONS.ContainsKey(type))
			{
				UI_BUTTONS.Remove(type);
			}
			UI_BUTTONS.Add(type, b);
		}
	}
}
