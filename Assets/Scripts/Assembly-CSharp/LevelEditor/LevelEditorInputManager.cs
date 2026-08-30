using System;
using UnityEngine;

namespace LevelEditor
{
	public class LevelEditorInputManager
	{
		private static bool m_CanUseKeyboard = true;

		private static bool m_CanUseMouse = true;

		private static readonly LevelEditorInputManager _instance = new LevelEditorInputManager();

		private static Action<bool, bool> m_OnInputStateChanged;

		public static bool CanUseMouse
		{
			get
			{
				return m_CanUseMouse;
			}
		}

		public static bool CanUseKeyBoard
		{
			get
			{
				return m_CanUseKeyboard;
			}
		}

		public static LevelEditorInputManager Instance
		{
			get
			{
				return _instance;
			}
		}

		public void Destruct()
		{
			m_OnInputStateChanged = null;
		}

		public static void AddOnInputStateChangedAction(Action<bool, bool> a)
		{
			m_OnInputStateChanged = (Action<bool, bool>)Delegate.Combine(m_OnInputStateChanged, a);
		}

		public static void SetNewInputState(bool canUseMouse, bool canUseKeyBoard)
		{
			m_CanUseMouse = canUseMouse;
			m_CanUseKeyboard = canUseKeyBoard;
			if (m_OnInputStateChanged != null)
			{
				m_OnInputStateChanged(m_CanUseMouse, m_CanUseKeyboard);
			}
		}

		public static void SetNewKeyboardInputState(bool canUseKeyBoard)
		{
			m_CanUseKeyboard = canUseKeyBoard;
			if (m_OnInputStateChanged != null)
			{
				m_OnInputStateChanged(m_CanUseMouse, m_CanUseKeyboard);
			}
		}

		public static void SetNewMouseInputState(bool canUseMouse)
		{
			m_CanUseMouse = canUseMouse;
			if (m_OnInputStateChanged != null)
			{
				m_OnInputStateChanged(m_CanUseMouse, m_CanUseKeyboard);
			}
		}

		public static bool DidPressSpace()
		{
			return Input.GetKeyDown(KeyCode.Space) && m_CanUseKeyboard;
		}

		public static bool DidPressEscape()
		{
			return Input.GetKeyDown(KeyCode.Escape);
		}

		public static bool DidPressFlip()
		{
			return Input.GetKeyDown(KeyCode.F) && m_CanUseMouse;
		}

		public static bool DidPressRotate()
		{
			return Input.GetKeyDown(KeyCode.R) && m_CanUseMouse;
		}

		public static bool DidReleasedMouse()
		{
			return Input.GetMouseButtonUp(0) && m_CanUseMouse;
		}

		public static bool IsHoldingPlace()
		{
			return Input.GetMouseButton(0) && m_CanUseMouse;
		}

		public static bool DidClickPlace()
		{
			return Input.GetMouseButtonDown(0) && m_CanUseMouse;
		}

		public static bool DidClickDelete()
		{
			return Input.GetMouseButtonDown(1) && m_CanUseMouse;
		}

		public static bool IsHoldingDelete()
		{
			return Input.GetMouseButton(1) && m_CanUseMouse;
		}
	}
}
