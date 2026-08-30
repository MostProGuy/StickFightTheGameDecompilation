using UnityEngine;

public class BodyPart : MonoBehaviour
{
	public HealthHandler health;

	private float maxHp;

	public float multiplier = 1f;

	public float physicsMultiplier = 1f;

	public float weakness = 1f;

	private ParticleSystem damagePart;

	private Fighting fighting;

	private Rigidbody rig;

	private Vector3 lastVelocity;

	public bool isGun;

	public float gunHp = 1f;

	private float currentGunHp;

	public BodyPart[] parts;

	private AI ai;

	private Controller mParentController;

	private NetworkPlayer mNetworkPlayer;

	private CharacterInformation info;

	private SetMovementAbility movementAbility;

	public bool isNeverGun;

	private Controller controller;

	public Controller ParentController
	{
		get
		{
			return mParentController;
		}
	}

	private void Start()
	{
		health = base.transform.GetComponentInParent<HealthHandler>();
		ai = base.transform.GetComponentInParent<AI>();
		damagePart = health.GetComponentInChildren<DamageParticle>().GetComponent<ParticleSystem>();
		fighting = health.GetComponent<Fighting>();
		rig = GetComponent<Rigidbody>();
		mParentController = health.GetComponent<Controller>();
		mNetworkPlayer = health.GetComponent<NetworkPlayer>();
		parts = health.GetComponentsInChildren<BodyPart>();
		if ((bool)GetComponent<Weapon>() && !isNeverGun)
		{
			isGun = true;
		}
		maxHp = health.health;
		info = base.transform.root.GetComponent<CharacterInformation>();
		movementAbility = base.transform.root.GetComponent<SetMovementAbility>();
		controller = base.transform.root.GetComponent<Controller>();
	}

	private void OnEnable()
	{
		currentGunHp = gunHp;
	}

	private void LateUpdate()
	{
		lastVelocity = rig.velocity;
		if (health.hasWeakness)
		{
			weakness = 1f + (maxHp - health.health) / 100f;
			if (health.health <= 0f)
			{
				weakness = 0.5f;
			}
		}
	}

	public void TakeDamage(float damage, Controller damager)
	{
		if (isGun)
		{
			currentGunHp -= damage;
			if (currentGunHp < 0f)
			{
				fighting.DropWeapon(true);
			}
		}
		else
		{
			health.TakeDamage(damage * multiplier, damager);
		}
	}

	public void TakeDamageWithParticle(float damage, Vector3 position, Vector3 direction, Controller damager, DamageType type = DamageType.Other)
	{
		if (isGun)
		{
			currentGunHp -= damage;
			if (currentGunHp < 0f && mParentController.HasControl)
			{
				fighting.DropWeapon(true);
			}
		}
		else if (!(damager != null) || damager.HasControl || type == DamageType.LocalDamage)
		{
			damage = Mathf.Clamp(damage, 0f, 101f);
			if (health.TakeDamage(damage * multiplier, damager, type))
			{
				damagePart.transform.position = position + direction.normalized;
				damagePart.transform.rotation = Quaternion.LookRotation(direction);
				ParticleSystem.MainModule main = damagePart.main;
				main.startSpeedMultiplier = 1f + damage * 2f * multiplier;
				damagePart.Emit(5 + (int)(damage * 0.5f * multiplier));
			}
		}
	}

