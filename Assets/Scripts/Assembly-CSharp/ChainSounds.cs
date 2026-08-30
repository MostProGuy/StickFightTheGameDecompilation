using UnityEngine;

public class ChainSounds : MonoBehaviour
{
	private AudioSource au;

	private Rigidbody rig;

	public float multiplier = 1f;

	public float lerpSpeed;

	private void Start()
	{
		au = GetComponent<AudioSource>();
		rig = GetComponent<Rigidbody>();
		au.volume = 0f;
	}

	private void Update()
	{
		if (rig != null && au != null)
		{
			if (lerpSpeed == 0f)
			{
				au.volume = Mathf.Pow(rig.velocity.magnitude, 1.5f) * 0.01f * multiplier;
			}
			else
			{
				au.volume = Mathf.Lerp(au.volume, Mathf.Pow(rig.velocity.magnitude, 1.5f) * 0.01f * multiplier, Time.deltaTime * lerpSpeed);
			}
		}
	}
}
