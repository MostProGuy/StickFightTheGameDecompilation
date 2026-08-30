using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fighting : MonoBehaviour
{
	[Serializable]
	public class MoveTheseAsWell
	{
		public Rigidbody rig;

		public float multiplier;
	}

	public enum FightingState : byte
	{
		Idle = 0,
		Attacking = 1,
		Blocking = 2
	}

	public delegate void ProjectilePackageDelegate(Vector3 pos, Vector3 rot, ushort syncindex);

	public Weapon setThisWeapon;

	private Rigidbody weaponBody;

	public Weapon weapon;

	public Rigidbody UseThisRig;

	public MoveTheseAsWell[] moveTheseAsWell;

	private Rigidbody leftHand;

	private Rigidbody rightHand;

	private Rigidbody leftKnee;

	private Rigidbody leftElbow;

	private Rigidbody rightElbow;

	private Rigidbody hip;

	public float punchForce;

	public float upForceWhenWinding;

	public AnimationCurve punchCurve;

	public float punchTime;

	[HideInInspector]
	public float counter;

	private float punchCD;

	private bool hitLeft;

	public bool fullAuto;

	public float sincePunch;

	public float extraHitCD;

	private Transform aimPosition;

	private Transform aimPositionHelp;

	private Transform aimPositionPunch;

	private Vector3 aimingVector;

	private CharacterInformation info;

	private ConfigurableJoint joint1;

	private Weapons weapons;

	public float movementMultiplier = 1f;

	private AudioSource au;

	public AudioClip[] swings;

	public AudioClip[] throws;

	private int bulletsLeft;

	private ScreenshakeHandler screenShake;

	private BlockHandler blockHandler;

	private GrabHandler grabHandler;

	private Controller controller;

	private Rigidbody torso;

	private SetMovementAbility m_MovementAbility;

	public List<Controller> attackedTargets = new List<Controller>();

	public List<Rigidbody> attackedNonTargetRigs = new List<Rigidbody>();

	private CharacterStats stats;

	private GameObject lastPickedUpWeapon;

	public bool stopHit;

	public bool isSwinging;

	private bool windingUp;

	private bool isSwingingAtAll;

	private bool mJustAttacked;

	private byte m_WeaponIndex;

	private NetworkPlayer mNetworkPlayer;

	private ProjectilePackageDelegate mProjectileDelegate;

	private List<ProjectilePackageStruct> mProjectilePackages = new List<ProjectilePackageStruct>();

	private bool mRequestedThrow;

	public byte FightState
	{
		get
		{
			bool flag = mJustAttacked;
			if (flag)
			{
				mJustAttacked = false;
			}
			if (blockHandler.isBlocking)
			{
				return 2;
			}
			if (flag)
			{
				return 1;
			}
			return 0;
		}
	}

	public byte CurrentWeaponIndex
	{
		get
		{
			return m_WeaponIndex;
		}
	}

	private void Start()
	{
		aimPositionHelp = GetComponentInChildren<AimTargetHelper>().transform;
		au = GetComponentInChildren<AudioSource>();
		blockHandler = GetComponent<BlockHandler>();
		grabHandler = GetComponent<GrabHandler>();
		screenShake = ScreenshakeHandler.Instance;
		leftHand = GetComponentInChildren<LeftHand>().GetComponent<Rigidbody>();
		rightHand = GetComponentInChildren<RightHand>().GetComponent<Rigidbody>();
		leftElbow = GetComponentInChildren<LeftElbow>().GetComponent<Rigidbody>();
		rightElbow = GetComponentInChildren<RightElbow>().GetComponent<Rigidbody>();
		m_MovementAbility = GetComponent<SetMovementAbility>();
		LeftKnee componentInChildren = GetComponentInChildren<LeftKnee>();
		if ((bool)componentInChildren)
		{
			leftKnee = componentInChildren.GetComponent<Rigidbody>();
		}
		aimPosition = GetComponentInChildren<AimTarget>().transform;
		info = GetComponent<CharacterInformation>();
		Hip componentInChildren2 = GetComponentInChildren<Hip>();
		if ((bool)componentInChildren2)
		{
			hip = componentInChildren2.GetComponent<Rigidbody>();
		}
		if ((bool)setThisWeapon)
		{
			SetWeapon(setThisWeapon);
		}
		weapons = GetComponentInChildren<Weapons>();
		controller = GetComponent<Controller>();
		torso = GetComponentInChildren<Torso>().GetComponent<Rigidbody>();
		stats = GetComponent<CharacterStats>();
		mNetworkPlayer = GetComponentInParent<NetworkPlayer>();
		mProjectileDelegate = AddNewProjectilePackage;
	}

	private void FixedUpdate()
	{
		if (!weapon && blockHandler.isBlocking && info.sinceGrounded < 0.2f)
		{
			hip.velocity *= 0.5f;
		}
	}

	private void Update()
	{
		if (info.sinceFallen < 0f)
		{
			return;
		}
		counter += Time.deltaTime;
		sincePunch += Time.deltaTime;
		punchCD += Time.deltaTime;
		aimingVector = aimPositionHelp.forward;
		if ((bool)weapon && weapon.gradualRotationSpeed > 1000f)
		{
			aimingVector = aimPosition.forward;
		}
		if (!weapon)
		{
			if (grabHandler.isGrabbing)
			{
				HoldWeaponPosition(leftHand, aimPosition.position + aimPosition.forward * 0f, 50000f);
				HoldWeaponPosition(rightHand, aimPosition.position + aimPosition.forward * 0f, 50000f);
			}
			else if (blockHandler.isBlocking)
			{
				HoldWeaponPosition(leftHand, aimPosition.position - aimPosition.forward * 0.2f, 50000f);
				HoldWeaponPosition(rightHand, aimPosition.position - aimPosition.forward * 0.2f, 50000f);
			}
		}
		else
		{
			HoldWeaponPosition(weaponBody, aimPosition.position + aimPosition.forward * 0.05f, weapon.positionForce);
			HoldWeaponRotation(weaponBody, aimPosition.forward, weapon.rotationForce);
		}
	}

	public void Attack()
	{
		if (blockHandler.isBlocking)
		{
			return;
		}
		if ((bool)weapon && counter > weapon.cd)
		{
			counter = 0f;
			mJustAttacked = true;
			if (weapon.isGun)
			{
				weapon.Shoot(mProjectileDelegate);
				stats.bulletsShot++;
			}
			else
			{
				weapon.PlaySound();
				StartCoroutine(DoPunch(weaponBody, 0.5f * weapon.swingForceMultiplier, weapon.swingTime, weapon.curve));
				StartCoroutine(DoPunch(torso, 0.2f * weapon.swingForceMultiplier, weapon.swingTime, weapon.curve));
			}
			bulletsLeft--;
			if (bulletsLeft <= 0)
			{
				ThrowWeapon(true);
			}
		}
		else if (counter > 0.3f && !weapon && punchCD > 0f)
		{
			mJustAttacked = true;
			Punch();
			counter = 0f;
			punchCD = 0f - extraHitCD * UnityEngine.Random.Range(0.9f, 1.1f);
		}
	}

	private void HoldWeaponRotation(Rigidbody rig, Vector3 aimDir, float force)
	{
		if (weapon.swingTorque == 0f || !isSwingingAtAll)
		{
			if (weapon.inverseDirectionDuringWindup && windingUp)
			{
				aimDir = -aimDir;
			}
			float num = Vector3.Angle(rig.transform.forward, aimDir);
			Vector3 vector = Vector3.Cross(rig.transform.forward, aimDir);
			rig.AddTorque(vector * num * force * 1f * Time.deltaTime, ForceMode.Acceleration);
		}
	}

	private void HoldWeaponPosition(Rigidbody rig, Vector3 position, float force)
	{
		Vector3 vector = position - rig.worldCenterOfMass;
		rig.AddForce(vector * force * Time.deltaTime, ForceMode.Acceleration);
	}

	public void DropWeapon(bool networkSpawn = false)
	{
		if (!weapon)
		{
			return;
		}
		Vector3 position = new Vector3(0f, aimPosition.position.y, aimPosition.position.z) - aimPosition.forward * 0.3f;
		Vector3 vector = new Vector3(0f, base.transform.forward.y, base.transform.forward.z);
		if (MatchmakingHandler.IsNetworkMatch && networkSpawn)
		{
			if (!controller.HasControl)
			{
				Dissarm();
				return;
			}
			byte weaponIndex = m_WeaponIndex;
			mNetworkPlayer.DropWeapon(weaponIndex, position, vector);
			Dissarm();
			return;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(weapon.weaponDrop, position, Quaternion.LookRotation(vector));
		Rigidbody component = gameObject.GetComponent<Rigidbody>();
		Collider[] componentsInChildren = gameObject.GetComponentsInChildren<Collider>();
		foreach (Collider collider in componentsInChildren)
		{
			collider.gameObject.layer = base.gameObject.layer;
		}
		component.drag = 0.3f;
		gameObject.GetComponent<WeaponPickUp>().wasDroppped = true;
		gameObject.GetComponent<WeaponPickUp>().sinceDropCounter = 0.3f;
		gameObject.GetComponent<ConstantForce>().enabled = false;
		component.maxAngularVelocity = 100f;
		component.AddForce(Vector3.up * 10f, ForceMode.VelocityChange);
		component.AddTorque(Vector3.right * 50f, ForceMode.VelocityChange);
		Dissarm();
	}

	public void ThrowWeapon(bool justDrop)
	{
		if (!weapon)
		{
			return;
		}
		if (!justDrop)
		{
			Vector3 position = new Vector3(0f, aimPosition.position.y, aimPosition.position.z) - aimPosition.forward * 0.5f;
			Vector3 vector = new Vector3(0f, aimPosition.forward.y, aimPosition.forward.z);
			if (MatchmakingHandler.IsNetworkMatch && controller.HasControl)
			{
				if (!mRequestedThrow)
				{
					mRequestedThrow = true;
					byte weaponIndex = m_WeaponIndex;
					mNetworkPlayer.ThrowWeapon(justDrop, weaponIndex, position, vector, aimingVector);
				}
				return;
			}
			if (throws.Length > 0)
			{
				au.PlayOneShot(throws[UnityEngine.Random.Range(0, throws.Length)]);
			}
			GameObject gameObject = UnityEngine.Object.Instantiate(weapon.weaponDrop, position, Quaternion.LookRotation(vector));
			Rigidbody component = gameObject.GetComponent<Rigidbody>();
			gameObject.GetComponent<ConstantForce>().enabled = false;
			component.maxAngularVelocity = 100f;
			component.AddForce(aimingVector * 35f, ForceMode.VelocityChange);
			float num = 1f;
			if (aimingVector.z < 0f)
			{
				num = -1f;
			}
			if (weapon.spinWhenThrown)
			{
				component.AddTorque(Vector3.right * 50f * num, ForceMode.VelocityChange);
			}
			component.drag = 0.3f;
			gameObject.GetComponent<WeaponPickUp>().Throw(controller);
			gameObject.GetComponent<WeaponPickUp>().cantBePickledUpFor = 0.5f;
			Collider[] componentsInChildren = gameObject.GetComponentsInChildren<Collider>();
			foreach (Collider collider in componentsInChildren)
			{
				collider.gameObject.layer = base.gameObject.layer;
			}
			Dissarm();
			StartCoroutine(DoPunch(rightHand, 1f, punchTime, punchCurve));
			counter = 0f;
			screenShake.AddShake(aimPosition.forward * 0.3f);
		}
		else
		{
			Vector3 position2 = new Vector3(0f, aimPosition.position.y, aimPosition.position.z) - aimPosition.forward * 0.3f;
			Vector3 vector2 = new Vector3(0f, weapon.transform.rotation.eulerAngles.y, weapon.transform.rotation.eulerAngles.z);
			if (MatchmakingHandler.IsNetworkMatch && controller.HasControl)
			{
				byte weaponIndex2 = m_WeaponIndex;
				mNetworkPlayer.ThrowWeapon(justDrop, weaponIndex2, position2, vector2, default(Vector3));
			}
			GameObject gameObject2 = UnityEngine.Object.Instantiate(weapon.gameObject, position2, Quaternion.Euler(vector2));
			gameObject2.AddComponent<RemoveOnLevelChange>();
			UnityEngine.Object.Destroy(gameObject2.GetComponent<PunchForce>());
			UnityEngine.Object.Destroy(gameObject2.GetComponent<Weapon>());
			UnityEngine.Object.Destroy(gameObject2.GetComponent<BodyPart>());
			AudioSource component2 = gameObject2.GetComponent<AudioSource>();
			if ((bool)component2)
			{
				UnityEngine.Object.Destroy(component2);
			}
			Rigidbody component3 = gameObject2.GetComponent<Rigidbody>();
			component3.useGravity = true;
			component3.constraints = RigidbodyConstraints.FreezePositionX;
			component3.drag = 0f;
			component3.angularDrag = 0f;
			component3.maxAngularVelocity = 100f;
			component3.AddTorque(Vector3.right * 20f, ForceMode.VelocityChange);
			Dissarm();
			Collider[] componentsInChildren2 = gameObject2.GetComponentsInChildren<Collider>(true);
			foreach (Collider collider2 in componentsInChildren2)
			{
				collider2.enabled = false;
			}
		}
	}

	public void SetMovementAbility(int ability)
	{
		m_MovementAbility.SetAbility(ability);
	}

	public void ActivateWeapon(bool active)
	{
		weapon.isActive = active;
		mJustAttacked = active;
	}

	public void Dissarm()
	{
		Weapon[] componentsInChildren = GetComponentsInChildren<Weapon>();
		foreach (Weapon weapon in componentsInChildren)
		{
			weapon.gameObject.SetActive(false);
		}
		movementMultiplier = 1f;
		this.weapon = null;
		fullAuto = false;
		UnityEngine.Object.Destroy(joint1);
		m_WeaponIndex = 0;
		mRequestedThrow = false;
	}

	public void PickUpWeapon(int i, GameObject go)
	{
		if (go != null)
		{
			WeaponPickUp component = go.GetComponent<WeaponPickUp>();
			if ((bool)component)
			{
				component.cantBePickledUpFor = 2f;
			}
			if (!component || !component.unEnding)
			{
				if (go == lastPickedUpWeapon)
				{
					return;
				}
				lastPickedUpWeapon = go;
				if (i == 15)
				{
					SteamStatsAndAchievements.Instance.UnlockAchievement(SteamStatsAndAchievements.EAchievement.Lightsaber);
				}
			}
		}
		if ((bool)weapon)
		{
			DropWeapon(true);
		}
		SetWeapon(weapons.transform.GetChild(i).GetComponent<Weapon>());
		m_WeaponIndex = (byte)(i + 1);
	}

	private void SetWeapon(Weapon setWeapon)
	{
		if ((bool)weapon)
		{
			weapon.gameObject.SetActive(false);
		}
		if ((bool)joint1)
		{
			UnityEngine.Object.Destroy(joint1);
		}
		setWeapon.gameObject.SetActive(true);
		weaponBody = setWeapon.GetComponent<Rigidbody>();
		weapon = setWeapon;
		fullAuto = weapon.fullAuto;
		if (weapon.isEnergyBased)
		{
			fullAuto = true;
		}
		bulletsLeft = weapon.startBullets;
		movementMultiplier = weapon.movementMultiplier;
		StartCoroutine(GrabWeapon());
	}

	private IEnumerator GrabWeapon()
	{
		while (Vector3.Distance(rightHand.position, weaponBody.position) > 0.3f && (bool)weapon)
		{
			weapon.transform.position = aimPosition.position;
			weapon.transform.rotation = aimPosition.rotation;
			rightHand.AddForce((weaponBody.position - rightHand.transform.position) * 50000f * Time.deltaTime, ForceMode.Acceleration);
			yield return null;
		}
		if ((bool)weapon)
		{
			rightHand.transform.position = weapon.transform.GetChild(0).position;
			rightHand.transform.rotation = weapon.transform.GetChild(0).rotation;
			joint1 = AttachHand(rightHand, weaponBody);
		}
	}

	private ConfigurableJoint AttachHand(Rigidbody startRig, Rigidbody endRig)
	{
		ConfigurableJoint configurableJoint = startRig.gameObject.AddComponent<ConfigurableJoint>();
		configurableJoint.angularXMotion = ConfigurableJointMotion.Limited;
		configurableJoint.angularYMotion = ConfigurableJointMotion.Limited;
		configurableJoint.angularZMotion = ConfigurableJointMotion.Limited;
		SoftJointLimit angularYLimit = configurableJoint.angularYLimit;
		angularYLimit.limit = 30f;
		configurableJoint.angularYLimit = angularYLimit;
		configurableJoint.highAngularXLimit = angularYLimit;
		configurableJoint.angularZLimit = angularYLimit;
		angularYLimit.limit = -30f;
		configurableJoint.lowAngularXLimit = angularYLimit;
		configurableJoint.xMotion = ConfigurableJointMotion.Locked;
		configurableJoint.yMotion = ConfigurableJointMotion.Locked;
		configurableJoint.zMotion = ConfigurableJointMotion.Locked;
		configurableJoint.projectionMode = JointProjectionMode.PositionAndRotation;
		configurableJoint.anchor = startRig.transform.InverseTransformPoint(endRig.transform.position);
		configurableJoint.connectedBody = endRig;
		return configurableJoint;
	}

	private void DoFirstThings()
	{
	}

	private void Punch()
	{
		if ((bool)UseThisRig)
		{
			StartCoroutine(DoPunch(UseThisRig, 1.5f, punchTime, punchCurve));
			return;
		}
		if (info.sinceGrounded > 0.3f || info.sinceJumped < 0.4f)
		{
			StartCoroutine(DoPunch(leftKnee, 3f, punchTime, punchCurve));
			return;
		}
		if (hitLeft)
		{
			StartCoroutine(DoPunch(leftHand, 1.5f, punchTime, punchCurve));
		}
		else
		{
			StartCoroutine(DoPunch(rightHand, 1.5f, punchTime, punchCurve));
		}
		hitLeft = !hitLeft;
	}

	private IEnumerator DoPunch(Rigidbody rig, float multiplier, float timeToPunch, AnimationCurve curve)
	{
		isSwingingAtAll = true;
		bool hasSetPunch = false;
		float t = 0f;
		while (t < 1f && !info.isDead && !stopHit)
		{
			yield return new WaitForFixedUpdate();
			if (curve.Evaluate(t) > 0f && !hasSetPunch)
			{
				if (swings.Length > 0)
				{
					au.PlayOneShot(swings[UnityEngine.Random.Range(0, swings.Length)], 0.5f);
				}
				sincePunch = 0f;
				if ((bool)rig.GetComponent<PunchForce>())
				{
					rig.GetComponent<PunchForce>().SetPunch();
				}
				hasSetPunch = true;
				isSwinging = true;
				if ((bool)weapon && weapon.gameObject == rig.gameObject)
				{
					weapon.CallSwingEvent();
				}
				if ((bool)weapon && weapon.shootAnyway && weapon.gameObject == rig.gameObject)
				{
					weapon.Shoot();
				}
			}
			t += Time.fixedDeltaTime / timeToPunch;
			Vector3 dir = aimPosition.forward;
			if (curve.Evaluate(t) < 0f)
			{
				windingUp = true;
				dir = (dir + -Vector3.up * upForceWhenWinding).normalized;
			}
			screenShake.AddShake(punchForce * dir * 1E-05f * 0.0166f);
			rig.AddForce(dir * Time.fixedDeltaTime * multiplier * punchForce * curve.Evaluate(t), ForceMode.Acceleration);
			if ((bool)weapon && weapon.gameObject == rig.gameObject && weapon.swingTorque != 0f)
			{
				rig.AddTorque(aimPosition.right * Time.fixedDeltaTime * weapon.swingTorque * curve.Evaluate(t), ForceMode.Acceleration);
			}
			for (int i = 0; i < moveTheseAsWell.Length; i++)
			{
				moveTheseAsWell[i].rig.AddForce(dir * moveTheseAsWell[i].multiplier * Time.fixedDeltaTime * multiplier * punchForce * curve.Evaluate(t), ForceMode.Acceleration);
			}
			if ((bool)weapon)
			{
				for (int j = 0; j < weapon.moveTheseAsWelk.Length; j++)
				{
					weapon.moveTheseAsWelk[j].rig.AddForce(dir * weapon.moveTheseAsWelk[j].multiplier * Time.fixedDeltaTime * multiplier * punchForce * curve.Evaluate(t), ForceMode.Acceleration);
				}
			}
			torso.AddForce(dir * Time.fixedDeltaTime * multiplier * punchForce * 0.03f * curve.Evaluate(t), ForceMode.Acceleration);
			rig.velocity *= 0.7f;
		}
		stopHit = false;
		isSwinging = false;
		windingUp = false;
		isSwingingAtAll = false;
	}

	public void SetFightState(byte b)
	{
		switch ((FightingState)b)
		{
		case FightingState.Idle:
			blockHandler.EndBlock();
			if ((bool)weapon && weapon.isEnergyBased)
			{
				ActivateWeapon(false);
			}
			break;
		case FightingState.Attacking:
			blockHandler.EndBlock();
			if ((bool)weapon && weapon.isEnergyBased)
			{
				ActivateWeapon(true);
			}
			break;
		case FightingState.Blocking:
			blockHandler.StartBlock();
			if ((bool)weapon && weapon.isEnergyBased)
			{
				ActivateWeapon(false);
			}
			break;
		}
	}

	public void NetworkPickUpWeapon(byte index)
	{
		if (index != m_WeaponIndex)
		{
			if (index != 0)
			{
				PickUpWeapon(index - 1, null);
			}
			else
			{
				Dissarm();
			}
		}
	}

	public void NetworkThrowWeapon(bool justDrop, byte weaponIndex, Vector3 position, Vector3 rotation, Vector3 aimVector, ushort spawnIndex, ushort syncIndex)
	{
		if (!justDrop)
		{
			GameObject weaponDrop = weapons.transform.GetChild(weaponIndex - 1).GetComponent<Weapon>().weaponDrop;
			if (throws.Length > 0)
			{
				au.PlayOneShot(throws[UnityEngine.Random.Range(0, throws.Length)]);
			}
			GameObject gameObject = UnityEngine.Object.Instantiate(weaponDrop, position, Quaternion.LookRotation(rotation));
			UnityEngine.Object.FindObjectOfType<MultiplayerManager>().OnWeaponSpawned(gameObject.GetComponent<WeaponPickUp>(), spawnIndex, syncIndex);
			Rigidbody component = gameObject.GetComponent<Rigidbody>();
			gameObject.GetComponent<ConstantForce>().enabled = false;
			component.maxAngularVelocity = 100f;
			component.AddForce(aimVector * 35f, ForceMode.VelocityChange);
			float num = 1f;
			if (aimVector.z < 0f)
			{
				num = -1f;
			}
			if (weapon != null && weapon.spinWhenThrown)
			{
				component.AddTorque(Vector3.right * 50f * num, ForceMode.VelocityChange);
			}
			component.drag = 0.3f;
			gameObject.GetComponent<WeaponPickUp>().Throw(controller);
			gameObject.GetComponent<WeaponPickUp>().cantBePickledUpFor = 0.5f;
			Collider[] componentsInChildren = gameObject.GetComponentsInChildren<Collider>();
			foreach (Collider collider in componentsInChildren)
			{
				collider.gameObject.layer = base.gameObject.layer;
			}
			Dissarm();
			StartCoroutine(DoPunch(rightHand, 1f, punchTime, punchCurve));
			counter = 0f;
			screenShake.AddShake(aimPosition.forward * 0.3f);
		}
		else
		{
			GameObject original = weapons.transform.GetChild(weaponIndex - 1).GetComponent<Weapon>().gameObject;
			GameObject gameObject2 = UnityEngine.Object.Instantiate(original, position, Quaternion.Euler(rotation));
			gameObject2.AddComponent<RemoveOnLevelChange>();
			UnityEngine.Object.Destroy(gameObject2.GetComponent<PunchForce>());
			UnityEngine.Object.Destroy(gameObject2.GetComponent<Weapon>());
			UnityEngine.Object.Destroy(gameObject2.GetComponent<BodyPart>());
			AudioSource component2 = gameObject2.GetComponent<AudioSource>();
			if ((bool)component2)
			{
				UnityEngine.Object.Destroy(component2);
			}
			Rigidbody component3 = gameObject2.GetComponent<Rigidbody>();
			component3.useGravity = true;
			component3.constraints = RigidbodyConstraints.FreezePositionX;
			component3.drag = 0f;
			component3.angularDrag = 0f;
			component3.maxAngularVelocity = 100f;
			component3.AddTorque(Vector3.right * 20f, ForceMode.VelocityChange);
			Dissarm();
			Collider[] componentsInChildren2 = gameObject2.GetComponentsInChildren<Collider>(true);
			foreach (Collider collider2 in componentsInChildren2)
			{
				collider2.enabled = false;
			}
		}
	}

	public void AddNewProjectilePackage(Vector3 position, Vector3 rotation, ushort syncindex)
	{
		if (!controller.HasControl)
		{
			Debug.LogError("Trying to Add projectile packages to a non controlling client");
			return;
		}
		ProjectilePackageStruct item = new ProjectilePackageStruct
		{
			shootPosition = new ShortVector2(position),
			shootVector = new ByteVector2(rotation),
			syncIndex = syncindex
		};
		mProjectilePackages.Add(item);
		Debug.Log(string.Concat("Adding new projectile package: ", item.shootPosition, " : ", item.shootVector));
	}

	public ProjectilePackageStruct[] GetProjectilePackages()
	{
		ProjectilePackageStruct[] result = mProjectilePackages.ToArray();
		mProjectilePackages.Clear();
		return result;
	}

	public void FirePackages(ProjectilePackageStruct[] packages, byte weaponType)
	{
		if (packages.Length > 0)
		{
			StartCoroutine(PackageFireEnumerator(packages, weaponType));
			return;
		}
		Attack();
		NetworkPickUpWeapon(weaponType);
	}

	private IEnumerator PackageFireEnumerator(ProjectilePackageStruct[] packages, byte weaponToSwitchToAfter)
	{
		int len = packages.Length;
		for (int i = 0; i < len; i++)
		{
			ProjectilePackageStruct package = packages[i];
			Vector3 shootVectorOverride = package.shootVector.ToVector3();
			Vector3 shootPositionOverride = package.shootPosition.ToVector3();
			if ((bool)weapon)
			{
				weapon.Shoot(null, true, shootVectorOverride, shootPositionOverride, package.syncIndex);
				Debug.Log(string.Concat("Firing Projectile Package: ", shootPositionOverride, " : ", shootVectorOverride));
			}
			else
			{
				Debug.LogWarning("Trying to fire null weapon");
			}
			yield return new WaitForEndOfFrame();
		}
		NetworkPickUpWeapon(weaponToSwitchToAfter);
	}
}
