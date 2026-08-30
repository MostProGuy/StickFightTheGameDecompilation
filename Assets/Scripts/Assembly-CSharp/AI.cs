using UnityEngine;

public class AI : MonoBehaviour
{
	private Controller controller;

	public Rigidbody target;

	public Transform behaviourTarget;

	private CharacterInformation targetInformation;

	private CharacterInformation info;

	private Transform head;

	private Movement movement;

	private float reactionCounter;

	public float reactionTime = 0.4f;

	public float reactionHitReset = -0.5f;

	public float range = 1f;

	public bool canAttack = true;

	public float heightRange = float.PositiveInfinity;

	public float preferredRange;

	private Transform aimer;

	private Fighting fighting;

	public float jumpOffset = 1f;

	private float counter;

	public float targetingSmoothing;

	public float velocitySmoothnes = 2f;

	private float velocity;

	private ControllerHandler controllerHandler;

	public bool goForGuns = true;

	public bool attacking;

	public bool dontAimWhenAttacking;

	public float startAttackDelay;

	private void Start()
	{
		controllerHandler = ControllerHandler.Instance;
		controller = GetComponent<Controller>();
		info = GetComponent<CharacterInformation>();
		movement = GetComponent<Movement>();
		fighting = GetComponent<Fighting>();
		head = GetComponentInChildren<Head>().transform;
		SetStats();
		aimer = GetComponentInChildren<AimTarget>().transform.parent;
	}

	private void SetStats()
	{
		movement.forceMultiplier *= Random.Range(0.5f, 1f);
	}

	private void Update()
	{
		startAttackDelay -= Time.deltaTime;
		Vector3 vector = Vector3.zero;
		if ((bool)behaviourTarget)
		{
			vector = behaviourTarget.position;
		}
		else if ((bool)target)
		{
			vector = target.position;
		}
		if (counter > 1f)
		{
			counter = Random.Range(-0.5f, 0.5f);
			target = null;
		}
		if (vector != Vector3.zero && (!targetInformation || !targetInformation.isDead))
		{
			info.paceState = 0;
			if (!dontAimWhenAttacking || !fighting.isSwinging)
			{
				if (targetingSmoothing == 0f)
				{
					aimer.rotation = Quaternion.LookRotation(vector - head.position);
				}
				else
				{
					aimer.rotation = Quaternion.Lerp(aimer.rotation, Quaternion.LookRotation(vector - head.position), Time.deltaTime * (5f / targetingSmoothing));
				}
			}
			counter += Time.deltaTime;
			if (Vector3.Distance(head.position, vector) > preferredRange)
			{
				if (vector.z < head.position.z)
				{
					if (velocitySmoothnes == 0f)
					{
						velocity = -1f;
					}
					else
					{
						velocity = Mathf.Lerp(velocity, -1f, Time.deltaTime * (5f / velocitySmoothnes));
					}
				}
				if (vector.z > head.position.z)
				{
					if (velocitySmoothnes == 0f)
					{
						velocity = 1f;
					}
					else
					{
						velocity = Mathf.Lerp(velocity, 1f, Time.deltaTime * (5f / velocitySmoothnes));
					}
				}
				controller.Move(velocity);
			}
			if (vector.y > head.position.y + jumpOffset)
			{
				controller.Jump();
			}
			attacking = false;
			if ((bool)behaviourTarget || !canAttack || !(startAttackDelay < 0f))
			{
				return;
			}
			attacking = true;
			float num = range;
			reactionTime = 0.4f;
			if ((bool)fighting.weapon)
			{
				if (fighting.weapon.isGun)
				{
					num = 25f;
					reactionTime = 0.25f;
				}
				else
				{
					num = 2f;
					reactionTime = 0.25f;
				}
			}
			if (Vector3.Distance(head.position, vector) < num && vector.y - head.position.y < heightRange)
			{
				reactionCounter += Time.deltaTime;
				if (reactionCounter > reactionTime)
				{
					reactionCounter = 0f;
					controller.Attack();
				}
			}
			else if (reactionCounter > 0f)
			{
				reactionCounter -= Time.deltaTime;
			}
		}
		else
		{
			if ((bool)behaviourTarget)
			{
				return;
			}
			float num2 = 100f;
			WeaponPickUp weaponPickUp = null;
			if (goForGuns)
			{
				weaponPickUp = Object.FindObjectOfType<WeaponPickUp>();
			}
			if (!weaponPickUp || !(weaponPickUp.transform.position.y < 10f))
			{
				foreach (Controller player in controllerHandler.players)
				{
					if (!(player == null))
					{
						CharacterInformation component = player.GetComponent<CharacterInformation>();
						if (!component.isDead)
						{
							Transform transform = player.GetComponentInChildren<Torso>().transform;
							float num3 = Vector3.Distance(head.position, transform.position);
							if (num3 < num2)
							{
								num2 = num3;
								target = transform.GetComponent<Rigidbody>();
								targetInformation = component;
							}
						}
					}
				}
				return;
			}
			target = weaponPickUp.GetComponent<Rigidbody>();
		}
	}
}
