using Steamworks;

namespace LevelEditor
{
	public class WorkshopDataHolder
	{
		public class WorkshopData
		{
			public bool isNew = true;

			public PublishedFileId_t publishedFileID;

			public string directoryPath;

			public string path;

			public string previewImagePath;

			public string levelName;

			public string description;
		}

		public WorkshopData workshopData = new WorkshopData();

		private static readonly WorkshopDataHolder _instance = new WorkshopDataHolder();

		public static WorkshopDataHolder Instance
		{
			get
			{
				return _instance;
			}
		}
	}
}
