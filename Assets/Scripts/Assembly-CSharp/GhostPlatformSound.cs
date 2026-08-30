using UnityEngine;

public class GhostPlatformSound : MonoBehaviour
{
	private static GhostPlatformSound instance;

	private AudioSource au;

	private float counter;

	public static GhostPlatformSound Instance
	{
		get
		{
			return instance;
		}
	}

	private void Awake()
	{
		instance = this;
		au = GetComponent<AudioSource>();
	}

	private void Update()
	{
		counter += Time.deltaTime;
	}

	public void PlaySound()
	{
		if (!(counter < 0.1f))
		{
			counter = 0f;
			au.PlayOneShot(au.clip);
		}
	}
}
