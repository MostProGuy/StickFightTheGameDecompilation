using System;
using UnityEngine;

namespace LevelEditor
{
	public class LevelToolsHandler
	{
		public enum ToolState
		{
			Placing = 0,
			Dragging = 1,
			WeaponPlacing = 2
		}

		private ToolState mToolState = ToolState.Dragging;

		private bool m_IsMirroring;

		private bool m_IsMirroringRotation;

		private bool m_IsSnapping;

		private Action m_OnMirrorChangedAction;

		private Action m_OnSnapChangedAction;

		private Action<ToolState> m_OnToolChanged;

		private static readonly LevelToolsHandler _instance = new LevelToolsHandler();

		public ToolState CurrentToolState
		{
			get
			{
				return mToolState;
			}
		}

		public bool IsMirroring
		{
			get
			{
				return m_IsMirroring;
			}
		}

		public bool IsMirroringRotation
		{
			get
			{
				return m_IsMirroringRotation;
			}
		}

		public bool IsSnapping
		{
			get
			{
				return m_IsSnapping;
			}
		}

		public static LevelToolsHandler Instance
		{
			get
			{
				return _instance;
			}
		}

		public void ClearActions()
		{
			m_OnMirrorChangedAction = null;
			m_OnSnapChangedAction = null;
			m_OnToolChanged = null;
		}

		public void AddOnMirrorAction(Action onMirror)
		{
			m_OnMirrorChangedAction = (Action)Delegate.Combine(m_OnMirrorChangedAction, onMirror);
		}

		public void AddOnSnapAction(Action onSnap)
		{
			m_OnSnapChangedAction = (Action)Delegate.Combine(m_OnSnapChangedAction, onSnap);
		}

		public void AddOnToolAction(Action<ToolState> onTool)
		{
			m_OnToolChanged = (Action<ToolState>)Delegate.Combine(m_OnToolChanged, onTool);
		}

		public static void SetNewToolState(ToolState newState)
		{
			if (_instance.mToolState != newState)
			{
				if (_instance.m_OnToolChanged != null)
				{
					_instance.m_OnToolChanged(_instance.mToolState);
				}
				_instance.mToolState = newState;
				Debug.Log("New toolstate: " + newState);
			}
		}

		public static void SetNewMirrorState(bool isMirroring)
		{
			if (isMirroring != _instance.m_IsMirroring)
			{
				_instance.m_IsMirroring = isMirroring;
				Debug.Log("Setting New Mirror State: " + isMirroring);
				if (_instance.m_OnMirrorChangedAction != null)
				{
					_instance.m_OnMirrorChangedAction();
				}
			}
		}

		public static void SetNewMirrorRotationState(bool isMirroring)
		{
			_instance.m_IsMirroringRotation = isMirroring;
		}

		public static void SetNewSnapState(bool isSnapping)
		{
			if (isSnapping != _instance.m_IsSnapping)
			{
				_instance.m_IsSnapping = isSnapping;
				Debug.Log("Setting New SNap State: " + isSnapping);
				if (_instance.m_OnSnapChangedAction != null)
				{
					_instance.m_OnSnapChangedAction();
				}
			}
		}

		public void Destruct()
		{
			m_IsSnapping = false;
			m_IsMirroring = false;
		}
	}
}
