using UnityEngine;

public class PlayRandomClip : MonoBehaviour
{
	public AudioClip[] clips;

	private AudioSource au;

	private void Start()
	{
		au = GetComponent<AudioSource>();
		au.PlayOneShot(clips[Random.Range(0, clips.Length)]);
	}

	private void Update()
	{
	}
}
