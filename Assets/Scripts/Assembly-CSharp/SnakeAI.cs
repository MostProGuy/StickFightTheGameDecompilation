using System;
using LevelEditor;
using UnityEngine;

public class SnakeAI : MonoBehaviour
{
	public Rigidbody mainRig;

	public Rigidbody[] rigs;

	public float movementSpeed;

	public float gravity;

	private Rigidbody target;

	private CharacterInformation targetInformation;

	private Controller mTargetController;

	public float playerForceMultiplier = 1f;

	public float playerForceMultiplierUp = 1f;

	public float rangeMultiplier = 1f;

	public float damageMultiplier = 1f;

	public float targetRange = 10f;

	private float reactionCounter;

	private float reactionTime = 0.4f;

	private Fighting fighting;

	private float snakeCounter;

	private float randomOffset;

	private float randomSpeed;

	private Vector3 lastPoint;

	private CharacterInformation info;

	private Controller[] players;

	private float lifeCounter;

	private bool mHasControl;

	private NetworkSyncableObject mNetworkSyncableObject;

	private void Awake()
	{
		info = GetComponent<CharacterInformation>();
		SetStats();
		mHasControl = !MatchmakingHandler.IsNetworkMatch || (MatchmakingHandler.IsNetworkMatch && MultiplayerManager.IsServer);
	}

	private void Start()
	{
		mNetworkSyncableObject = GetComponent<NetworkSyncableObject>();
		GameManager gameManager = UnityEngine.Object.FindObjectOfType<GameManager>();
		if ((bool)gameManager)
		{
			gameManager.OnMatchEnded = (Action)Delegate.Combine(gameManager.OnMatchEnded, new Action(OnMatchEnded));
		}
	}

	private void OnDisable()
	{
		GameManager gameManager = UnityEngine.Object.FindObjectOfType<GameManager>();
		if ((bool)gameManager)
		{
			gameManager.OnMatchEnded = (Action)Delegate.Remove(gameManager.OnMatchEnded, new Action(OnMatchEnded));
		}
	}

	private void OnMatchEnded()
	{
		if (mTargetController != null && mTargetController.HasControl)
		{
			HealthHandler component = mTargetController.GetComponent<HealthHandler>();
			if (component != null && component.health > 0f)
			{
				SteamStatsAndAchievements.Instance.UnlockAchievement(SteamStatsAndAchievements.EAchievement.StickIrvin);
			}
		}
	}

	private void SetStats()
	{
		randomOffset = UnityEngine.Random.Range(0, 10);
		movementSpeed *= UnityEngine.Random.Range(0.9f, 1.1f);
		randomSpeed = UnityEngine.Random.Range(0.25f, 2f);
	}

