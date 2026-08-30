using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PunchForce : MonoBehaviour
{
	public float damageMultiplier = 1f;

	public float forceMultiplier = 1f;

	private Transform aimDir;

	private ScreenshakeHandler screenShake;

	private Fighting fighting;

	private ParticleSystem damagePart;

	private float effectCd;

	private AudioSource au;

	public AudioClip[] clips;

	public AudioClip[] clipsBlock;

	private ParticleSystem[] parryParticle;

	private ParticleSystem punchParticle;

	public PunchForce[] connectedPuncers;

	public bool unblockable;

	private CharacterInformation info;

	private Standing stand;

	private Rigidbody rig;

	private Rigidbody torso;

	private Movement move;

	private CharacterInformation myInfo;

	private Controller controller;

	public bool onlyMe;

	private float meCounter;

	public UnityEvent groundCollisionEvent;

	public bool stopHitOnGroundCollision;

	private bool hasHitGroundYet;

	public PlayParticles GroundCollisionParticles;

	private void Start()
	{
		damagePart = base.transform.root.GetComponentInChildren<DamageParticle>().GetComponent<ParticleSystem>();
		connectedPuncers = base.transform.root.GetComponentsInChildren<PunchForce>();
		punchParticle = base.transform.root.GetComponentInChildren<PunchParticle>().GetComponent<ParticleSystem>();
		aimDir = base.transform.root.GetComponentInChildren<AimTarget>().transform;
		fighting = base.transform.root.GetComponent<Fighting>();
		screenShake = ScreenshakeHandler.Instance;
		au = base.transform.root.GetComponentInChildren<AudioSource>();
		myInfo = base.transform.root.GetComponent<CharacterInformation>();
	}

	private void Update()
	{
		meCounter += Time.deltaTime;
		effectCd += Time.deltaTime;
		parryParticle = base.transform.root.GetComponentInChildren<ParryParticle>().GetComponentsInChildren<ParticleSystem>();
		info = base.transform.root.GetComponent<CharacterInformation>();
		stand = base.transform.root.GetComponent<Standing>();
		controller = base.transform.root.GetComponent<Controller>();
		torso = base.transform.root.GetComponentInChildren<Torso>().GetComponent<Rigidbody>();
		rig = GetComponent<Rigidbody>();
	}

	public void SetPunch()
	{
		punchParticle.transform.position = base.transform.position;
		punchParticle.transform.rotation = Quaternion.LookRotation(aimDir.forward);
		punchParticle.Play();
		fighting.attackedTargets.Clear();
		fighting.attackedNonTargetRigs.Clear();
		meCounter = 0f;
		hasHitGroundYet = false;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.relativeVelocity.magnitude < 5f || collision.transform.root == base.transform.root || (fighting.sincePunch > 0.2f && fighting.punchTime < 1f) || (fighting.punchTime > 1f && !fighting.isSwinging))
		{
			return;
		}
		if (!collision.rigidbody)
		{
			if (!hasHitGroundYet && collision.gameObject.layer != 26 && collision.gameObject.layer != 27)
			{
				if (stopHitOnGroundCollision)
				{
					fighting.stopHit = true;
				}
				if ((bool)GroundCollisionParticles)
				{
					GroundCollisionParticles.MoveAndGo(collision.contacts[0].point);
				}
				groundCollisionEvent.Invoke();
				hasHitGroundYet = true;
			}
		}
		else
		{
			if (myInfo.isDead || (onlyMe && meCounter > fighting.punchTime))
			{
				return;
			}
			Controller component = collision.transform.root.GetComponent<Controller>();
			if (!component)
			{
				if (fighting.attackedNonTargetRigs.Contains(collision.rigidbody))
				{
					return;
				}
				fighting.attackedNonTargetRigs.Add(collision.rigidbody);
			}
			else
			{
				if (fighting.attackedTargets.Contains(component))
				{
					return;
				}
				fighting.attackedTargets.Add(component);
			}
			BlockHandler component2 = collision.transform.root.GetComponent<BlockHandler>();
			float num = 1f;
			num = ((!component2 || !component2.isBlocking || unblockable || !(Vector3.Angle(-component2.blockAngle, aimDir.forward) < component2.blockForce * 40f)) ? 1f : 0.5f);
			float num2 = 15f * num;
			if (!component2 || unblockable || (!component2.isBlocking && component2.sinceBlockStart > 0.3f))
			{
				Rigidbody rigidbody = collision.rigidbody;
				Vector3 force = num2 * forceMultiplier * aimDir.forward * 100f;
				if (collision.rigidbody.mass < 300f)
				{
					collision.rigidbody.velocity *= 0.3f;
				}
				if (MatchmakingHandler.IsNetworkMatch)
				{
					if ((bool)controller && controller.HasControl)
					{
						RigidBodyIndexHolder component3 = rigidbody.GetComponent<RigidBodyIndexHolder>();
						if ((bool)component3)
						{
							if ((bool)component)
							{
								byte index = rigidbody.GetComponent<RigidBodyIndexHolder>().Index;
								NetworkPlayer component4 = component.GetComponent<NetworkPlayer>();
								if ((bool)component4)
								{
									component4.SendAddedForce(index, force, ForceMode.Impulse);
									rigidbody.AddForce(force, ForceMode.Impulse);
									component4.DelaySyncingBy(1);
								}
							}
							else
							{
								byte index2 = component3.Index;
								NetworkSyncableObject component5 = rigidbody.transform.root.GetComponent<NetworkSyncableObject>();
								if ((bool)component5)
								{
									component5.ForceAdded(index2, force, ForceMode.Impulse);
									rigidbody.AddForce(force, ForceMode.Impulse);
								}
							}
						}
					}
				}
				else
				{
					rigidbody.AddForce(force, ForceMode.Impulse);
				}
			}
			if (!(effectCd > 0.2f) || !component)
			{
				return;
			}
			Fighting component6 = collision.transform.root.GetComponent<Fighting>();
			screenShake.AddShake(num2 * aimDir.forward * 0.01f);
			if ((bool)component2 && component2.isBlocking && component2.sinceBlockStart < 0.3f && !unblockable && num < 1f)
			{
				fighting.counter = -1.5f;
				Vector3 force2 = num2 * -aimDir.forward * 1000f;
				if (MatchmakingHandler.IsNetworkMatch)
				{
					if (controller.HasControl)
					{
						byte index3 = torso.GetComponent<RigidBodyIndexHolder>().Index;
						NetworkPlayer component7 = torso.transform.root.GetComponent<NetworkPlayer>();
						component7.SendAddedForceAndPlayBlock(index3, force2, ForceMode.Impulse, (byte)collision.transform.root.GetComponent<NetworkPlayer>().NetworkSpawnID);
						torso.AddForce(force2, ForceMode.Impulse);
						component2.DoBlock();
						component2.GetComponent<CharacterStats>().blocks++;
					}
				}
				else
				{
					torso.AddForce(force2, ForceMode.Impulse);
					component2.DoBlock();
					component2.GetComponent<CharacterStats>().blocks++;
				}
				return;
			}
			if ((bool)component6 && ((component6.sincePunch > fighting.sincePunch && (double)component6.sincePunch < 0.3) || (component6.sincePunch < 0.05f && component6.sincePunch < 0.05f)) && !unblockable)
			{
				if (clipsBlock.Length > 0)
				{
					au.PlayOneShot(clipsBlock[Random.Range(0, clipsBlock.Length)]);
				}
				ParticleSystem[] componentsInChildren = collision.transform.root.GetComponentInChildren<ParryParticle>().GetComponentsInChildren<ParticleSystem>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].transform.position = collision.contacts[0].point + Random.insideUnitSphere * 0.3f;
					componentsInChildren[i].Play();
				}
				base.transform.root.GetComponentInChildren<Torso>().GetComponent<Rigidbody>().AddForce(num2 * (0f - forceMultiplier) * aimDir.forward * 200f, ForceMode.Impulse);
				return;
			}
			effectCd = 0f;
			BodyPart component8 = collision.rigidbody.GetComponent<BodyPart>();
			if ((bool)component8)
			{
				if (!MatchmakingHandler.IsNetworkMatch || controller.HasControl)
				{
					if (clips.Length > 0)
					{
						au.PlayOneShot(clips[Random.Range(0, clips.Length)]);
					}
					screenShake.AddShake(num2 * aimDir.forward * 0.02f);
				}
				bool flag = component8.health.health <= 0f;
				component8.TakeDamageWithParticle(num2 * 1.5f * damageMultiplier, collision.contacts[0].point, aimDir.forward, true, controller);
				if (component8.health.health <= 0f && !flag)
				{
					bool flag2 = collision.transform.root.GetComponent<SnakeAI>() != null;
					CharacterInformation component9 = controller.GetComponent<CharacterInformation>();
					if (base.gameObject.name.Contains("Leg") || (base.gameObject.name.Contains("Knee") && !component9.isGrounded && !flag2))
					{
						SteamStatsAndAchievements.Instance.UnlockAchievement(SteamStatsAndAchievements.EAchievement.YourKungFuIsStrong);
					}
				}
				GetComponentInParent<CharacterStats>().punchesLanded++;
				Standing component10 = component8.transform.root.GetComponent<Standing>();
				if ((bool)component10)
				{
					component10.gravity = 0f;
				}
				fighting.counter = 0.1f;
				Transform root = collision.transform.root;
				Rigidbody component11 = root.GetComponentInChildren<Torso>().GetComponent<Rigidbody>();
				Rigidbody component12 = base.transform.root.GetComponentInChildren<Torso>().GetComponent<Rigidbody>();
				Vector3 force3 = num2 * aimDir.forward * 100f;
				Vector3 force4 = num2 * forceMultiplier * component8.weakness * aimDir.forward * 200f;
				if (MatchmakingHandler.IsNetworkMatch)
				{
					if (controller.HasControl)
					{
						byte b = 0;
						NetworkPlayer component13 = root.GetComponent<NetworkPlayer>();
						RigidBodyIndexHolder component14 = component11.GetComponent<RigidBodyIndexHolder>();
						if ((bool)component14)
						{
							b = component14.Index;
							if ((bool)component13)
							{
								component13.SendAddedForce(b, force4, ForceMode.Impulse);
							}
						}
						else
						{
							Debug.LogWarning("Rig missing index holder");
						}
						byte index4 = component12.GetComponent<RigidBodyIndexHolder>().Index;
						controller.GetComponent<NetworkPlayer>().SendAddedForce(index4, force3, ForceMode.Impulse);
						if ((bool)component13)
						{
							component13.DelaySyncingBy(1);
						}
						component11.AddForce(force4, ForceMode.Impulse);
						component12.AddForce(force3, ForceMode.Impulse);
					}
				}
				else
				{
					component11.AddForce(force4, ForceMode.Impulse);
					component12.AddForce(force3, ForceMode.Impulse);
				}
			}
			info.sinceGrounded = 0f;
			stand.gravity = 0f;
		}
	}

	private IEnumerator HitStop(float stop)
	{
		Time.timeScale = 0f;
		yield return new WaitForSecondsRealtime(stop);
		Time.timeScale = 1f;
	}

	public void PlayPunchSound()
	{
		if (clips.Length > 0)
		{
			au.PlayOneShot(clips[Random.Range(0, clips.Length)]);
		}
		screenShake.AddShake(15f * aimDir.forward * 0.02f);
	}
}
