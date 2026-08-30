using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Steamworks;
using UnityEngine;

namespace LevelEditor
{
	public class EditorLoadSave
	{
		private string m_LoadedLevelPath = string.Empty;

		private static string m_SavePath = Application.persistentDataPath + "/CustomLevels/";

		private static readonly EditorLoadSave _instance = new EditorLoadSave();

		public string CurrentLoadedMap
		{
			get
			{
				return m_LoadedLevelPath;
			}
		}

		public bool HasbeenTouched { get; private set; }

		public static EditorLoadSave Instance
		{
			get
			{
				return _instance;
			}
		}

		public static bool CheckIfOverwriteLevel(string levelName)
		{
			if (levelName == "temp")
			{
				return false;
			}
			bool flag = _instance.m_LoadedLevelPath != levelName;
			return Directory.Exists(m_SavePath + levelName) && flag;
		}

		public static bool CheckIfPublishNew()
		{
			Debug.Log("Checking if can be updated: " + WorkshopDataHolder.Instance.workshopData.publishedFileID);
			return WorkshopDataHolder.Instance.workshopData.publishedFileID != new PublishedFileId_t(0uL);
		}

		public void PublishUpdate(string levelName)
		{
			Debug.Log("Publishing Update");
			SaveAndPublish(levelName);
		}

		public void PublishNew(string levelName)
		{
			Debug.Log("Publishing new...");
			WorkshopDataHolder.Instance.workshopData.publishedFileID = new PublishedFileId_t(0uL);
			SaveAndPublish(levelName);
		}

		private void SaveAndPublish(string levelName)
		{
			SaveLevel(levelName);
			if (WorkshopDataHolder.Instance.workshopData.publishedFileID == new PublishedFileId_t(0uL))
			{
				DialougePanelUI.Instance.Message("Uploading...");
				WorkshopContentHandler.CreateNewItem(WorkshopDataHolder.Instance.workshopData.path, levelName, WorkshopDataHolder.Instance.workshopData.description);
			}
			else
			{
				DialougePanelUI.Instance.Message("Updating...");
				WorkshopContentHandler.UpdateExistingItem(WorkshopDataHolder.Instance.workshopData.publishedFileID, WorkshopDataHolder.Instance.workshopData.path, levelName, WorkshopDataHolder.Instance.workshopData.description);
			}
			WorkshopContentHandler.SetOnItemUpdatedAction(() =>
			{
				Debug.Log("Trying To Delete directory: " + WorkshopDataHolder.Instance.workshopData.directoryPath);
				Directory.Delete(WorkshopDataHolder.Instance.workshopData.directoryPath, true);
				WorkshopDataHolder.Instance.workshopData.isNew = true;
				WorkshopDataHolder.Instance.workshopData.directoryPath = string.Empty;
				WorkshopDataHolder.Instance.workshopData.path = string.Empty;
				SetHasBeenTouched(false);
				LevelEditorInputManager.SetNewInputState(true, true);
			});
		}

		public void SaveLevel(string levelName, Action onClick = null)
		{
			bool flag = levelName != m_LoadedLevelPath;
			Debug.Log("Saving... IsNewMap? " + flag);
			CheckWorkshopFolder();
			string text = m_SavePath + levelName;
			if (Directory.Exists(text))
			{
				if (!(levelName != "temp"))
				{
					Debug.Log("Temp folder was already found? Did this not get deleted, Deleting and creating a fresh one!");
					Directory.Delete(text, true);
					Directory.CreateDirectory(text);
				}
			}
			else
			{
				Directory.CreateDirectory(text);
			}
			string text2 = text + "/Level";
			LevelManager instance = LevelManager.Instance;
			CustomLevel customLevel = new CustomLevel();
			customLevel.PlacedObjects = instance.GetSaveableLevelObjects;
			customLevel.PlacedWeapons = instance.GetSaveableLevelWeaponObjects;
			customLevel.SpawnPoints = instance.SpawnPoints;
			customLevel.Theme = instance.CurrentMapSettings.Theme;
			MapSizeHandler instance2 = MapSizeHandler.Instance;
			customLevel.MapSize = instance2.mapSize;
			Debug.Log("Saving Level With Theme: " + customLevel.Theme);
			IFormatter formatter = new BinaryFormatter();
			Stream stream = new FileStream(text2 + ".bin", FileMode.Create, FileAccess.Write, FileShare.None);
			formatter.Serialize(stream, customLevel);
			stream.Close();
			WorkshopDataHolder.Instance.workshopData.levelName = levelName;
			WorkshopDataHolder.Instance.workshopData.description = "Woop woop";
			WorkshopDataHolder.Instance.workshopData.path = text2 + ".bin";
			WorkshopDataHolder.Instance.workshopData.directoryPath = text;
			if (levelName != "temp")
			{
				string text3 = text + "/ScreenShot.png";
				ScreenshotHandler.Instance.TakeScreenshot(text3);
				WorkshopDataHolder.Instance.workshopData.previewImagePath = text3;
				m_LoadedLevelPath = levelName;
			}
			if (onClick != null)
			{
				onClick();
			}
			Debug.Log("Succesully Saved Level with: " + customLevel.PlacedObjects.Count + " Objects! And: " + customLevel.PlacedWeapons.Count + " Objects!" + text2);
		}

