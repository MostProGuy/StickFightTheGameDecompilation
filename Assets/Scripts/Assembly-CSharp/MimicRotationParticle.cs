using UnityEngine;

public class MimicRotationParticle : MonoBehaviour
{
	private ParticleSystem parts;

	private void Start()
	{
		parts = GetComponent<ParticleSystem>();
	}

	private void Update()
	{
		ParticleSystem.MainModule main = parts.main;
		main.startRotationXMultiplier = 1f;
		main.startRotationX = base.transform.rotation.eulerAngles.x;
	}
}
