using UnityEngine;

public class IceDestructionAudio : MonoBehaviour
{
	private float counter;

	private AudioSource audioSource;

	public AudioClip[] clips;

	private void Awake()
	{
		audioSource = GetComponent<AudioSource>();
		if (!audioSource)
		{
			audioSource = base.gameObject.AddComponent<AudioSource>();
			audioSource.playOnAwake = false;
		}
	}

	private void Update()
	{
		counter += Time.deltaTime;
	}

	public void PlayDestruction()
	{
		if (!(counter < 0.5f) && (bool)audioSource && clips.Length >= 1)
		{
			audioSource.PlayOneShot(clips[Random.Range(0, clips.Length)]);
			counter = 0f;
		}
	}
}
