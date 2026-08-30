using UnityEngine;

public class BlockHandler : MonoBehaviour
{
	public struct BlockData
	{
		public Controller blocker;

		public float blockTimeStamp;

		public BlockData(Controller blocker, float blockTimeStamp)
		{
			this.blocker = blocker;
			this.blockTimeStamp = blockTimeStamp;
		}
	}

	public float sinceBlockStart = 1f;

	private float blockcdCounter;

	private float blockCd = 0.2f;

	public bool isBlocking;

	private ScreenshakeHandler screenShake;

	private Transform target;

	private AudioSource au;

	public AudioClip[] clips;

	public AudioClip[] blockConnectClips;

	private Fighting fighting;

	private BlockAnimation anim;

	public float blockForce;

	public Vector3 blockAngle;

	private ParticleSystem blockPart;

	private ParticleSystem blockPart2;

	private float c;

	private void Start()
	{
		screenShake = ScreenshakeHandler.Instance;
		target = GetComponentInChildren<AimTarget>().transform;
		au = GetComponentInChildren<AudioSource>();
		fighting = GetComponent<Fighting>();
		anim = GetComponentInChildren<BlockAnimation>();
		blockPart = GetComponentInChildren<BlockParticle>().GetComponent<ParticleSystem>();
		blockPart2 = blockPart.transform.GetChild(0).GetComponent<ParticleSystem>();
	}

	private void Update()
	{
		c += Time.deltaTime;
		sinceBlockStart += Time.deltaTime;
		blockAngle = target.forward;
		if (!isBlocking)
		{
			blockCd += Time.deltaTime;
			anim.isBlocking = false;
		}
		else
		{
			blockCd = 0f;
			anim.isBlocking = true;
		}
		blockForce = anim.blockPower;
	}

	public void StartBlock()
	{
		if (!isBlocking && !(fighting.counter < 0f) && !(c < 0.3f))
		{
			c = 0f;
			sinceBlockStart = 0f;
			isBlocking = true;
			screenShake.AddShake(target.forward * 0.02f);
			au.PlayOneShot(clips[Random.Range(0, clips.Length)]);
		}
	}

	public void EndBlock()
	{
		isBlocking = false;
	}

	public void DoBlock()
	{
		blockPart2.Play();
		blockPart.Play();
		au.PlayOneShot(blockConnectClips[Random.Range(0, blockConnectClips.Length)]);
		Controller component = fighting.GetComponent<Controller>();
		if (component != null && component.HasControl)
		{
			MemoryBucket memoryBucket = component.GetMemoryBucket(Controller.EMemoryBucketType.Transient);
			memoryBucket.Put("BlockData", new BlockData(component, Time.timeSinceLevelLoad));
		}
	}
}
