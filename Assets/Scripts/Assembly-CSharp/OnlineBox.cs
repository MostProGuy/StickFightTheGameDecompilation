using UnityEngine;

public class OnlineBox : MonoBehaviour
{
	private bool allPlayersAreIn;

	private float counter;

	private ParticleSystem part;

	public GameObject blockers;

	private CodeAnimation codeAnim;

	private Animator anim;

	private GameManager manager;

	private bool done;

	private const float TIMEOUT_TIME = 120f;

	private static float mDisconnectTimer;

	private static bool mJoined;

	private LoadingScreenManager mLoadingScreenManager;

	private void Awake()
	{
		mJoined = false;
		mDisconnectTimer = 0f;
	}

	private void Start()
	{
		anim = GetComponent<Animator>();
		codeAnim = GetComponent<CodeAnimation>();
		manager = GameManager.Instance;
		part = GetComponentInChildren<ParticleSystem>();
		mLoadingScreenManager = Object.FindObjectOfType<LoadingScreenManager>();
	}

	private void Update()
	{
		if (done)
		{
			if (!mJoined)
			{
				TickDisconnectTimer();
			}
			return;
		}
		CheckPlayers();
		if (allPlayersAreIn && manager.playersAlive.Count > 0)
		{
			counter += Time.deltaTime;
		}
		else
		{
			counter -= Time.deltaTime;
		}
		counter = Mathf.Clamp(counter, 0f, 1f);
		if (counter > 0.5f)
		{
			anim.SetBool("IsOpen", false);
			blockers.SetActive(true);
			if (!part.isPlaying)
			{
				part.Play();
			}
			if (!done)
			{
				done = true;
				MultiplayerManager multiplayerManager = Object.FindObjectOfType<MultiplayerManager>();
				multiplayerManager.AssignBox(this);
				if (base.gameObject.name.ToLower() == "host")
				{
					MatchmakingHandler.Instance.CreateSteamLobby(4, true);
				}
				else
				{
					MatchmakingHandler.Instance.JoinRandomServer();
				}
			}
		}
		else
		{
			anim.SetBool("IsOpen", true);
			blockers.SetActive(false);
			part.Stop();
		}
	}

	public static void Joined()
	{
		mJoined = true;
	}

	public void StartLoading()
	{
		mLoadingScreenManager.StartLoading();
		done = true;
	}

	private void TickDisconnectTimer()
	{
		mDisconnectTimer += Time.deltaTime;
		if (mDisconnectTimer > 120f)
		{
			Debug.LogError("Diconnect!");
			mLoadingScreenManager.LoadThenFail(ConnectionErrorType.TimeOut, string.Empty);
		}
	}

	public void PlayBoxAnimation()
	{
		codeAnim.Play();
	}

	private void CheckPlayers()
	{
		allPlayersAreIn = true;
		foreach (Controller item in manager.playersAlive)
		{
			if (Mathf.Abs(item.GetComponentInChildren<Torso>().transform.position.z - base.transform.position.z) > 1f)
			{
				allPlayersAreIn = false;
			}
			if (Mathf.Abs(item.GetComponentInChildren<Torso>().transform.position.y - base.transform.position.y) > 0.9f)
			{
				allPlayersAreIn = false;
			}
		}
	}
}
