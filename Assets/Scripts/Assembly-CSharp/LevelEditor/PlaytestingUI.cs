using UnityEngine;
using UnityEngine.UI;

namespace LevelEditor
{
	public class PlaytestingUI : EditorUIBase
	{
		[SerializeField]
		private Button m_PlaytestingButton;

		private void Start()
		{
			m_PlaytestingButton.onClick.AddListener(() =>
			{
				Validate(OnPlayTestingButtonPressed);
			});
		}

		private void Update()
		{
			if (LevelEditorInputManager.DidPressEscape())
			{
				WorkshopStateHandler.ExitPlayMode();
			}
			if (LevelEditorInputManager.DidPressSpace())
			{
				Validate(WorkshopStateHandler.StartPlayMode);
			}
		}

		public void OnPlayTestingButtonPressed()
		{
			Debug.Log("Playtesting pressed!");
			WorkshopStateHandler.StartPlayMode();
		}
	}
}
