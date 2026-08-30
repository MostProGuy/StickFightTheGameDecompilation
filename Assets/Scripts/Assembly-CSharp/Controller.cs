using System;
using InControl;
using UnityEngine;

public class Controller : MonoBehaviour
{
	[Flags]
	public enum MovementStateEnum : byte
	{
		None = 0,
		Left = 1,
		Right = 2,
		WallJump = 4,
		GroundJump = 8
	}

	public enum EMemoryBucketType
	{
		Transient = 0,
		Persistent = 1
	}

	public static bool allowMouse = true;

	private Movement movement;

	private Rotation rotation;

	private Animations animation;

	private CharacterInformation info;

	public bool isAI;

	private Transform playerTarget;

	private Fighting fighting;

	private CharacterActions mPlayerActions;

	private Transform aimer;

	private Transform helpAimer;

	private BlockHandler blockHandler;

	public int playerID;

	private ParticleSystem jumpPart;

	private Transform torso;

	public Controller damager;

	public GrabHandler grabHandler;

	public bool inactive;

	public bool canFly;

	public float flySpeed = 1f;

	private static bool mIsNetworkMatch;

	private bool mHasControl;

	private MovementStateEnum m_MovementState;

	private Vector2 mLookRotation;

	private MemoryBucket TransientMemory = new MemoryBucket();

	private MemoryBucket PersistentMemory = new MemoryBucket();

	private float m_KickCounter;

	private const float m_KickTime = 120f;

	public CharacterActions PlayerActions
	{
		get
		{
			return mPlayerActions;
		}
	}

	public bool HasControl
	{
		get
		{
			return mHasControl;
		}
	}

	public byte MovementState
	{
		get
		{
			byte movementState = (byte)m_MovementState;
			m_MovementState = MovementStateEnum.None;
			return movementState;
		}
	}

	public Vector2 LookRotation
	{
		get
		{
			return mLookRotation;
		}
	}

	public MemoryBucket GetMemoryBucket(EMemoryBucketType bucketType)
	{
		return (bucketType != EMemoryBucketType.Persistent) ? TransientMemory : PersistentMemory;
	}

	private void Start()
	{
		if (inactive)
		{
			return;
		}
		rotation = GetComponent<Rotation>();
		movement = GetComponent<Movement>();
		animation = GetComponent<Animations>();
		info = GetComponent<CharacterInformation>();
		fighting = GetComponent<Fighting>();
		blockHandler = GetComponent<BlockHandler>();
		grabHandler = GetComponent<GrabHandler>();
		aimer = GetComponentInChildren<AimTarget>().transform.parent;
		helpAimer = GetComponentInChildren<AimTargetHelper>().transform.parent;
		jumpPart = base.transform.GetComponentInChildren<JumpParticle>().GetComponent<ParticleSystem>();
		torso = base.transform.GetComponentInChildren<Torso>().transform;
		if (isAI)
		{
			GetComponent<AI>().enabled = true;
			return;
		}
		MatchmakingHandler instance = MatchmakingHandler.Instance;
		mIsNetworkMatch = !(instance == null) && instance.IsInsideLobby;
		MatchmakingHandler.SetNetworkMatch(mIsNetworkMatch);
		if (!mIsNetworkMatch)
		{
			mHasControl = true;
		}
		TransientMemory.OnValueChanged<int>("RoundKills", OnRoundKillCounterChanged);
		TransientMemory.OnValueChanged<int>("MultiKill", OnMultiKill);
		PersistentMemory.OnValueChanged<int>("KillingSpree", OnKillingSpreeChanged);
	}

	private void OnDisabled()
	{
	}

	public void OnRoundKillCounterChanged(string key, int newValue)
	{
		if (newValue >= 3)
		{
			SteamStatsAndAchievements.Instance.UnlockAchievement(SteamStatsAndAchievements.EAchievement.Ace);
		}
		if (!(TransientMemory.Copy<float>("DamageTaken").GetValue(0f) > 0f) && newValue >= 3)
		{
			SteamStatsAndAchievements.Instance.UnlockAchievement(SteamStatsAndAchievements.EAchievement.RoyalAce);
		}
	}

	public void OnMultiKill(string key, int killCount)
	{
		switch (killCount)
		{
		case 2:
			SteamStatsAndAchievements.Instance.UnlockAchievement(SteamStatsAndAchievements.EAchievement.DoubleKill);
			break;
		case 3:
			SteamStatsAndAchievements.Instance.UnlockAchievement(SteamStatsAndAchievements.EAchievement.TripleKill);
			break;
		}
	}

