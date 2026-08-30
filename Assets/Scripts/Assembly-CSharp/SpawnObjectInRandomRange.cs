using UnityEngine;

public class SpawnObjectInRandomRange : MonoBehaviour
{
	public GameObject obj;

	public Vector3 spawn;

	public Vector3 randomSpawn;

	public float cd = 0.5f;

	public float aditionalRandomCd;

	private float counter;

	public bool randomX;

	public bool useThisTransformsPosition;

	public bool removeItemOnOutOfScreen;

	private float startCounter;

	private MultiplayerManager mNetworkManager;

	[SerializeField]
	private bool m_SyncPosition;

	public bool alwaysGo;

	private void Awake()
	{
		mNetworkManager = Object.FindObjectOfType<MultiplayerManager>();
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (!GameManager.inFight && !alwaysGo)
		{
			return;
		}
		startCounter += Time.deltaTime;
		if (startCounter < 1.5f)
		{
			return;
		}
		counter += Time.deltaTime;
		if (!(counter > cd))
		{
			return;
		}
		counter = 0f - Random.Range(0f, aditionalRandomCd);
		Vector3 position = spawn;
		if (useThisTransformsPosition)
		{
			position = base.transform.position;
		}
		position += new Vector3(Random.Range(0f - randomSpawn.x, randomSpawn.x), Random.Range(0f - randomSpawn.y, randomSpawn.y), Random.Range(0f - randomSpawn.z, randomSpawn.z));
		if (MatchmakingHandler.IsNetworkMatch)
		{
			if (MultiplayerManager.IsServer)
			{
				float rotation = Random.Range(-360, 360);
				mNetworkManager.SpawnObject(obj, position, rotation, m_SyncPosition);
			}
			return;
		}
		GameObject gameObject = Object.Instantiate(obj, position, Quaternion.identity);
		if (randomX)
		{
			gameObject.transform.rotation = Quaternion.Euler(Random.Range(-360, 360), 0f, 0f);
		}
		if (removeItemOnOutOfScreen)
		{
			gameObject.AddComponent<RemoveOffScreen>();
		}
	}
}
