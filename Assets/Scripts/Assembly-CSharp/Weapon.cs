using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Weapon : MonoBehaviour
{
	[SerializeField]
	private byte m_Rarity;

	public AudioSource loopSound;

	public AudioClip[] clips;

	public AudioClip[] drawSounds;

	public AudioClip[] dropSound;

	public GameObject weaponDrop;

	public float movementMultiplier = 1f;

	[Header("Gun Stuff")]
	public bool isGun;

	public bool shootAnyway;

	public bool isCharged;

	public bool isEnergyBased;

	public bool fullAuto;

	public float cd = 0.1f;

	public float spread;

	public GameObject projectile;

	public float screenShakeAmount = 5f;

	public float maxChargeTime = 1f;

	[HideInInspector]
	public float currentCharge;

	private ScreenshakeHandler screenShake;

	private ParticleSystem part;

	private Transform shootPosition;

	private Transform aimTarget;

	public float recoil;

	public float torsoRecoil;

	public int shotsBeforeReload;

	public float reloadTime;

	public bool shootAllBullets;

	private bool isSpamming;

	private int currentShots;

	private bool reloads;

	private Rigidbody rig;

	private Rigidbody rigTorso;

	private Controller controller;

	public int startBullets;

	[HideInInspector]
	public float sinceShot = 5f;

	private AudioSource au;

	private Fighting fighting;

	public bool isBossOnly;

	public Material ghostMat;

	[Header("Melee Stuff")]
	public bool inverseDirectionDuringWindup;

	public float positionForce = 20000f;

	public float rotationForce = 100000f;

	public float swingForceMultiplier = 1f;

	public float gradualRotationSpeed = 10001f;

	public float swingTime;

	public float swingTorque;

	public AnimationCurve curve;

	public Fighting.MoveTheseAsWell[] moveTheseAsWelk;

	public UnityEvent swingEvent;

	private bool canShoot = true;

	public UnityEvent readyToShootEvent;

	public GameObject[] neededObjects;

	public bool spinWhenThrown = true;

	[Header("Energy Stuff")]
	public float secondsOfUse = 5f;

	[HideInInspector]
	public bool isActive;

	[HideInInspector]
	public float secondsOfUseLeft;

	private Standing stand;

	public float volumeFix = 1f;

	public float pitchFix = 1f;

	public bool canNoChangePitch;

	public float shoodDelay;

	public bool fixAim;

	public BossTimer bossTimer;

	public string phaseName = string.Empty;

	public int switchToWeaponOnDisable = -1;

	public float loseWeaponAfter;

	[HideInInspector]
	public float loseWeaponCurrentTime;

	public bool loseWeaponOnReload;

	public float cantShootNextFor;

	private bool losesWeaponAfterTime;

	public byte Rarity
	{
		get
		{
			return m_Rarity;
		}
	}

	private void OnDisable()
	{
		if ((bool)au && dropSound.Length > 0)
		{
			au.PlayOneShot(dropSound[Random.Range(0, dropSound.Length)]);
		}
		GameObject[] array = neededObjects;
		foreach (GameObject gameObject in array)
		{
			gameObject.SetActive(false);
		}
		if ((bool)bossTimer)
		{
			bossTimer.ConnectWeapon(null, string.Empty);
		}
	}

	private void Awake()
	{
		if (shotsBeforeReload != 0)
		{
			reloads = true;
		}
		screenShake = ScreenshakeHandler.Instance;
		part = GetComponentInChildren<ParticleSystem>();
		rig = GetComponent<Rigidbody>();
		stand = base.transform.root.GetComponent<Standing>();
		if (loseWeaponAfter != 0f)
		{
			losesWeaponAfterTime = true;
		}
		controller = base.transform.root.GetComponent<Controller>();
		fighting = base.transform.root.GetComponent<Fighting>();
		au = base.transform.root.GetComponentInChildren<AudioSource>();
		Shoot componentInChildren = base.transform.GetComponentInChildren<Shoot>();
		if ((bool)componentInChildren)
		{
			shootPosition = componentInChildren.transform;
		}
		Torso componentInChildren2 = base.transform.root.GetComponentInChildren<Torso>();
		if ((bool)componentInChildren2)
		{
			rigTorso = componentInChildren2.GetComponent<Rigidbody>();
		}
		AimTarget componentInChildren3 = base.transform.root.GetComponentInChildren<AimTarget>();
		if ((bool)componentInChildren3)
		{
			aimTarget = componentInChildren3.transform;
		}
	}

	public void CallSwingEvent()
	{
		swingEvent.Invoke();
	}

	private void OnEnable()
	{
		if (drawSounds.Length > 0 && au != null)
		{
			au.PlayOneShot(drawSounds[Random.Range(0, drawSounds.Length)]);
		}
		secondsOfUseLeft = secondsOfUse;
		if ((bool)fighting && fighting.counter > 0f)
		{
			readyToShootEvent.Invoke();
		}
		else if (loseWeaponAfter != 0f)
		{
			swingEvent.Invoke();
			canShoot = false;
			sinceShot = fighting.counter;
		}
		loseWeaponCurrentTime = 0f;
		if ((bool)bossTimer)
		{
			bossTimer.ConnectWeapon(this, phaseName);
		}
		GameObject[] array = neededObjects;
		foreach (GameObject gameObject in array)
		{
			gameObject.SetActive(true);
		}
	}

	private void Disarm()
	{
		if (cantShootNextFor != 0f)
		{
			fighting.counter = 0f - cantShootNextFor;
		}
		loseWeaponCurrentTime = 0f;
		fighting.Dissarm();
		if (switchToWeaponOnDisable != -1)
		{
			fighting.PickUpWeapon(switchToWeaponOnDisable, null);
		}
	}

	private void Update()
	{
		if (losesWeaponAfterTime && fighting.counter > 0f)
		{
			loseWeaponCurrentTime += Time.deltaTime;
			if (loseWeaponCurrentTime > loseWeaponAfter)
			{
				Disarm();
			}
		}
		if (isSpamming && fighting.counter > cd)
		{
			fighting.counter = 0f;
			ActuallyShoot();
		}
		if ((bool)loopSound)
		{
			if (sinceShot < 0.11f || isActive)
			{
				loopSound.volume = Mathf.Lerp(loopSound.volume, 1f, Time.deltaTime * 30f);
			}
			else
			{
				loopSound.volume = Mathf.Lerp(loopSound.volume, 0f, Time.deltaTime * 30f);
			}
		}
		sinceShot += Time.deltaTime;
		if (sinceShot > cd && !canShoot)
		{
			canShoot = true;
			readyToShootEvent.Invoke();
		}
		if (secondsOfUseLeft < 0f)
		{
			fighting.Dissarm();
		}
		if (isActive)
		{
			secondsOfUseLeft -= Time.deltaTime;
			screenShake.AddShake((base.transform.forward + Random.insideUnitSphere) * Time.deltaTime * screenShakeAmount);
		}
	}

	public void PlaySound()
	{
		if (clips.Length > 0)
		{
			if (canNoChangePitch)
			{
				au.pitch = 1f;
				au.volume = 1f;
			}
			else
			{
				au.pitch = Random.Range(0.95f, 1.05f) * pitchFix;
				au.volume = Random.Range(0.95f, 1.05f) * pitchFix;
			}
			au.PlayOneShot(clips[Random.Range(0, clips.Length)], volumeFix);
		}
	}

	public void Shoot(Fighting.ProjectilePackageDelegate callBackMethod = null, bool networkForce = false, Vector3 shootVectorOverride = default(Vector3), Vector3 shootPositionOverride = default(Vector3), ushort syncIndex = ushort.MaxValue)
	{
		if (shootAllBullets)
		{
			isSpamming = true;
		}
		if (shoodDelay == 0f)
		{
			ActuallyShoot(callBackMethod, networkForce, shootVectorOverride, shootPositionOverride, syncIndex);
		}
		else
		{
			StartCoroutine(ShootAfterDelay(callBackMethod, networkForce, shootVectorOverride, shootPositionOverride, syncIndex));
		}
	}

	private IEnumerator ShootAfterDelay(Fighting.ProjectilePackageDelegate callBackMethod = null, bool networkForce = false, Vector3 shootVectorOverride = default(Vector3), Vector3 shootPositionOverride = default(Vector3), ushort syncIndex = ushort.MaxValue)
	{
		yield return new WaitForSeconds(shoodDelay);
		ActuallyShoot(callBackMethod, networkForce, shootVectorOverride, shootPositionOverride, syncIndex);
	}

	private void ActuallyShoot(Fighting.ProjectilePackageDelegate callBackMethod = null, bool networkForce = false, Vector3 shootVectorOverride = default(Vector3), Vector3 shootPositionOverride = default(Vector3), ushort syncIndex = ushort.MaxValue)
	{
		sinceShot = 0f;
		if (reloads)
		{
			currentShots++;
			if (currentShots >= shotsBeforeReload)
			{
				fighting.counter = 0f - reloadTime;
				sinceShot = 0f - reloadTime;
				currentShots = 0;
				isSpamming = false;
				if (loseWeaponOnReload)
				{
					Disarm();
				}
			}
		}
		CallSwingEvent();
		canShoot = false;
		Vector3 vector = ((!networkForce) ? new Vector3(0f, base.transform.forward.y + Random.Range(0f - spread, spread), base.transform.forward.z + Random.Range(0f - spread, spread)) : shootVectorOverride);
		if (fixAim)
		{
			vector = aimTarget.forward;
		}
		vector.x = 0f;
		Vector3 vector2 = ((!networkForce) ? shootPosition.position : shootPositionOverride);
		GameObject gameObject = Object.Instantiate(projectile, vector2, Quaternion.LookRotation(vector));
		if (MatchmakingHandler.IsNetworkMatch && (bool)gameObject.GetComponent<SnakeAI>())
		{
			NetworkSyncableObject component = gameObject.GetComponent<NetworkSyncableObject>();
			if ((bool)component)
			{
				MultiplayerManager multiplayerManager = Object.FindObjectOfType<MultiplayerManager>();
				if (syncIndex == ushort.MaxValue)
				{
					syncIndex = multiplayerManager.GetNextSyncableObjectSpawnID();
				}
				else
				{
					multiplayerManager.AddSyncableObject(syncIndex, component);
				}
				component.InitNetworkIndex(syncIndex, false);
			}
		}
		Vector3 pos = vector2;
		Vector3 rot = vector;
		gameObject.transform.position = new Vector3(0f, gameObject.transform.position.y, gameObject.transform.position.z);
		screenShake.AddShake(vector.normalized * screenShakeAmount);
		rig.AddForce(-vector * recoil, ForceMode.VelocityChange);
		if ((bool)rigTorso)
		{
			rigTorso.AddForce(-vector * torsoRecoil, ForceMode.VelocityChange);
		}
		stand.gravity -= Mathf.Clamp((-vector * 0.003f * torsoRecoil).y, 0f, 100f);
		stand.gravity = Mathf.Clamp(stand.gravity, 0f, 1000f);
		RayCastForward[] componentsInChildren = gameObject.GetComponentsInChildren<RayCastForward>();
		foreach (RayCastForward rayCastForward in componentsInChildren)
		{
			if (!rayCastForward.dontChangeMask)
			{
				rayCastForward.mask = 1 << controller.playerID + 8;
				rayCastForward.mask = ~(int)rayCastForward.mask;
			}
			if (isCharged)
			{
				rayCastForward.speed *= currentCharge / maxChargeTime;
			}
			if (!isBossOnly)
			{
				continue;
			}
			bool flag = false;
			foreach (Controller item in GameManager.Instance.playersAlive)
			{
				if (item.canFly && !item.GetComponent<CharacterInformation>().isDead)
				{
					flag = true;
				}
			}
			if (flag)
			{
				rayCastForward.onlyBosses = true;
				TrailRenderer[] componentsInChildren2 = rayCastForward.GetComponentsInChildren<TrailRenderer>();
				foreach (TrailRenderer trailRenderer in componentsInChildren2)
				{
					trailRenderer.sharedMaterial = ghostMat;
				}
			}
		}
		ProjectileCollision[] componentsInChildren3 = gameObject.GetComponentsInChildren<ProjectileCollision>();
		foreach (ProjectileCollision projectileCollision in componentsInChildren3)
		{
			projectileCollision.damager = controller;
			if (isCharged)
			{
				projectileCollision.damage *= currentCharge / maxChargeTime;
				projectileCollision.force *= currentCharge / maxChargeTime;
			}
		}
		SpawnObjectAfterDelay[] componentsInChildren4 = gameObject.GetComponentsInChildren<SpawnObjectAfterDelay>();
		foreach (SpawnObjectAfterDelay spawnObjectAfterDelay in componentsInChildren4)
		{
			spawnObjectAfterDelay.damager = controller;
		}
		Collider[] componentsInChildren5 = gameObject.GetComponentsInChildren<Collider>();
		foreach (Collider collider in componentsInChildren5)
		{
			collider.gameObject.layer = base.transform.gameObject.layer;
		}
		DamagerHolder[] componentsInChildren6 = gameObject.GetComponentsInChildren<DamagerHolder>();
		foreach (DamagerHolder damagerHolder in componentsInChildren6)
		{
			damagerHolder.damager = controller;
		}
		gameObject.AddComponent<TeamHolder>().team = controller.playerID;
		if ((bool)part)
		{
			part.Play();
		}
		PlaySound();
		if (!networkForce && callBackMethod != null)
		{
			callBackMethod(pos, rot, syncIndex);
		}
	}
}