	public void OnKillingSpreeChanged(string key, int newValue)
	{
		switch (newValue)
		{
		case 4:
			SteamStatsAndAchievements.Instance.UnlockAchievement(SteamStatsAndAchievements.EAchievement.KillingSpree);
			break;
		case 5:
			SteamStatsAndAchievements.Instance.UnlockAchievement(SteamStatsAndAchievements.EAchievement.Rampage);
			break;
		case 6:
			SteamStatsAndAchievements.Instance.UnlockAchievement(SteamStatsAndAchievements.EAchievement.Dominating);
			break;
		case 7:
			SteamStatsAndAchievements.Instance.UnlockAchievement(SteamStatsAndAchievements.EAchievement.Unstoppable);
			break;
		case 8:
			SteamStatsAndAchievements.Instance.UnlockAchievement(SteamStatsAndAchievements.EAchievement.Genocide);
			break;
		case 9:
			SteamStatsAndAchievements.Instance.UnlockAchievement(SteamStatsAndAchievements.EAchievement.Godlike);
			break;
		case 10:
			SteamStatsAndAchievements.Instance.UnlockAchievement(SteamStatsAndAchievements.EAchievement.WickedStick);
			break;
		}
	}

	public void TakeLocalControl(CharacterActions actions)
	{
		mHasControl = true;
		GetComponent<Standing>().TakeLocalControl();
		mPlayerActions = actions;
	}

	public void SetCollision(bool enabled)
	{
		if (inactive)
		{
			return;
		}
		Transform[] componentsInChildren = base.gameObject.GetComponentsInChildren<Transform>(true);
		foreach (Transform transform in componentsInChildren)
		{
			if ((bool)transform.GetComponent<Walkable>() || transform.gameObject.layer == 20 || transform.gameObject.layer == 29)
			{
				continue;
			}
			if (enabled)
			{
				if (isAI)
				{
					transform.gameObject.layer = 12;
				}
				else
				{
					transform.gameObject.layer = 8 + playerID;
				}
			}
			else
			{
				transform.gameObject.layer = 18;
			}
		}
	}

	private void Update()
	{
		if (inactive || PauseManager.isPaused || info.isDead || (!GameManager.inFight && !GameManager.stillInMenu))
		{
			return;
		}
		if (mHasControl && mPlayerActions.InputType == InputType.Keyboard)
		{
			if (mPlayerActions.PunchOrFireWithKeyBoard.WasPressed)
			{
				allowMouse = false;
			}
			if (Input.GetAxis("Mouse Y") + Input.GetAxis("Mouse X") > 0.1f)
			{
				allowMouse = true;
			}
		}
		if (!mHasControl)
		{
			ApplyMovementState();
			return;
		}
		if (!isAI && !ChatManager.isTyping)
		{
			info.paceState = 0;
			UserAim();
			if ((bool)fighting.weapon && fighting.weapon.isEnergyBased)
			{
				if (mPlayerActions.PunchOrFire.IsPressed)
				{
					ActivateEnergy(true);
				}
				else
				{
					ActivateEnergy(false);
				}
			}
			else
			{
				if (mPlayerActions.PunchOrFire.WasPressed)
				{
					Attack();
				}
				else if (fighting.fullAuto && (bool)mPlayerActions.PunchOrFire)
				{
					Attack();
				}
				if (mPlayerActions.Block.IsPressed)
				{
					StartBlock();
				}
				if (!mPlayerActions.Block.IsPressed)
				{
					EndBlock();
				}
			}
			if (canFly)
			{
				Vector3 flyDirection = new Vector3(0f, mPlayerActions.Movement.Value.y, 0f - mPlayerActions.Movement.Value.x);
				Fly(flyDirection, flySpeed);
			}
			else
			{
				if (mPlayerActions.JumpWasPressed)
				{
					Jump();
				}
				if (mPlayerActions.Movement.X < -0.05f)
				{
					Left();
					m_MovementState |= MovementStateEnum.Left;
				}
				if (mPlayerActions.Movement.X > 0.05f)
				{
					Right();
					m_MovementState |= MovementStateEnum.Right;
				}
			}
			if (mPlayerActions.Throw.WasPressed && !canFly)
			{
				Throw();
			}
		}
		if (info.sinceFallen < 0f)
		{
			EndBlock();
		}
	}

	private void LateUpdate()
	{
		if (mIsNetworkMatch && !MultiplayerManager.IsServer)
		{
			CheckForKick();
		}
	}

	private void CheckForKick()
	{
		if (CheckForInput())
		{
			ResetKickTimer();
		}
		else
		{
			IncrementKickTimer();
		}
	}

	private void IncrementKickTimer()
	{
		m_KickCounter += Time.deltaTime;
		if (m_KickCounter >= 120f)
		{
			KickMySelf();
		}
	}