		public void LoadLevel(string levelName, bool workshopLevel = false, string publishID = "")
		{
			if (workshopLevel)
			{
				LoadWorkshopLevel(levelName, publishID);
			}
			else
			{
				LoadLocalLevel(levelName);
			}
		}

		public void SetHasBeenTouched(bool touched)
		{
			Debug.Log("HasBeenTOuched: " + touched);
			HasbeenTouched = touched;
		}

		public CustomLevel LoadLocalLevel(string levelName)
		{
			LevelManager.Instance.ClearLevel();
			m_LoadedLevelPath = levelName;
			IFormatter formatter = new BinaryFormatter();
			Stream stream = new FileStream(m_SavePath + levelName + "/Level.bin", FileMode.Open, FileAccess.Read, FileShare.None);
			CustomLevel customLevel = (CustomLevel)formatter.Deserialize(stream);
			stream.Close();
			MapSizeHandler.Instance.LoadSize(customLevel.MapSize);
			WorkshopLevelManager.SetNewLoadedLevel(customLevel);
			WorkshopDataHolder.Instance.workshopData.levelName = levelName;
			WorkshopDataHolder.Instance.workshopData.description = "Woop woop";
			WorkshopDataHolder.Instance.workshopData.path = m_SavePath + levelName + "/Level.bin";
			WorkshopDataHolder.Instance.workshopData.directoryPath = m_SavePath + levelName;
			WorkshopDataHolder.Instance.workshopData.publishedFileID = new PublishedFileId_t(0uL);
			WorkshopDataHolder.Instance.workshopData.isNew = false;
			return customLevel;
		}

		public CustomLevel LoadWorkshopLevel(string levelName, string publishID)
		{
			LevelManager.Instance.ClearLevel();
			m_LoadedLevelPath = levelName;
			IFormatter formatter = new BinaryFormatter();
			Stream stream = new FileStream(WorkshopMapsLoader.Instance.WorkshopPath + publishID + "/Level.bin", FileMode.Open, FileAccess.Read, FileShare.None);
			CustomLevel customLevel = (CustomLevel)formatter.Deserialize(stream);
			stream.Close();
			MapSizeHandler.Instance.LoadSize(customLevel.MapSize);
			WorkshopLevelManager.SetNewLoadedLevel(customLevel);
			WorkshopDataHolder.Instance.workshopData.levelName = levelName;
			WorkshopDataHolder.Instance.workshopData.description = "Woop woop";
			WorkshopDataHolder.Instance.workshopData.path = WorkshopMapsLoader.Instance.WorkshopPath + publishID + "/Level.bin";
			WorkshopDataHolder.Instance.workshopData.directoryPath = WorkshopMapsLoader.Instance.WorkshopPath + publishID;
			WorkshopDataHolder.Instance.workshopData.publishedFileID = new PublishedFileId_t(ulong.Parse(publishID));
			WorkshopDataHolder.Instance.workshopData.isNew = false;
			return customLevel;
		}

		public void DeleteLevel(string levelName)
		{
			Directory.Delete(m_SavePath + levelName, true);
			Debug.Log("Deleting Level: " + m_SavePath + levelName);
		}

		private static void CheckWorkshopFolder()
		{
			if (!Directory.Exists(m_SavePath))
			{
				Directory.CreateDirectory(m_SavePath);
			}
		}
	}
}
