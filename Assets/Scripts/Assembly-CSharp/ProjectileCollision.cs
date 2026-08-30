using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ProjectileCollision : MonoBehaviour
{
	private bool done;

	public float force;

	public float torsoForce;

	public float damage;

	public float secondsToReachFullDamage;

	private float damageScale = 1f;

	public bool hardDamageScale;

	private float fullDamage;

	private float lifeCounter;

	private AudioSource au;

	public AudioClip[] clips;

	public AudioClip[] clips2;

	public string SpecialEffect = string.Empty;

	private SpawnStickyObject spawnStickyObject;

	private StickyObject stickyObject;

	public float secondsToDestroyAfterCollision;

	public Controller damager;

	public bool useNormalRotationForSpawning;

	private ProjectileBounce bounce;

	public GameObject spawnObject;

	public UnityEvent hitEvent;

	public bool neverStop;

	public bool spawnNoRotation;

	public bool stopMovingWhenHitSomething;

	private Controller ricochetCauser;

	private RayCastForward forward;

	private Gravity gravity;

	[SerializeField]
	private bool m_GiveLocalDamageOnly;

	private void Start()
	{
		au = base.transform.root.GetComponentInChildren<AudioSource>();
		spawnStickyObject = GetComponent<SpawnStickyObject>();
		stickyObject = GetComponent<StickyObject>();
		bounce = GetComponent<ProjectileBounce>();
		forward = GetComponent<RayCastForward>();
		gravity = GetComponent<Gravity>();
		fullDamage = damage;
	}

	private void Update()
	{
		if (secondsToReachFullDamage != 0f)
		{
			if (hardDamageScale)
			{
				if (lifeCounter > secondsToReachFullDamage)
				{
					damageScale = 1f;
					base.transform.GetChild(0).localScale = Vector3.one * 1f;
				}
				else
				{
					damageScale = 0.1f;
					base.transform.GetChild(0).localScale = Vector3.one * 0.3f;
				}
			}
			else
			{
				damageScale = Mathf.Clamp(lifeCounter / secondsToReachFullDamage, 0f, 1f);
			}
		}
		lifeCounter += Time.deltaTime;
	}

	public bool HitObject(RaycastHit hit)
	{
		if (done && !neverStop)
		{
			return false;
		}
		RayCastForward component = GetComponent<RayCastForward>();
		if (stopMovingWhenHitSomething)
		{
			if ((bool)component)
			{
				component.enabled = false;
			}
			base.transform.position = hit.point;
			return false;
		}
		Rigidbody rigidbody = hit.rigidbody;
		RemoveParticles[] componentsInChildren = GetComponentsInChildren<RemoveParticles>();
		foreach (RemoveParticles removeParticles in componentsInChildren)
		{
			Object.Destroy(removeParticles.gameObject);
		}
		float resistance = 1f;
		float num = 1f;
		Armor component2 = hit.collider.GetComponent<Armor>();
		if ((bool)component2)
		{
			resistance = component2.knockBackProt;
			num = component2.healthProt;
			rigidbody = component2.rig;
			if (component2.bounce)
			{
				base.transform.rotation = Quaternion.LookRotation(Vector3.Reflect(base.transform.forward, hit.normal));
				base.transform.rotation = Quaternion.LookRotation(new Vector3(0f, base.transform.forward.y, base.transform.forward.z));
				base.transform.position = hit.point;
				component.mask = -1;
				PlayParticles(hit.normal);
				return false;
			}
		}
		if ((bool)rigidbody)
		{
			if ((bool)rigidbody.GetComponent<Glue>())
			{
				return false;
			}
			DestructiblePiece component3 = rigidbody.GetComponent<DestructiblePiece>();
			if ((bool)component3)
			{
				float num2 = 1f;
				if ((bool)damager)
				{
					component3.damager = damager;
				}
				if (damage < 15f)
				{
					num2 = 2f;
				}
				if ((bool)damager && damager.HasControl)
				{
					component3.Collide(base.transform.forward * damage * num2, 1f);
				}
			}
			if (rigidbody.isKinematic)
			{
				if ((bool)bounce)
				{
					bounce.Bounce(hit);
					return false;
				}
				if (MatchmakingHandler.IsNetworkMatch && !MultiplayerManager.IsServer)
				{
					SendNetworkForceToObject(rigidbody);
				}
			}
			else
			{
				BodyPart component4 = rigidbody.GetComponent<BodyPart>();
				if ((bool)component4 && num > 0f)
				{
					BlockHandler component5 = component4.transform.root.GetComponent<BlockHandler>();
					Vector3 normalized = base.transform.forward;
					if ((bool)gravity)
					{
						normalized = (forward.speed * base.transform.forward + gravity.force * Vector3.down).normalized;
					}
					if ((bool)component5 && Vector3.Angle(-component5.blockAngle, normalized) < component5.blockForce * 40f && component5.isBlocking && component5.sinceBlockStart < 0.3f)
					{
						ricochetCauser = component5.GetComponent<Controller>();
						base.transform.rotation = Quaternion.LookRotation(component5.blockAngle);
						component5.DoBlock();
						component.mask = 1 << component4.gameObject.layer;
						component.mask = ~(int)component.mask;
						return true;
					}
					if ((bool)damager)
					{
						damager.GetComponent<CharacterStats>().bulletsHit++;
					}
					if ((bool)damager && damager.HasControl && !m_GiveLocalDamageOnly)
					{
						CharacterInformation component6 = component4.transform.root.GetComponent<CharacterInformation>();
						bool flag = component4.health.health <= 0f;
						component4.TakeDamageWithParticle(damage * damageScale * num, hit.point, base.transform.forward, damager);
						if (component4.health.health <= 0f && !flag)
						{
							if (damager != null && component4.GetComponent<Head>() != null)
							{
								SteamStatsAndAchievements.Instance.UnlockAchievement(SteamStatsAndAchievements.EAchievement.Headshot);
							}
							RayCastForward component7 = GetComponent<RayCastForward>();
							if (component7 != null && base.gameObject.name.Contains("Sniper"))
							{
								int value = SteamStatsAndAchievements.PersistentMemory.Copy<int>("STAT_SniperKills").GetValue(0);
								SteamStatsAndAchievements.PersistentMemory.Put("STAT_SniperKills", value + 1);
							}
							if (bounce != null && bounce.bounces <= 0)
							{
								SteamStatsAndAchievements.Instance.UnlockAchievement(SteamStatsAndAchievements.EAchievement.Bounce);
							}
						}
						if (SpecialEffect != string.Empty)
						{
							base.transform.SendMessage(SpecialEffect, rigidbody);
						}
					}
					else if (m_GiveLocalDamageOnly && component4.ParentController.HasControl)
					{
						component4.TakeDamageWithParticle(damage * damageScale * num, hit.point, base.transform.forward, damager, DamageType.LocalDamage);
						if (SpecialEffect != string.Empty)
						{
							base.transform.SendMessage(SpecialEffect, rigidbody);
						}
					}
					if (!damager)
					{
						component4.TakeDamageWithParticle(damage * damageScale * num, hit.point, base.transform.forward, null);
						if (SpecialEffect != string.Empty)
						{
							base.transform.SendMessage(SpecialEffect, rigidbody);
						}
					}
					int num3 = 0;
					int num4 = 0;
					List<ConnectedClientData> list = new List<ConnectedClientData>();
					list.AddRange(Object.FindObjectOfType<MultiplayerManager>().ConnectedClients);
					foreach (ConnectedClientData item in list)
					{
						if (item == null)
						{
							continue;
						}
						num4++;
						if (item.PlayerObject != null)
						{
							HealthHandler component8 = item.PlayerObject.GetComponent<HealthHandler>();
							CharacterInformation component9 = item.PlayerObject.GetComponent<CharacterInformation>();
							if (component8.health <= 0f)
							{
								num3++;
							}
						}
					}
					if (num3 >= num4 - 1 && ricochetCauser != null)
					{
						if (ricochetCauser.HasControl)
						{
							SteamStatsAndAchievements.Instance.UnlockAchievement(SteamStatsAndAchievements.EAchievement.Ricochet);
						}
						else
						{
							NetworkPlayer component10 = ricochetCauser.GetComponent<NetworkPlayer>();
							if (component10 != null)
							{
								component10.WonWithRicochet();
							}
						}
					}
				}
				else
				{
					if ((bool)bounce)
					{
						bounce.Bounce(hit);
						return false;
					}
					if ((bool)damager)
					{
						damager.GetComponent<CharacterStats>().bulletsMissed++;
					}
				}
				AddForce(rigidbody, resistance);
			}
		}
		else
		{
			if ((bool)hit.transform.GetComponentInParent<ProjectileCollision>())
			{
				Object.Destroy(hit.transform.parent.gameObject);
				ParticleSystem[] componentsInChildren2 = hit.transform.parent.transform.GetChild(0).GetComponentsInChildren<ParticleSystem>();
				foreach (ParticleSystem particleSystem in componentsInChildren2)
				{
					particleSystem.Play();
				}
			}
			if ((bool)bounce)
			{
				bounce.Bounce(hit);
				return false;
			}
			if ((bool)damager && !bounce)
			{
				damager.GetComponent<CharacterStats>().bulletsMissed++;
			}
		}
		if (!neverStop)
		{
			component.enabled = false;
			base.transform.position = hit.point;
		}
		done = true;
		hitEvent.Invoke();
		PlayParticles(hit.normal);
		if ((bool)spawnObject)
		{
			Quaternion rotation = base.transform.rotation;
			if (useNormalRotationForSpawning)
			{
				rotation = Quaternion.LookRotation(hit.normal);
			}
			if (spawnNoRotation)
			{
				rotation = Quaternion.identity;
			}
			if (spawnObject.name.ToLower().Contains("blackhole") && MatchmakingHandler.IsNetworkMatch)
			{
				if (damager.HasControl)
				{
					Object.FindObjectOfType<MultiplayerManager>().SpawnObject(spawnObject, base.transform.position, rotation.eulerAngles, false);
				}
			}
			else
			{
				Object.Instantiate(spawnObject, base.transform.position, rotation);
			}
		}
		if ((bool)spawnStickyObject)
		{
			Quaternion rotation2 = base.transform.rotation;
			if (useNormalRotationForSpawning)
			{
				rotation2 = Quaternion.LookRotation(hit.normal);
			}
			spawnStickyObject.Hit(hit.point, rotation2, rigidbody, damager);
		}
		if ((bool)stickyObject)
		{
			base.transform.position = hit.point;
			Quaternion rot = base.transform.rotation;
			if (useNormalRotationForSpawning)
			{
				rot = Quaternion.LookRotation(hit.normal);
			}
			stickyObject.Stick(rigidbody, rot, damager);
		}
		if (secondsToDestroyAfterCollision != float.PositiveInfinity && !neverStop)
		{
			StartCoroutine(DestroyAfterDelay(secondsToDestroyAfterCollision));
		}
		return false;
	}

	private void SendNetworkForceToObject(Rigidbody rig)
	{
		if (!rig)
		{
			Debug.LogWarning("Projectile trying to add force to object without rig");
			return;
		}
		Vector3 vector = base.transform.forward * force;
		if (!MatchmakingHandler.IsNetworkMatch || !damager || !damager.HasControl)
		{
			return;
		}
		RigidBodyIndexHolder component = rig.GetComponent<RigidBodyIndexHolder>();
		if (component == null)
		{
			Debug.LogError("Could not find rigidbodyindex holder for rigidbody: " + rig.name, rig);
			return;
		}
		byte index = component.Index;
		NetworkSyncableObject componentInParent = rig.GetComponentInParent<NetworkSyncableObject>();
		if ((bool)componentInParent)
		{
			componentInParent.ForceAdded(index, vector, ForceMode.Impulse);
		}
		else
		{
			rig.GetComponentInParent<NetworkPlayer>().SendAddedForce(index, vector, ForceMode.Impulse);
		}
	}

	private void AddForce(Rigidbody rig, float resistance)
	{
		Vector3 vector = base.transform.forward * force;
		if ((bool)gravity && (bool)forward)
		{
			vector = (forward.speed * base.transform.forward + gravity.force * Vector3.down).normalized * force;
		}
		vector *= resistance;
		float num = Mathf.Clamp(rig.mass / 100f, 0.01f, 1f);
		if (!rig.transform.root.GetComponent<CharacterInformation>())
		{
			vector *= num;
		}
		if (MatchmakingHandler.IsNetworkMatch)
		{
			if ((bool)damager && !damager.HasControl)
			{
				return;
			}
			RigidBodyIndexHolder component = rig.GetComponent<RigidBodyIndexHolder>();
			if (component == null)
			{
				Debug.LogWarning("Could not find rigidbodyindex holder for rigidbody: " + rig.name, rig);
			}
			else
			{
				byte index = component.Index;
				NetworkSyncableObject componentInParent = rig.GetComponentInParent<NetworkSyncableObject>();
				if ((bool)componentInParent)
				{
					componentInParent.ForceAdded(index, vector, ForceMode.Impulse);
				}
				else
				{
					rig.GetComponentInParent<NetworkPlayer>().SendAddedForce(index, vector, ForceMode.Impulse);
				}
			}
		}
		if ((bool)rig)
		{
			rig.AddForce(vector * num * damageScale, ForceMode.Impulse);
		}
		if (torsoForce == 0f)
		{
			return;
		}
		Torso componentInChildren = rig.transform.root.GetComponentInChildren<Torso>();
		if (!componentInChildren)
		{
			return;
		}
		Vector3 vector2 = base.transform.forward * torsoForce;
		Rigidbody component2 = componentInChildren.GetComponent<Rigidbody>();
		component2.AddForce(vector2 * damageScale, ForceMode.Impulse);
		if (MatchmakingHandler.IsNetworkMatch && (!damager || damager.HasControl))
		{
			RigidBodyIndexHolder component3 = component2.GetComponent<RigidBodyIndexHolder>();
			if (component3 == null)
			{
				Debug.LogWarning("Could not find rigidbodyindex holder for rigidbody: " + rig.name, rig);
				return;
			}
			byte index2 = component3.Index;
			component2.GetComponentInParent<NetworkPlayer>().SendAddedForce(index2, vector, ForceMode.Impulse);
		}
	}

	public void PlayParticles(Vector3 direction)
	{
		base.transform.GetChild(0).transform.rotation = Quaternion.LookRotation(direction);
		ParticleSystem[] componentsInChildren = base.transform.GetChild(0).GetComponentsInChildren<ParticleSystem>();
		foreach (ParticleSystem particleSystem in componentsInChildren)
		{
			particleSystem.Play();
		}
		float volumeScale = 1f;
		if (lifeCounter < 0.1f)
		{
			volumeScale = 0.2f;
		}
		if ((bool)au)
		{
			au.PlayOneShot(clips[Random.Range(0, clips.Length)], volumeScale);
			if (clips2.Length > 0)
			{
				au.PlayOneShot(clips2[Random.Range(0, clips2.Length)], volumeScale);
			}
		}
	}

	private IEnumerator DestroyAfterDelay(float f)
	{
		yield return new WaitForSeconds(f);
		Object.Destroy(base.gameObject);
	}
}