	public void TakeDamageWithParticle(float damage, Vector3 position, Vector3 direction, bool physicalDamage, Controller damager)
	{
		if (isGun)
		{
			currentGunHp -= damage;
			if (currentGunHp < 0f)
			{
				fighting.DropWeapon(true);
			}
		}
		else
		{
			if (damager != null && !damager.HasControl)
			{
				return;
			}
			damage = Mathf.Clamp(damage, 0f, 101f);
			bool flag = health.health <= 0f;
			bool flag2 = health.TakeDamage(damage, damager, DamageType.Punch, true, position, direction);
			bool flag3 = health.health <= 0f;
			if (damager != null && damager.HasControl)
			{
				MemoryBucket memoryBucket = damager.GetMemoryBucket(Controller.EMemoryBucketType.Transient);
				if (flag3 && !flag)
				{
					Weapon weapon = damager.GetComponent<Fighting>().weapon;
					if (weapon == null)
					{
						BlockHandler.BlockData value = memoryBucket.Copy<BlockHandler.BlockData>("BlockData").GetValue(new BlockHandler.BlockData(null, 0f));
						float num = Time.timeSinceLevelLoad - value.blockTimeStamp;
						float value2 = memoryBucket.Copy<float>("DamageTakenTimeStamp").GetValue(0f);
						float value3 = memoryBucket.Copy<float>("DamageGivenTimeStamp").GetValue(0f);
						float num2 = Time.timeSinceLevelLoad - value2;
						float num3 = Time.timeSinceLevelLoad - value3;
						if (num < num2 && value3 < num && value.blocker == damager)
						{
							SteamStatsAndAchievements.Instance.UnlockAchievement(SteamStatsAndAchievements.EAchievement.Riposte);
						}
					}
					else
					{
						bool flag4 = weapon.name.Contains("Blink Dagger");
						int num4 = memoryBucket.Copy<int>("BlinkDaggerKills").GetValue(0) + 1;
						memoryBucket.Put("BlinkDaggerKills", num4);
						if (num4 >= 3)
						{
							SteamStatsAndAchievements.Instance.UnlockAchievement(SteamStatsAndAchievements.EAchievement.BlinkDagger);
						}
					}
				}
				memoryBucket.Put("DamageGivenTimeStamp", Time.timeSinceLevelLoad);
			}
			if (flag2)
			{
				damagePart.transform.position = position + direction.normalized;
				damagePart.transform.rotation = Quaternion.LookRotation(direction);
				ParticleSystem.MainModule main = damagePart.main;
				main.startSpeedMultiplier = 1f + damage * 2f;
				damagePart.Emit(5 + (int)(damage * 0.5f));
			}
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.transform.root == base.transform.root || !collision.rigidbody || info.isDead)
		{
			return;
		}
		if (!collision.transform.root.GetComponent<Controller>() && mParentController.HasControl)
		{
			WeaponPickUp component = collision.rigidbody.GetComponent<WeaponPickUp>();
			if ((bool)component && component.counter > 0.3f && component.cantBePickledUpFor < 0f && !controller.canFly && (!fighting.weapon || OptionsHolder.pickup == 1 || component.sendTheMovementAbility != -1) && (!ai.enabled || ai.goForGuns))
			{
				if (MatchmakingHandler.IsNetworkMatch)
				{
					if (Time.time > component.CurrentDeadTime)
					{
						ushort networkSpawnIndex = component.NetworkSpawnIndex;
						Object.FindObjectOfType<MultiplayerManager>().RequestWeaponPickUp(networkSpawnIndex, (byte)mNetworkPlayer.NetworkSpawnID);
						component.DelayBy(0.5f);
					}
				}
				else
				{
					if (component.sendTheMovementAbility != -1)
					{
						movementAbility.SetAbility(component.sendTheMovementAbility);
					}
					fighting.PickUpWeapon(component.id, component.gameObject);
					if (!component.unEnding)
					{
						Object.Destroy(component.gameObject);
					}
					fighting.GetComponent<CharacterStats>().weaponsPickedUp++;
				}
			}
		}
		float num = KineticEnergy(collision.rigidbody) * 0.001f * multiplier;
		if (num > 25f && !(Vector3.Angle(collision.rigidbody.velocity, base.transform.position - collision.rigidbody.position) < 60f))
		{
		}
	}

	public static float KineticEnergy(Rigidbody rb)
	{
		return 0.5f * Mathf.Clamp(rb.mass, 0f, 500f) * Mathf.Pow(rb.velocity.magnitude, 2f);
	}

	private void OnCollisionStay(Collision collision)
	{
		if (collision.transform.root == base.transform.root || !collision.rigidbody)
		{
			return;
		}
		MeshRenderer component = collision.rigidbody.GetComponent<MeshRenderer>();
		if (!component || !(component.bounds.center.y > base.transform.position.y) || !collision.rigidbody.GetComponent<DestructiblePiece>() || !(collision.rigidbody.mass > 700f) || collision.rigidbody.isKinematic)
		{
			return;
		}
		float num = 0f;
		for (int i = 0; i < parts.Length; i++)
		{
			float num2 = Mathf.Abs(base.transform.position.y - parts[i].transform.position.y);
			if (num2 > num)
			{
				num = num2;
			}
		}
		if (num < 0.25f)
		{
			DestructiblePiece component2 = collision.rigidbody.GetComponent<DestructiblePiece>();
			if ((bool)component2)
			{
				TakeDamage(Time.deltaTime * 25f, component2.damager);
			}
		}
	}
}
