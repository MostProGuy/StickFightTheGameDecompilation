using UnityEngine;

namespace LevelEditor
{
	public class InterfaceManager : MonoBehaviour
	{
		[SerializeField]
		private GameObject m_UIParent;

		private bool m_IsOverUI;

		private Camera m_MainCamera;

		private static InterfaceManager _instance;

		public static InterfaceManager Instance
		{
			get
			{
				return _instance;
			}
		}

		private void Awake()
		{
			_instance = this;
			m_MainCamera = Camera.main;
			Time.timeScale = 1f;
		}

		public void Destruct()
		{
			_instance = null;
		}

		public void IsOverOtherObject()
		{
		}

		public bool IsOutsideOfEditorArea()
		{
			Vector3 vector = m_MainCamera.ScreenToViewportPoint(Input.mousePosition);
			if (vector.x < 0f)
			{
				return true;
			}
			if (vector.y < 0f)
			{
				return true;
			}
			return false;
		}

		public void HideAllUI()
		{
			m_UIParent.SetActive(false);
		}

		public void ShowAllUI()
		{
			m_UIParent.SetActive(true);
		}
	}
}
