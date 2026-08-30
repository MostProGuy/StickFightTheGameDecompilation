using UnityEngine;

public class MultiplayerManagerAssets : MonoBehaviour
{
	[SerializeField]
	private GameObject m_PlayerPrefab;

	[SerializeField]
	private Material[] m_Colors;

	public GameObject PlayerPrefab
	{
		get
		{
			return m_PlayerPrefab;
		}
	}

	public Material[] Colors
	{
		get
		{
			return m_Colors;
		}
	}

	public static MultiplayerManagerAssets Instance { get; private set; }

	private void Awake()
	{
		Instance = this;
	}
}
