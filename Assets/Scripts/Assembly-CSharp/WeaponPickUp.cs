using UnityEngine;

public class WeaponPickUp : MonoBehaviour
{
	public int id;

	public float upForce;

	public float flyUpAfter = 7f;

	private Rigidbody rig;

	public float counter;

	private bool activeColliders;

	private bool isThrown;

	private bool canDealDamage;

	public bool wasDroppped;

	private Controller damager;

	public float sinceDropCounter;

	public float cantBePickledUpFor = 1f;

	private ScreenshakeHandler screenshake;

	private float sinceThrow;

	public int sendTheMovementAbility = -1;

	public bool unEnding;

	private Vector3 startPos = Vector3.zero;

	public AudioClip specialMusic;

	public AudioClip soundEffect;

	private static GameObject presentPref;

	private Vector2 m_StartPos;

	private MultiplayerManager mNetworkManager;

	private ushort mNetworkSpawnIndex;

	private float mCurrentDeadTime;

	public bool IsGroundWeapon
	{
		get
		{
			return flyUpAfter >= float.PositiveInfinity;
		}
	}

	public Vector2 StartPos
	{
		get
		{
			return m_StartPos;
		}
	}

	public ushort NetworkSpawnIndex
	{
		get
		{
			return mNetworkSpawnIndex;
		}
	}

	public float CurrentDeadTime
	{
		get
		{
			return mCurrentDeadTime;
		}
	}

	private void Awake()
	{
		startPos = base.transform.localPosition;
		screenshake = ScreenshakeHandler.Instance;
		Vector3 position = base.transform.position;
		Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
		foreach (Collider collider in componentsInChildren)
		{
		}
		if (MatchmakingHandler.IsNetworkMatch && IsGroundWeapon)
		{
			Debug.Log("Weapon Is On The Ground!");
			mNetworkManager = Object.FindObjectOfType<MultiplayerManager>();
			Vector2 pos = new Vector2(position.y, position.z);
			mNetworkManager.AddPreSpawnedWeapon(pos, this);
		}
		if (unEnding)
		{
			RemoveAfterSeconds component = GetComponent<RemoveAfterSeconds>();
			if ((bool)component)
			{
				component.enabled = false;
			}
		}
	}

	private void Start()
	{
		rig = GetComponent<Rigidbody>();
		InsertIntoBlackHole();
	}

	public void ChangeToPresent()
	{
		if (!presentPref)
		{
			presentPref = (GameObject)Resources.Load("PacketGFX", typeof(GameObject));
		}
		for (int num = base.transform.childCount - 1; num >= 0; num--)
		{
			Object.Destroy(base.transform.GetChild(num).gameObject);
		}
		Object.Destroy(base.transform.GetComponent<ConstantForce>());
		GetComponent<Rigidbody>().constraints = (RigidbodyConstraints)98;
		Object.Instantiate(presentPref, base.transform);
	}

	public void InitGroundWeapon()
	{
		Awake();
	}

	private void InsertIntoBlackHole()
	{
		BlackHole[] array = Object.FindObjectsOfType<BlackHole>();
		BlackHole[] array2 = array;
		foreach (BlackHole blackHole in array2)
		{
			blackHole.AddRunTimeWeapon(base.gameObject.GetComponent<Rigidbody>());
		}
	}

	public void InitNetwork(ushort index)
	{
		mNetworkSpawnIndex = index;
	}

	public void Throw(Controller d)
	{
		isThrown = true;
		canDealDamage = true;
		damager = d;
		sinceThrow += Time.deltaTime;
	}

