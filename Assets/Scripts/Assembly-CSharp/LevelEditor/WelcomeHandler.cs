using UnityEngine;
using UnityEngine.UI;

namespace LevelEditor
{
	public class WelcomeHandler : MonoBehaviour
	{
		private const string WELCOME_KEY = "PlayedEditor";

		private void Awake()
		{
			Close();
		}

		private void Close()
		{
			Object.FindObjectOfType<LevelCreator>().Init();
			base.gameObject.SetActive(false);
			PlayerPrefs.SetInt("PlayedEditor", 1);
		}

		private void AssignListener()
		{
			Button componentInChildren = GetComponentInChildren<Button>(true);
			componentInChildren.onClick.AddListener(Close);
		}
	}
}