	private void KickMySelf()
	{
		MatchmakingHandler.Instance.Disconnect(false);
		UnityEngine.Object.FindObjectOfType<LoadingScreenManager>().LoadThenFail(ConnectionErrorType.Kicked, string.Empty);
	}

	private bool CheckForInput()
	{
		if (mPlayerActions != null)
		{
			foreach (PlayerAction action in mPlayerActions.Actions)
			{
				if (action.WasPressed)
				{
					return true;
				}
			}
		}
		return true;
	}

	private void ResetKickTimer()
	{
		m_KickCounter = 0f;
	}

	public void AssignNewDevice(InputDevice inputDevice, bool keyBoard = false)
	{
		mPlayerActions = ((!keyBoard) ? CharacterActions.CreateWithControllerBindings() : CharacterActions.CreateWithKeyBoardMouseBindings());
		mPlayerActions.Device = inputDevice;
	}

	public void Move(float direction)
	{
		rotation.lookingRight = direction < 0f;
		movement.Move(direction);
		animation.Run();
		info.paceState = 1;
	}

	public void Left()
	{
		rotation.lookingRight = false;
		movement.MoveLeft();
		animation.Run();
		info.paceState = 1;
	}

	public void Right()
	{
		rotation.lookingRight = true;
		movement.MoveRight();
		animation.Run();
		info.paceState = 1;
	}

	public void Fly(Vector3 flyDirection, float speed)
	{
		rotation.lookingRight = flyDirection.z < 0f;
		movement.Fly(flyDirection * speed);
		animation.Run();
		info.paceState = 1;
	}

	public void Jump(bool force = false, bool forceWallJump = false)
	{
		if (((info.sinceGrounded < 0.2f || info.sinceWall < 0.2f || grabHandler.isHoldingSomethingAnchored) && info.sinceFallen > 0f && info.sinceJumped > 0.3f) || force)
		{
			if (info.sinceWall > info.sinceGrounded)
			{
				info.wallNormal = Vector3.zero;
			}
			if (grabHandler.isHoldingSomething)
			{
				grabHandler.EndGrab();
			}
			info.sinceGrounded = 1f;
			info.sinceWall = 1f;
			info.sinceJumped = 0f;
			bool flag = movement.Jump(force, forceWallJump);
			m_MovementState = ((!flag) ? MovementStateEnum.GroundJump : MovementStateEnum.WallJump);
			jumpPart.Play();
			jumpPart.transform.position = torso.position;
		}
	}

	public void Down()
	{
	}

	public void Attack()
	{
		fighting.Attack();
	}

	public void StartBlock()
	{
		if (!fighting.weapon && info.sinceFallen > 0f)
		{
			blockHandler.StartBlock();
		}
	}

	public void EndBlock()
	{
		if ((bool)blockHandler)
		{
			blockHandler.EndBlock();
		}
	}

	public void Throw()
	{
		fighting.ThrowWeapon(false);
	}

	public void ActivateEnergy(bool active)
	{
		fighting.ActivateWeapon(active);
	}

	public void UserAim()
	{
		if (mPlayerActions.Aiming.Value.magnitude > 0.1f || mPlayerActions.InputType == InputType.Keyboard)
		{
			if (!fighting.weapon || fighting.weapon.gradualRotationSpeed > 10000f)
			{
				Vector3 vector = new Vector3(0f, mPlayerActions.Aiming.Y, 0f - mPlayerActions.Aiming.X);
				if (vector != Vector3.zero)
				{
					aimer.transform.rotation = Quaternion.LookRotation(vector);
				}
				if (mPlayerActions.InputType == InputType.Keyboard)
				{
					RotateTowardsMouse(aimer);
				}
			}
			else
			{
				helpAimer.transform.rotation = Quaternion.LookRotation(new Vector3(0f, mPlayerActions.Aiming.Y, 0f - mPlayerActions.Aiming.X));
				if (mPlayerActions.InputType == InputType.Keyboard)
				{
					RotateTowardsMouse(helpAimer);
				}
				if (aimer.InverseTransformPoint(helpAimer.GetChild(0).position).y > 0f)
				{
					aimer.transform.Rotate(Vector3.right * (0f - fighting.weapon.gradualRotationSpeed) * Time.deltaTime);
				}
				else
				{
					aimer.transform.Rotate(Vector3.right * fighting.weapon.gradualRotationSpeed * Time.deltaTime);
				}
			}
		}
		else if (mPlayerActions.Movement.Value.magnitude > 0.1f || mPlayerActions.InputType == InputType.Keyboard)
		{
			if (!fighting.weapon || fighting.weapon.gradualRotationSpeed > 10000f)
			{
				aimer.transform.rotation = Quaternion.LookRotation(new Vector3(0f, mPlayerActions.Movement.Y, 0f - mPlayerActions.Movement.X));
				if (mPlayerActions.InputType == InputType.Keyboard)
				{
					RotateTowardsMouse(aimer);
				}
			}
			else
			{
				helpAimer.transform.rotation = Quaternion.LookRotation(new Vector3(0f, mPlayerActions.Movement.Y, 0f - mPlayerActions.Movement.X));
				if (mPlayerActions.InputType == InputType.Keyboard)
				{
					RotateTowardsMouse(helpAimer);
				}
				if (aimer.InverseTransformPoint(helpAimer.GetChild(0).position).y > 0f)
				{
					aimer.transform.Rotate(Vector3.right * (0f - fighting.weapon.gradualRotationSpeed) * Time.deltaTime);
				}
				else
				{
					aimer.transform.Rotate(Vector3.right * fighting.weapon.gradualRotationSpeed * Time.deltaTime);
				}
			}
		}
		float y = aimer.transform.forward.y;
		float z = aimer.transform.forward.z;
		aimer.transform.rotation = Quaternion.LookRotation(new Vector3(0f, y, z));
		mLookRotation = new Vector2(y, z);
	}

