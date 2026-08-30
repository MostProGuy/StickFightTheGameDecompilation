using UnityEngine;

public class SoundPan : MonoBehaviour
{
	private AudioSource au;

	private void Start()
	{
		au = GetComponent<AudioSource>();
	}

	private void Update()
	{
		au.panStereo = base.transform.localPosition.z / -20f;
	}
}
