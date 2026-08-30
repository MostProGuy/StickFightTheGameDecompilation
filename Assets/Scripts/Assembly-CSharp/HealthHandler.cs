using System;
using System.Collections;
using System.Collections.Generic;
using LevelEditor;
using UnityEngine;

public class HealthHandler : MonoBehaviour
{
	public float health = 100f;

	public float maxHealth = 100f;

	private CharacterInformation info;

	public float sinceDamage;

	private Controller controller;

	private Fighting fighting;

	private GrabHandler grabbing;

	private AI ai;

	public bool hasWeakness = true;

	private AudioSource au;

	public AudioClip[] deathClips;

	public AudioClip[] fallOutClips;

	public float sinceSpawn;

	private float sinceLavaDamage = 1f;

	private NetworkPlayer mNetworkPlayer;

	private float mLastSend;

	private float mSendThreshold = 0.2f;

	public float bossHealthZ = 100f;

	public bool FirstDeathFlag;

	private float DeathFlagTime;

	private bool canBeKilledWithoutOwnership;

	private void Awake()
	{
		info = GetComponent<CharacterInformation>();
		controller = GetComponent<Controller>();
		fighting = GetComponent<Fighting>();
		grabbing = GetComponent<GrabHandler>();
		au = GetComponentInChildren<AudioSource>();
		mNetworkPlayer = GetComponent<NetworkPlayer>();
		if ((bool)GetComponent<SnakeAI>())
		{
			canBeKilledWithoutOwnership = true;
		}
	}

	private void Start()
	{
		if (controller.isAI)
		{
			ai = GetComponent<AI>();
		}
		maxHealth = health;
	}

	private void Update()
	{
		sinceLavaDamage += Time.deltaTime;
		sinceSpawn += Time.deltaTime;
		sinceDamage += Time.deltaTime;
		if (controller.HasControl)
		{
			if ((bool)info && info.headTransform.position.y < -200f)
			{
				Die();
			}
			if (OptionsHolder.regen == 1)
			{
				health += Time.deltaTime * 10f * (100f / bossHealthZ) * (100f / (float)OptionsHolder.HP);
				health = Mathf.Clamp(health, 0f, 100f);
			}
		}
	}

	public bool TakeDamage(float damage, Controller damager, DamageType dmgType = DamageType.Other, bool playParticles = false, Vector3 position = default(Vector3), Vector3 direction = default(Vector3))
	{
		if ((bool)info && info.isDead)
		{
			return false;
		}
		damage *= 100f / (float)OptionsHolder.HP;
		damage *= 100f / bossHealthZ;
		if (sinceSpawn < 0.5f)
		{
			return false;
		}
		if ((bool)damager && !damager.HasControl && dmgType != DamageType.LocalDamage)
		{
			Debug.LogError("Tried to add damage locally but dont have control!");
			return false;
		}
		if (info == null || info.isDead || health <= 0f || (!GameManager.inFight && !GameManager.stillInMenu))
		{
			return false;
		}
		if ((bool)controller && controller.isAI && ai.reactionHitReset != 0f)
		{
			fighting.counter = ai.reactionHitReset;
		}
		if (damager != null)
		{
			controller.damager = damager;
		}
		float num = health;
		health -= damage;
		Debug.Log("Player Took damage!");
		if (MatchmakingHandler.IsNetworkMatch)
		{
			if (health <= 0f || Time.time - mLastSend > mSendThreshold)
			{
				bool killingBlow = false;
				if (health <= 0f)
				{
					killingBlow = true;
				}
				if ((bool)mNetworkPlayer)
				{
					mNetworkPlayer.UnitWasDamaged(damage, killingBlow, dmgType, playParticles, position, direction);
				}
				mLastSend = Time.time;
			}
		}
		else
		{
			if (health <= 0f && num > 0f)
			{
				if (controller.HasControl)
				{
					Die();
				}
				else if (canBeKilledWithoutOwnership)
				{
					Die();
				}
			}
			controller.OnTakeDamage(damage);
		}
		if ((bool)grabbing)
		{
			grabbing.grabStrength -= damage / 25f;
		}
		if (sinceDamage > 0.2f)
		{
			sinceDamage = 0f;
		}
		return true;
	}

