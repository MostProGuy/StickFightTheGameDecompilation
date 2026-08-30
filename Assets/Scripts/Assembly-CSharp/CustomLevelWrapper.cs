using LevelEditor;
using Steamworks;

public class CustomLevelWrapper
{
	private CustomLevel m_LevelData;

	private PublishedFileId_t m_PublishID;

	public PublishedFileId_t PublishID
	{
		get
		{
			return m_PublishID;
		}
	}

	public CustomLevel LevelData
	{
		get
		{
			return m_LevelData;
		}
	}

	public CustomLevelWrapper(CustomLevel data, PublishedFileId_t id)
	{
		m_LevelData = data;
		m_PublishID = id;
	}
}