	private void RotateTowardsMouse(Transform trans)
	{
		if (allowMouse)
		{
			Vector3 mousePosition = Input.mousePosition;
			mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
			mousePosition.x = 0f;
			trans.transform.rotation = Quaternion.LookRotation(mousePosition - trans.position);
		}
	}

	public void SetNewMovementType(byte movementType)
	{
		m_MovementState = (MovementStateEnum)movementType;
	}

	public void SetNewLookRotation(Vector2 rot)
	{
		aimer.transform.rotation = Quaternion.LookRotation(new Vector3(0f, rot.x, rot.y));
	}

	private void ApplyMovementState()
	{
		bool flag = HasFlag(m_MovementState, MovementStateEnum.WallJump);
		bool flag2 = HasFlag(m_MovementState, MovementStateEnum.GroundJump);
		bool flag3 = HasFlag(m_MovementState, MovementStateEnum.Left);
		bool flag4 = HasFlag(m_MovementState, MovementStateEnum.Right);
		if (flag)
		{
			Jump(true, true);
		}
		if (flag2)
		{
			Jump(true);
		}
		if (flag3)
		{
			Left();
		}
		if (flag4)
		{
			Right();
		}
	}

	private static bool HasFlag(MovementStateEnum a, MovementStateEnum b)
	{
		return (a & b) == b;
	}

	public void OnKilledEnemy(Controller enemyKilled)
	{
		if (enemyKilled.isAI)
		{
			return;
		}
		float num = Time.timeSinceLevelLoad - TransientMemory.Copy<float>("LastKillTimeStamp").GetValue(0f);
		if (HasControl)
		{
			if (num > 2f)
			{
				TransientMemory.Take<int>("MultiKill");
			}
			TransientMemory.Put("MultiKill", TransientMemory.Copy<int>("MultiKill").GetValue(0) + 1);
			TransientMemory.Put("RoundKills", TransientMemory.Copy<int>("RoundKills").GetValue(0) + 1);
			PersistentMemory.Put("KillingSpree", PersistentMemory.Copy<int>("KillingSpree").GetValue(0) + 1);
			SteamStatsAndAchievements.PersistentMemory.Put("STAT_Kills", SteamStatsAndAchievements.PersistentMemory.Copy<int>("STAT_Kills").GetValue(0) + 1);
		}
		TransientMemory.Put("LastKillTimeStamp", Time.timeSinceLevelLoad);
	}

	public void OnDeath()
	{
		if (HasControl)
		{
			PersistentMemory.Take<int>("KillingSpree");
		}
	}

	public void OnUnloadMap()
	{
		if (HasControl)
		{
			TransientMemory.Log();
			TransientMemory.ClearMemory();
		}
	}

	public void OnTakeDamage(float damageTaken)
	{
		if (HasControl)
		{
			TransientMemory.Put("DamageTaken", TransientMemory.Copy<float>("DamageTaken").GetValue(0f) + damageTaken);
			TransientMemory.Put("DamageTakenTimeStamp", Time.timeSinceLevelLoad);
		}
	}

	public void OnFallOut()
	{
		int value = SteamStatsAndAchievements.Instance.TransientMemory.Copy<int>("DeathsByFalling").GetValue(0);
		SteamStatsAndAchievements.Instance.TransientMemory.Put("DeathsByFalling", ++value);
		if (!GameManager.inFight && GameManager.Instance.LastWinner != null && value >= ControllerHandler.Instance.ActivePlayers.Count - 1 && GameManager.Instance.LastWinner.HasControl)
		{
			SteamStatsAndAchievements.Instance.UnlockAchievement(SteamStatsAndAchievements.EAchievement.Walkover);
		}
	}
}