	private void Update()
	{
		lifeCounter += Time.deltaTime;
		if (lifeCounter < 0.5f || info.isDead)
		{
			return;
		}
		if (snakeCounter > 1f)
		{
			snakeCounter = 0f;
			if (mHasControl && mNetworkSyncableObject != null)
			{
				target = null;
				mNetworkSyncableObject.NewSnakeTarget(byte.MaxValue);
			}
			else
			{
				target = null;
			}
		}
		for (int i = 0; i < rigs.Length; i++)
		{
			rigs[i].AddForce(Vector3.up * (Mathf.Cos((Time.time + randomOffset) * -15f * randomSpeed + (float)i) - 0.3f) * Time.deltaTime * 3000f, ForceMode.Acceleration);
			rigs[i].AddForce(Vector3.down * gravity, ForceMode.Acceleration);
		}
		if ((bool)target && (!targetInformation || !targetInformation.isDead))
		{
			snakeCounter += Time.deltaTime;
			if (target.position.z < mainRig.position.z)
			{
				mainRig.AddForce(Vector3.forward * Time.deltaTime * (0f - movementSpeed), ForceMode.Acceleration);
			}
			if (target.position.z > mainRig.position.z)
			{
				mainRig.AddForce(Vector3.forward * Time.deltaTime * movementSpeed, ForceMode.Acceleration);
			}
			if (target.position.y > mainRig.position.y)
			{
				mainRig.AddForce(Vector3.up * Time.deltaTime * movementSpeed * 0.5f, ForceMode.Acceleration);
			}
			float num = 1f;
			reactionTime = 0.4f;
			if (Vector3.Distance(mainRig.position, target.position) < num * rangeMultiplier + 0.5f)
			{
				if (Vector3.Distance(mainRig.position, target.position) < num * rangeMultiplier)
				{
					target.AddForce((mainRig.position - target.position).normalized * playerForceMultiplier * Time.deltaTime * 60000f * 0.5f, ForceMode.Acceleration);
					target.AddForce(Vector3.down * Time.deltaTime * 60000f * playerForceMultiplierUp * 0.5f, ForceMode.Acceleration);
				}
				reactionCounter += Time.deltaTime;
				if (reactionCounter > reactionTime && (bool)mTargetController && mTargetController.HasControl)
				{
					HealthHandler component = targetInformation.GetComponent<HealthHandler>();
					target.AddForce((mainRig.position - target.position).normalized * -250f * Mathf.Abs(playerForceMultiplier) * 0.5f, ForceMode.VelocityChange);
					mainRig.AddForce((mainRig.position - target.position).normalized * -50f * Mathf.Abs(playerForceMultiplier) * 0.5f, ForceMode.VelocityChange);
					bool flag = component.health <= 0f;
					component.TakeDamage(5f * damageMultiplier, null);
					if (component.health <= 0f && !flag)
					{
						SteamStatsAndAchievements.Instance.UnlockAchievement(SteamStatsAndAchievements.EAchievement.Snake);
					}
					reactionCounter = 0f;
				}
			}
			else if (reactionCounter > 0f)
			{
				reactionCounter -= Time.deltaTime;
			}
			return;
		}
		mainRig.AddForce(Vector3.forward * Mathf.Cos((Time.time + randomOffset) * -1f * randomSpeed) * Time.deltaTime * 4000f, ForceMode.Acceleration);
		mainRig.AddForce(Vector3.up * Time.deltaTime * (Mathf.Cos(Time.time + 100f + randomOffset) + 1f) * movementSpeed * 0.3f, ForceMode.Acceleration);
		Transform transform = null;
		byte targetPlayerIndex = byte.MaxValue;
		bool flag2 = false;
		float num2 = 100f;
		if (WorkshopStateHandler.IsPlayTestingMode)
		{
			CharacterStats characterStats = UnityEngine.Object.FindObjectOfType<CharacterStats>();
			Transform transform2 = characterStats.GetComponentInChildren<Hip>().transform;
			target = transform2.GetComponent<Rigidbody>();
			return;
		}
		foreach (Controller player in GameManager.Instance.GetComponent<ControllerHandler>().players)
		{
			if (!(player != null) || player.inactive)
			{
				continue;
			}
			CharacterInformation component2 = player.GetComponent<CharacterInformation>();
			if (component2.isDead)
			{
				continue;
			}
			Transform transform3 = player.GetComponentInChildren<Hip>().transform;
			float num3 = Vector3.Distance(mainRig.position, transform3.position);
			if (!(num3 < num2) || !(num3 < targetRange))
			{
				continue;
			}
			num2 = num3;
			targetInformation = component2;
			mTargetController = player;
			if (MatchmakingHandler.IsNetworkMatch && mNetworkSyncableObject != null)
			{
				if (MultiplayerManager.IsServer)
				{
					NetworkPlayer component3 = player.GetComponent<NetworkPlayer>();
					targetPlayerIndex = (byte)component3.NetworkSpawnID;
					flag2 = true;
				}
			}
			else
			{
				transform = transform3;
				flag2 = true;
			}
		}
		if (!flag2)
		{
			return;
		}
		if (MatchmakingHandler.IsNetworkMatch && mNetworkSyncableObject != null)
		{
			if (MultiplayerManager.IsServer)
			{
				mNetworkSyncableObject.NewSnakeTarget(targetPlayerIndex);
			}
		}
		else if ((bool)transform)
		{
			target = transform.GetComponent<Rigidbody>();
		}
	}

	public void NetworkForceNewTarget(Controller c)
	{
		if (c == null)
		{
			target = null;
			return;
		}
		Rigidbody component = c.GetComponentInChildren<Hip>().GetComponent<Rigidbody>();
		target = component;
		Debug.Log("New Snake target Assigned! ", c);
	}
}
