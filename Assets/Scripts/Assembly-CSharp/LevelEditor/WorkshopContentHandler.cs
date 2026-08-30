using System;
using Steamworks;

namespace LevelEditor
{
	public class WorkshopContentHandler
	{
		private WorkshopItemCreator m_WorkshopLevelCreator = new WorkshopItemCreator();

		private static readonly WorkshopContentHandler _instance = new WorkshopContentHandler();

		public static WorkshopContentHandler Instance
		{
			get
			{
				return _instance;
			}
		}

		public static void CreateNewItem(string path, string title, string description)
		{
			_instance.m_WorkshopLevelCreator.Upload(path, title, description);
		}

		public static void UpdateExistingItem(PublishedFileId_t pID, string path, string title, string description)
		{
			_instance.m_WorkshopLevelCreator.UpdateItem(pID, path, title, description);
		}

		public static void SetOnCreateItemAction(Action a)
		{
			_instance.m_WorkshopLevelCreator.OnItemCreatedAction(a);
		}

		public static void SetOnItemUpdatedAction(Action a)
		{
			_instance.m_WorkshopLevelCreator.OnItemUpdatedAction(a);
		}
	}
}
