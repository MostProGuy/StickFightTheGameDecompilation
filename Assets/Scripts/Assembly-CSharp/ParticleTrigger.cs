using UnityEngine;

public class ParticleTrigger : MonoBehaviour
{
	public ParticleSystem part;

	public float multiplier;

	public int cap = 5;

	public AudioClip[] clips;

	private AudioSource au;

	private float counter;

	private void Start()
	{
		au = GetComponent<AudioSource>();
	}

	private void Update()
	{
		counter += Time.deltaTime;
	}

	private void OnTriggerStay(Collider other)
	{
		Rigidbody component = other.GetComponent<Rigidbody>();
		if (!component)
		{
			return;
		}
		if (counter > 0.5f && component.velocity.magnitude > 10f)
		{
			au.pitch = Random.Range(0.95f, 1.05f);
			if (clips.Length > 0)
			{
				au.PlayOneShot(clips[Random.Range(0, clips.Length)], 0.2f * Random.Range(0.95f, 1.05f));
			}
			counter = 0f;
		}
		if (component.velocity.sqrMagnitude > 0f)
		{
			part.transform.rotation = Quaternion.LookRotation(component.velocity);
		}
		ParticleSystem.MainModule main = part.main;
		main.startSpeedMultiplier = 1f + Mathf.Clamp(component.velocity.magnitude * multiplier, 0f, cap) * 10f;
		part.Emit((int)Mathf.Clamp(component.velocity.magnitude * multiplier, 0f, cap));
	}
}
