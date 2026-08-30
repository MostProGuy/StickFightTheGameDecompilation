using UnityEngine;

namespace LevelEditor
{
	public class WorkshopStateHandler
	{
		private static bool mIsPlaytestMode;

		public static bool IsPlayTestingMode
		{
			get
			{
				return mIsPlaytestMode;
			}
		}

		public static void StartPlayMode()
		{
			if (!mIsPlaytestMode)
			{
				Debug.Log("Entering Playmode!");
				mIsPlaytestMode = true;
				LevelCreator.Instance.OnPlayTestStarted();
				LevelEditorInputManager.SetNewInputState(false, false);
				EditorLoadSave.Instance.SaveLevel("temp");
				EditorLoadSave.Instance.LoadLevel("temp", false, string.Empty);
				InterfaceManager.Instance.HideAllUI();
				EditorCameraHandler.Instance.FillScreen();
				WorkshopLevelManager.InitCurrentLoadedLevel(true);
				MapSizeHandler.Instance.mapSizeFrame.root.gameObject.SetActive(false);
			}
		}

		public static void ExitPlayMode()
		{
			if (mIsPlaytestMode)
			{
				Debug.Log("Exiting Playmode!");
				InterfaceManager.Instance.ShowAllUI();
				EditorCameraHandler.Instance.BackToEditorMode();
				EditorLoadSave.Instance.LoadLevel("temp", false, string.Empty);
				EditorLoadSave.Instance.DeleteLevel("temp");
				LevelManager.Instance.PopulateLevel();
				LevelCreator.Instance.OnPlayTestEnded();
				mIsPlaytestMode = false;
				LevelEditorInputManager.SetNewInputState(true, true);
				MapSizeHandler.Instance.mapSizeFrame.root.gameObject.SetActive(true);
			}
		}
	}
}
