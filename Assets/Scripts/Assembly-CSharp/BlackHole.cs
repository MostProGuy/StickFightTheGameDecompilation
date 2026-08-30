using System;
using System.Collections.Generic;
using UnityEngine;

public class BlackHole : MonoBehaviour
{
	public ParticleSystem suck;

	public Controller damager;

	private Rigidbody[] objects;

	private List<Rigidbody> mSpawnedWeapons;

	private AudioSource au;

	private void Start()
	{
		damager = base.transform.GetComponentInParent<DamagerHolder>().damager;
		MultiplayerManager multiplayerManager = UnityEngine.Object.FindObjectOfType<MultiplayerManager>();
		mSpawnedWeapons = ((!MatchmakingHandler.IsNetworkMatch) ? GameManager.Instance.SpawnedWeapons : multiplayerManager.SpawnedWeapons);
		objects = UnityEngine.Object.FindObjectsOfType<Rigidbody>();
		au = GetComponentInParent<AudioSource>();
		GameManager gameManager = UnityEngine.Object.FindObjectOfType<GameManager>();
		if (gameManager != null)
		{
			gameManager.OnMatchEnded = (Action)Delegate.Combine(gameManager.OnMatchEnded, new Action(OnMatchEnded));
		}
	}

	public void OnMatchEnded()
	{
		SteamStatsAndAchievements instance = SteamStatsAndAchievements.Instance;
		if (instance != null)
		{
			foreach (Controller item in GameManager.Instance.playersAlive)
			{
				if (item != null && item.HasControl)
				{
					instance.UnlockAchievement(SteamStatsAndAchievements.EAchievement.Blackhole);
					break;
				}
			}
		}
		GameManager gameManager = UnityEngine.Object.FindObjectOfType<GameManager>();
		if (gameManager != null)
		{
			gameManager.OnMatchEnded = (Action)Delegate.Remove(gameManager.OnMatchEnded, new Action(OnMatchEnded));
		}
		UnityEngine.Object.Destroy(this);
	}

	public void OnDisabled()
	{
		GameManager gameManager = UnityEngine.Object.FindObjectOfType<GameManager>();
		if (gameManager != null)
		{
			gameManager.OnMatchEnded = (Action)Delegate.Remove(gameManager.OnMatchEnded, new Action(OnMatchEnded));
		}
	}

	public void AddRunTimeWeapon(Rigidbody newWeapon)
	{
		if (!mSpawnedWeapons.Contains(newWeapon))
		{
			mSpawnedWeapons.Add(newWeapon);
		}
	}

	private void FixedUpdate()
	{
		float x = base.transform.localScale.x;
		suck.emissionRate = x * 200f;
		au.volume = x / 2f;
		ScreenshakeHandler.Instance.AddShake(UnityEngine.Random.insideUnitSphere * x * Time.fixedDeltaTime * 4f);
		foreach (Controller player in ControllerHandler.Instance.players)
		{
			if (player == null)
			{
				continue;
			}
			HealthHandler component = player.GetComponent<HealthHandler>();
			bool flag = false;
			Rigidbody[] componentsInChildren = player.GetComponentsInChildren<Rigidbody>();
			foreach (Rigidbody rigidbody in componentsInChildren)
			{
				float num = Vector3.Distance(base.transform.parent.position, rigidbody.transform.position);
				if (num < x * 3.2f)
				{
					rigidbody.AddExplosionForce(-1000000f * Time.fixedDeltaTime, base.transform.parent.position, 500f);
				}
				if (num < x * 2.5f)
				{
					flag = true;
				}
				rigidbody.AddExplosionForce(-10000f * Time.fixedDeltaTime + x * -10000f * Time.fixedDeltaTime, base.transform.parent.position, x * 30f);
				if (rigidbody.drag < 1f)
				{
					rigidbody.velocity *= 0.8f;
				}
			}
			if (!(component == null) && flag && component.health > 0f && player.HasControl)
			{
				component.TakeDamage(500f, damager);
			}
		}
		Rigidbody[] array = objects;
		foreach (Rigidbody rigidbody2 in array)
		{
			if (!rigidbody2 || (bool)rigidbody2.transform.root.GetComponent<Controller>())
			{
				continue;
			}
			float num2 = 1f;
			DestructiblePiece component2 = rigidbody2.GetComponent<DestructiblePiece>();
			if ((bool)component2)
			{
				float num3 = Vector3.Distance(base.transform.parent.position, rigidbody2.transform.GetComponent<Renderer>().bounds.center);
				if (num3 < x * 4f + 1f)
				{
					if (rigidbody2.isKinematic)
					{
						component2.Collide((component2.forceThreshold + 1f) * (base.transform.parent.position - rigidbody2.transform.position).normalized, 1f);
					}
					rigidbody2.gameObject.GetComponent<Collider>().enabled = false;
					num2 = 5f;
				}
			}
			rigidbody2.AddExplosionForce(-500f * Time.fixedDeltaTime * num2 + x * -500f * Time.fixedDeltaTime * num2, base.transform.parent.position, x * 30f * num2, 0f, ForceMode.Acceleration);
			rigidbody2.velocity *= 0.9f;
		}
		foreach (Rigidbody mSpawnedWeapon in mSpawnedWeapons)
		{
			if (!(mSpawnedWeapon == null))
			{
				mSpawnedWeapon.AddExplosionForce(-500f * Time.fixedDeltaTime + x * -500f * Time.fixedDeltaTime, base.transform.parent.position, x * 30f, 0f, ForceMode.Acceleration);
				mSpawnedWeapon.velocity *= 0.9f;
			}
		}
	}
}