	public void TakeDamage(float damage, byte attacker, bool hasControl)
	{
		if (sinceSpawn < 0.5f)
		{
			return;
		}
		List<ConnectedClientData> list = new List<ConnectedClientData>();
		list.AddRange(UnityEngine.Object.FindObjectOfType<MultiplayerManager>().ConnectedClients);
		controller.damager = list[attacker].PlayerObject.GetComponent<Controller>();
		if (controller.damager == null || !controller.damager.HasControl)
		{
			health -= damage;
		}
		if (damage == 666.666f)
		{
			Debug.Log("Killing Blow!!");
			health = 0f;
		}
		else
		{
			if (health <= 0f)
			{
				Debug.Log("DIED LOCALLY, SHOULD NOT DIE!");
			}
			health = Mathf.Clamp(health, 1f, 999f);
		}
		Debug.Log("Player Got damaged by Player: " + attacker);
		if (health <= 0f)
		{
			Die();
		}
		if ((bool)grabbing)
		{
			grabbing.grabStrength -= damage / 25f;
		}
		if (sinceDamage > 0.2f)
		{
			sinceDamage = 0f;
		}
		controller.OnTakeDamage(damage);
	}

	public void FallOut()
	{
		if (fallOutClips.Length > 0)
		{
			au.PlayOneShot(deathClips[UnityEngine.Random.Range(0, fallOutClips.Length)]);
		}
	}

	public void DealLavaDamage(Vector3 dir)
	{
		if (sinceLavaDamage < 0.3f)
		{
			return;
		}
		Rigidbody component = GetComponentInChildren<Hip>().GetComponent<Rigidbody>();
		Vector3 force = dir * 200f;
		if (MatchmakingHandler.IsNetworkMatch)
		{
			byte index = component.GetComponent<RigidBodyIndexHolder>().Index;
			if ((bool)mNetworkPlayer)
			{
				mNetworkPlayer.SendAddedForce(index, force, ForceMode.VelocityChange);
			}
		}
		component.AddForce(force, ForceMode.VelocityChange);
		sinceLavaDamage = 0f;
		TakeDamage(35f, null);
	}

	public bool DiedWithinSeconds(float time)
	{
		return Time.unscaledTime - DeathFlagTime < time;
	}

	public void ForcedDie()
	{
		if ((bool)info)
		{
			info.isDead = true;
		}
		FirstDeathFlag = true;
		DeathFlagTime = Time.unscaledTime;
		controller.EndBlock();
		NetworkPlayer networkPlayer = base.gameObject.FetchComponent<NetworkPlayer>();
		if (controller.HasControl)
		{
			networkPlayer.UnitWasDamaged(10000f, true);
		}
		networkPlayer.FlushChannels();
		if ((bool)fighting)
		{
			fighting.DropWeapon();
		}
	}

	private void Die()
	{
		if (sinceSpawn < 0.5f || (!GameManager.inFight && !GameManager.stillInMenu) || ((bool)info && info.isDead))
		{
			return;
		}
		if ((bool)info)
		{
			info.isDead = true;
		}
		if (!fighting)
		{
			return;
		}
		fighting.DropWeapon(true);
		if (deathClips.Length > 0)
		{
			au.PlayOneShot(deathClips[UnityEngine.Random.Range(0, deathClips.Length)]);
		}
		if ((bool)controller && controller.isAI)
		{
			UnityEngine.Object.FindObjectOfType<HoardHandler>().RemoveAI(controller);
			base.gameObject.AddComponent<RemoveAI>();
			return;
		}
		if (MatchmakingHandler.IsNetworkMatch)
		{
			NetworkPlayer networkPlayer = base.gameObject.FetchComponent<NetworkPlayer>();
			if (controller.HasControl)
			{
				networkPlayer.UnitWasDamaged(10000f, true);
			}
		}
		if (WorkshopStateHandler.IsPlayTestingMode)
		{
			StartCoroutine(WaitThen(2f, WorkshopStateHandler.ExitPlayMode));
		}
		else
		{
			GameManager.Instance.KillPlayer(controller);
		}
	}

	private IEnumerator WaitThen(float t, Action a)
	{
		yield return new WaitForSeconds(t);
		a();
	}
}
