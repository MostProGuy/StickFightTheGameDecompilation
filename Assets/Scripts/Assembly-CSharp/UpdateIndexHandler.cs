using UnityEngine;

public class UpdateIndexHandler : MonoBehaviour
{
	private static int m_updateIndex;

	private static int MAX_UPDATE_INDEX = 5;

	public static int FRAME_UPDATE_INDEX_FIXED { get; private set; }

	public static int FRAME_UPDATE_INDEX { get; private set; }

	public static int UPDATE_INDEX
	{
		get
		{
			IncrementUpdateIndex();
			return m_updateIndex;
		}
	}

	private void Awake()
	{
		FRAME_UPDATE_INDEX_FIXED = 0;
		m_updateIndex = -1;
	}

	private static void IncrementUpdateIndex()
	{
		m_updateIndex++;
		if (m_updateIndex >= MAX_UPDATE_INDEX)
		{
			m_updateIndex = 0;
		}
	}

	private void IncrementFrameUpdateIndexFixed()
	{
		FRAME_UPDATE_INDEX_FIXED++;
		if (FRAME_UPDATE_INDEX_FIXED >= MAX_UPDATE_INDEX)
		{
			FRAME_UPDATE_INDEX_FIXED = 0;
		}
	}

	private void IncrementFrameUpdateIndex()
	{
		FRAME_UPDATE_INDEX++;
		if (FRAME_UPDATE_INDEX >= MAX_UPDATE_INDEX)
		{
			FRAME_UPDATE_INDEX = 0;
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
		IncrementFrameUpdateIndex();
	}

	private void FixedUpdate()
	{
		IncrementFrameUpdateIndexFixed();
	}
}
