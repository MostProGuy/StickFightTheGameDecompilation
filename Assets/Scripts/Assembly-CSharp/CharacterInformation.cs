using UnityEngine;

public class CharacterInformation : MonoBehaviour
{
	public Material myMaterial;

	public Rigidbody mainRig;

	public bool isGrounded;

	public bool isDead;

	public float sinceGrounded;

	public float sinceWall = 1f;

	public float sinceJumped;

	public float shortestDistanceFromHeadToGround;

	public float sinceFallen = 2f;

	public bool leftSideForward;

	public bool isBeingGrabbed;

	public float sinceGrabbed;

	public float sinceThrown = 1f;

	public int paceState;

	public bool alwaysStand;

	public Vector3 wallNormal;

	private CheckForGroundCollision[] groundCheckers;

	[HideInInspector]
	public Transform headTransform;

	private Transform landParts;

	private ScreenshakeHandler screenshake;

	private Vector3 landPosition;

	public AudioClip[] landClips;

	private AudioSource au;

	private void Start()
	{
		Rigidbody[] componentsInChildren = base.transform.GetChild(0).GetComponentsInChildren<Rigidbody>();
		foreach (Rigidbody rigidbody in componentsInChildren)
		{
			rigidbody.gameObject.AddComponent<CheckForGroundCollision>();
		}
		groundCheckers = GetComponentsInChildren<CheckForGroundCollision>();
		headTransform = GetComponentInChildren<Head>().transform;
		screenshake = ScreenshakeHandler.Instance;
		au = GetComponentInChildren<AudioSource>();
		LandParticle componentInChildren = GetComponentInChildren<LandParticle>();
		if (componentInChildren != null)
		{
			landParts = componentInChildren.transform;
		}
	}

	private void FixedUpdate()
	{
		if (isDead)
		{
			sinceFallen = -10f;
		}
		CheckIfGrounded();
		sinceFallen += Time.deltaTime;
		sinceJumped += Time.deltaTime;
		sinceWall += Time.deltaTime;
		if (isBeingGrabbed)
		{
			sinceGrabbed = 0f;
		}
		else
		{
			sinceGrabbed += Time.deltaTime;
		}
		sinceGrabbed += Time.deltaTime;
		sinceThrown += Time.deltaTime;
		if (!isGrounded)
		{
			sinceGrounded += Time.deltaTime;
			return;
		}
		if (sinceGrounded > 0.4f)
		{
			ParticleSystem[] componentsInChildren = landParts.GetComponentsInChildren<ParticleSystem>();
			foreach (ParticleSystem particleSystem in componentsInChildren)
			{
				particleSystem.transform.position = landPosition;
				particleSystem.Play();
			}
			if (landClips.Length > 0)
			{
				au.PlayOneShot(landClips[Random.Range(0, landClips.Length)], 0.4f);
			}
			if ((bool)screenshake)
			{
				screenshake.AddShake(Vector3.down * 0.01f);
			}
		}
		sinceGrounded = 0f;
	}

	private void Fall(float time)
	{
		sinceFallen = 0f - time;
	}

	private void CheckIfGrounded()
	{
		int num = groundCheckers.Length;
		isGrounded = false;
		for (int i = 0; i < num; i++)
		{
			if (groundCheckers[i].isGrounded)
			{
				float num2 = Vector3.Distance(headTransform.position, groundCheckers[i].collisionPosition);
				if (num2 < shortestDistanceFromHeadToGround || !isGrounded)
				{
					shortestDistanceFromHeadToGround = num2;
					landPosition = groundCheckers[i].collisionPosition;
				}
				if (sinceJumped > 0.2f)
				{
					isGrounded = true;
				}
			}
		}
		if (alwaysStand)
		{
			isGrounded = true;
		}
	}
}
