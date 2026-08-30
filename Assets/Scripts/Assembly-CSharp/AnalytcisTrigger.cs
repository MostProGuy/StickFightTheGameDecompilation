using UnityEngine;
using UnityEngine.Analytics;

public class AnalytcisTrigger : MonoBehaviour
{
	[SerializeField]
	private AnalyticsTracker m_MatchEndTracker;

	[HideInInspector]
	public bool IsNetworkMatch;

	[HideInInspector]
	public bool IsCustomMatch;

	private static AnalytcisTrigger _instance;

	public static AnalytcisTrigger Instance
	{
		get
		{
			return _instance;
		}
	}

	private void Awake()
	{
		if (_instance != null)
		{
			Object.Destroy(base.gameObject);
		}
		else
		{
			_instance = this;
		}
	}

	public void OnMatchEnd(bool network, bool customMap)
	{
		IsNetworkMatch = network;
		IsCustomMatch = customMap;
		m_MatchEndTracker.TriggerEvent();
	}
}
