using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LevelEditor
{
	public class EditorSystemUI : EditorUIBase
	{
		[SerializeField]
		private Button m_QuitButton;

		private static EditorLoadSave m_LoadSave;

		private void Awake()
		{
			InitListeners();
		}

		private void Start()
		{
			m_LoadSave = EditorLoadSave.Instance;
		}

		private void InitListeners()
		{
			m_QuitButton.onClick.AddListener(() =>
			{
				Validate(OnQuitClicked);
			});
		}

		private void OnQuitClicked()
		{
			Action action = () =>
			{
				AudioListener.pause = false;
				UnityEngine.Object.FindObjectOfType<LevelCreator>().Destruct();
				PauseManager.isPaused = false;
				Time.timeScale = 1f;
				TimeHandler.managerTime = 1f;
				TimeHandler.pauseTime = 1f;
				DisableIfPlayed.hasPlayed = true;
				SceneManager.LoadScene("MainScene");
			};
			Action noAction = () =>
			{
			};
			if (m_LoadSave.HasbeenTouched)
			{
				DialougePanelUI.Instance.GiveChoice("Do You Want To Quit The Editor? Unsaved progress will be lost", action, noAction);
			}
			else
			{
				action();
			}
		}
	}
}