	private void OnDestroy()
	{
		if (sendTheMovementAbility != -1 && (bool)specialMusic)
		{
			MusicHandler.Instance.PlaySpecialSong(specialMusic, 0, 1);
		}
		if ((bool)soundEffect)
		{
			MusicHandler.Instance.specialAu2.PlayOneShot(soundEffect, 0.6f);
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if ((bool)collision.rigidbody && collision.relativeVelocity.magnitude > 20f && canDealDamage)
		{
			BodyPart component = collision.rigidbody.GetComponent<BodyPart>();
			if ((bool)component && (bool)damager)
			{
				if (component.transform.root.GetComponent<Controller>() == damager && sinceThrow < 0.2f)
				{
					return;
				}
				component.TakeDamageWithParticle(55f, base.transform.position, -collision.relativeVelocity, damager);
				if (component.isGun)
				{
					component.transform.root.GetComponentInChildren<Torso>().GetComponent<BodyPart>().TakeDamageWithParticle(55f, base.transform.position, -collision.relativeVelocity, damager);
				}
				Vector3 force = -collision.relativeVelocity.normalized * 50f;
				Vector3 force2 = -collision.relativeVelocity.normalized * 80f;
				Rigidbody rigidbody = collision.rigidbody;
				Rigidbody component2 = collision.transform.root.GetComponentInChildren<Torso>().GetComponent<Rigidbody>();
				if (MatchmakingHandler.IsNetworkMatch)
				{
					RigidBodyIndexHolder component3 = rigidbody.GetComponent<RigidBodyIndexHolder>();
					if (component3 != null && damager.HasControl)
					{
						byte index = component3.Index;
						byte index2 = component3.Index;
						NetworkPlayer component4 = rigidbody.transform.root.GetComponent<NetworkPlayer>();
						if ((bool)component4)
						{
							component4.SendAddedForce(index, force, ForceMode.VelocityChange);
							component4.SendAddedForce(index2, force2, ForceMode.VelocityChange);
						}
						rigidbody.AddForce(force, ForceMode.VelocityChange);
						component2.AddForce(force2, ForceMode.VelocityChange);
					}
				}
				else
				{
					rigidbody.AddForce(force, ForceMode.VelocityChange);
					component2.AddForce(force2, ForceMode.VelocityChange);
				}
			}
			canDealDamage = false;
			EnableCollidersForThrower();
		}
		if (isThrown)
		{
			EnableCollidersForThrower();
		}
		isThrown = false;
		ScreenshakeHandler.Instance.AddShake(0.1f * -collision.relativeVelocity / 20f);
	}

	private void EnableCollidersForThrower()
	{
		Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
		foreach (Collider collider in componentsInChildren)
		{
			collider.gameObject.layer = 29;
		}
	}

	private void Update()
	{
		if (rig == null)
		{
			return;
		}
		if (unEnding)
		{
			rig.AddForce((startPos - base.transform.localPosition) * Time.deltaTime * 5000f, ForceMode.Acceleration);
		}
		if (!wasDroppped && !canDealDamage && !isThrown)
		{
			cantBePickledUpFor -= Time.deltaTime;
		}
		sinceDropCounter -= Time.deltaTime;
		sinceThrow += Time.deltaTime;
		if (sinceDropCounter < 0f && wasDroppped)
		{
			EnableCollidersForThrower();
			wasDroppped = false;
		}
		if (rig.velocity.magnitude < 10f && canDealDamage)
		{
			EnableCollidersForThrower();
			canDealDamage = false;
		}
		if (counter > 0.3f && !activeColliders)
		{
			Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
			foreach (Collider collider in componentsInChildren)
			{
				collider.enabled = true;
			}
			activeColliders = true;
		}
		counter += Time.deltaTime;
		flyUpAfter -= Time.deltaTime;
		Ray ray = new Ray(base.transform.position, Vector3.down);
		RaycastHit hitInfo;
		if (Physics.Raycast(ray, out hitInfo, 1f) && (bool)hitInfo.transform && hitInfo.transform.gameObject.layer != 20)
		{
			rig.AddForce(Vector3.up * Time.deltaTime * 60f * upForce / Vector3.Distance(base.transform.position, hitInfo.point), ForceMode.Acceleration);
		}
	}

	public void DelayBy(float seconds)
	{
		mCurrentDeadTime = Time.time + seconds;
	}
}
