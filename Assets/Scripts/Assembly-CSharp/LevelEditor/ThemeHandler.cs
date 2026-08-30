using UnityEngine;

namespace LevelEditor
{
	public class ThemeHandler : MonoBehaviour
	{
		private Transform m_MapObject;

		private GameObject m_BackGroundObject;

		private static LevelManager m_LevelManager;

		private static ResourcesManager m_ResourcesManager;

		private static ThemeHandler _instance;

		public static ThemeHandler Instance
		{
			get
			{
				return _instance;
			}
		}

		private void Awake()
		{
			_instance = this;
		}

		private void Start()
		{
			m_MapObject = GameObject.Find("Map").transform;
			InitReferences();
			InitBackground();
		}

		private void InitReferences()
		{
			m_LevelManager = LevelManager.Instance;
			m_ResourcesManager = ResourcesManager.Instance;
		}

		private void InitBackground()
		{
			int theme = m_LevelManager.CurrentMapSettings.Theme;
			GameObject background = m_ResourcesManager.GetBackground(theme);
			if (m_BackGroundObject != null)
			{
				Object.Destroy(m_BackGroundObject);
			}
			m_BackGroundObject = Object.Instantiate(background, m_MapObject);
			m_BackGroundObject.transform.localPosition = new Vector3(10f, 0f, 0f);
		}

		public void SetNewBackground(int index)
		{
			m_LevelManager.SetNewMapTheme(index);
			InitBackground();
		}

		public void ChangeBackground(int index)
		{
			m_LevelManager.SetNewMapTheme(index);
			m_LevelManager.GenerateNewVegetation();
			InitBackground();
		}

		public void GenerateNewThemeProps()
		{
			m_LevelManager.GenerateNewVegetation();
		}
	}
}
