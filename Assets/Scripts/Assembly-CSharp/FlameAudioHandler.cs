using UnityEngine;

public class FlameAudioHandler : MonoBehaviour
{
	public static float fireTime = -1f;

	private AudioSource au;

	private void Start()
	{
		fireTime = -1f;
		au = GetComponent<AudioSource>();
	}

	private void Update()
	{
		fireTime -= Time.deltaTime;
		if (fireTime > 0f)
		{
			au.volume = Mathf.Lerp(au.volume, 1f, Time.deltaTime * 30f);
		}
		else
		{
			au.volume = Mathf.Lerp(au.volume, 0f, Time.deltaTime * 30f);
		}
	}

	public static void AddFire(float f)
	{
		fireTime = f;
	}
}
