using UnityEngine;

public class LaserAudioHandler : MonoBehaviour
{
	private RayCast rayCast;

	private AudioSource audioSource;

	public AudioSource hitSource;

	private void Start()
	{
		audioSource = GetComponent<AudioSource>();
		rayCast = GetComponentInChildren<RayCast>();
	}

	private void Update()
	{
		if ((bool)rayCast)
		{
			float num = Mathf.Clamp(rayCast.distanceToHit, 0f, 20f) * 0.01f;
			audioSource.pitch = 1.2f - num;
			if (rayCast.distanceToHit < 25f && rayCast.hitSomething)
			{
				hitSource.volume = Mathf.Lerp(hitSource.volume, 0.1f, Time.deltaTime * 10f);
				hitSource.pitch = 0.3f - num;
			}
			else
			{
				hitSource.volume = Mathf.Lerp(hitSource.volume, 0f, Time.deltaTime * 10f);
				hitSource.pitch = 0.3f - num;
			}
		}
	}
}
