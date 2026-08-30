using TMPro;
using UnityEngine;

namespace LevelEditor
{
	public class PopulateToolButtons : MonoBehaviour
	{
		[SerializeField]
		private GameObject m_ToolButtonPrefab;

		private static GameObject[] m_ToolButtons;

		private void Start()
		{
			Populate();
		}

		private void Populate()
		{
			GameObject[] placeableObjects = ResourcesManager.Instance.PlaceableObjects;
			int num = placeableObjects.Length;
			m_ToolButtons = new GameObject[num];
			for (int i = 0; i < num; i++)
			{
				GameObject gameObject = placeableObjects[i];
				string text = gameObject.name;
				GameObject gameObject2 = Object.Instantiate(m_ToolButtonPrefab, base.transform);
				gameObject2.GetComponentInChildren<ToolButtonUI>().Init(text);
				gameObject2.GetComponentInChildren<TextMeshProUGUI>().text = text.ToUpper();
				gameObject2.SetActive(true);
				m_ToolButtons[i] = gameObject2;
			}
		}

		public static bool GetToolButtonFromName(string buttonName, out GameObject toolButton)
		{
			GameObject[] toolButtons = m_ToolButtons;
			foreach (GameObject gameObject in toolButtons)
			{
				Debug.Log("Seraching: " + gameObject.name);
				if (gameObject.name.ToLower().Contains(buttonName.ToLower()))
				{
					toolButton = gameObject;
					return true;
				}
			}
			toolButton = null;
			return false;
		}
	}
}
