using UnityEngine;

public class ShakeOnImpact : MonoBehaviour
{
	public bool ignoreRigidbodies = true;

	public float amount;

	private ScreenshakeHandler screenshake;

	public float threshold;

	public AudioClip[] impactClips;

	public AudioSource au;

	public bool scaleWithImpact;

	private float counter;

	private void Start()
	{
		screenshake = ScreenshakeHandler.Instance;
		if (!au)
		{
			au = GetComponent<AudioSource>();
		}
	}

	private void Update()
	{
		counter += Time.deltaTime;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (base.transform.position.y < -15f || (ignoreRigidbodies && (bool)collision.rigidbody) || collision.relativeVelocity.magnitude < threshold || counter < 0.5f)
		{
			return;
		}
		counter = 0f;
		float num = 1f;
		if ((bool)collision.transform.root.GetComponent<Controller>())
		{
			num *= 0.1f;
		}
		if ((bool)au && impactClips.Length > 0)
		{
			if (scaleWithImpact)
			{
				num = Mathf.Clamp((collision.relativeVelocity.magnitude - threshold) / threshold, 0f, 1f);
			}
			au.PlayOneShot(impactClips[Random.Range(0, impactClips.Length)], num);
		}
		screenshake.AddShake(-collision.relativeVelocity.normalized * num * amount);
	}
}
