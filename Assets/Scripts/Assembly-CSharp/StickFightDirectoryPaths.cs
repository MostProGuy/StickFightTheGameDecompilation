using UnityEngine;

public class StickFightDirectoryPaths : MonoBehaviour
{
	[SerializeField]
	private string m_LocalWorkshopPath;

	private static StickFightDirectoryPaths _instance;

	public string LocalWorkshopPath
	{
		get
		{
			return m_LocalWorkshopPath;
		}
	}

	public static StickFightDirectoryPaths Instance
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
}
