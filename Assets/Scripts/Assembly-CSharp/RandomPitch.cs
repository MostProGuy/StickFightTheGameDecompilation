using UnityEngine;

public class RandomPitch : MonoBehaviour
{
	public float min = 0.95f;

	public float max = 1.05f;

	private AudioSource au;

	private void Awake()
	{
		au = GetComponent<AudioSource>();
		au.pitch *= Random.Range(min, max);
	}

	private void Update()
	{
	}
}
