using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
	public GameObject objectToSpawn;

	public float startDelay;

	public float rate;

	public float addedRandom;

	private float counter;

	public int allowedSpawns = 9999;

	private MultiplayerManager mNetworkManager;

	[SerializeField]
	private bool m_SyncPosition;

	private void Awake()
	{
		mNetworkManager = Object.FindObjectOfType<MultiplayerManager>();
	}

	private void Start()
	{
		counter = 0f - Random.Range(0f, addedRandom);
	}

	private void Update()
	{
		if (!GameManager.inFight && !GameManager.stillInMenu)
		{
			return;
		}
		startDelay -= Time.deltaTime;
		if (startDelay > 0f)
		{
			return;
		}
		counter += Time.deltaTime;
		if (!(counter > rate) || allowedSpawns <= 0)
		{
			return;
		}
		allowedSpawns--;
		counter = 0f - Random.Range(0f, addedRandom);
		if (MatchmakingHandler.IsNetworkMatch)
		{
			if (MultiplayerManager.IsServer)
			{
				mNetworkManager.SpawnObject(objectToSpawn, base.transform.position, base.transform.rotation.eulerAngles, m_SyncPosition);
			}
		}
		else
		{
			Quaternion rotation = base.transform.rotation;
			GameObject gameObject = Object.Instantiate(objectToSpawn, base.transform.position, rotation);
		}
	}
}
