using UnityEngine;

public class CheckForGroundCollision : MonoBehaviour
{
	public bool isGrounded;

	private bool JustSwitched;

	public Vector3 collisionPosition;

	private HealthHandler health;

	private Controller controller;

	private GrabHandler grabHandler;

	private CharacterInformation info;

	private ParticleSystem part;

	private ScreenshakeHandler screenShake;

	private bool isVitalPart;

	private void Start()
	{
		health = base.transform.root.GetComponent<HealthHandler>();
		controller = base.transform.root.GetComponent<Controller>();
		info = base.transform.root.GetComponent<CharacterInformation>();
		grabHandler = base.transform.root.GetComponent<GrabHandler>();
		part = base.transform.root.GetComponentInChildren<FallOutPart>().GetComponent<ParticleSystem>();
		screenShake = ScreenshakeHandler.Instance;
		if ((bool)GetComponent<Torso>() || (bool)GetComponent<Head>() || (bool)GetComponent<LeftLeg>() || (bool)GetComponent<RightLeg>())
		{
			isVitalPart = true;
		}
	}

	private void FixedUpdate()
	{
		if (!JustSwitched)
		{
			isGrounded = false;
		}
		else
		{
			JustSwitched = false;
		}
	}

	private void Collide(Collision collision)
	{
		bool flag = !MatchmakingHandler.IsNetworkMatch || controller.HasControl;
		if ((GameManager.inFight || true) && flag)
		{
			if (collision.collider.tag == "Lava")
			{
				health.DealLavaDamage(collision.contacts[0].normal);
			}
			if (collision.collider.tag == "Kill" && !info.isDead)
			{
				bool flag2 = health.health <= 0f;
				if (isVitalPart || collision.collider.gameObject.name == "Death")
				{
					health.TakeDamage(500f, null);
				}
				bool flag3 = health.health <= 0f;
				if (!flag2 && flag3)
				{
					screenShake.AddShake((-base.transform.position).normalized * 1f);
					if (collision.collider.gameObject.name == "Death")
					{
						Quaternion quaternion = Quaternion.LookRotation(collision.contacts[0].normal);
						Vector3 point = collision.contacts[0].point;
						if (MatchmakingHandler.IsNetworkMatch)
						{
							health.GetComponent<NetworkPlayer>().FallOut(quaternion, point);
						}
						part.transform.rotation = quaternion;
						part.transform.position = point;
						part.Play();
						controller.OnFallOut();
						health.FallOut();
						base.transform.GetComponentInParent<CharacterStats>().falls++;
					}
				}
			}
		}
		if (!(collision.transform.root == base.transform.root) && (!collision.transform.root.GetComponent<Controller>() || (bool)collision.transform.GetComponent<Walkable>()) && collision.gameObject.layer != 26 && collision.gameObject.layer != 27 && !(Vector3.Angle(Vector3.up, collision.contacts[0].normal) > 95f))
		{
			if (Vector3.Angle(Vector3.up, collision.contacts[0].normal) > 75f)
			{
				info.sinceWall = 0f;
				info.wallNormal = collision.contacts[0].normal;
			}
			else
			{
				JustSwitched = true;
				isGrounded = true;
				collisionPosition = collision.contacts[0].point;
			}
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		Collide(collision);
	}

	private void OnCollisionStay(Collision collision)
	{
		Collide(collision);
	}
}
