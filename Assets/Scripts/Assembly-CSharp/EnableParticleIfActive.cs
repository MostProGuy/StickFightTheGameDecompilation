using UnityEngine;

public class EnableParticleIfActive : MonoBehaviour
{
	public ParticleSystem[] parts;

	private Weapon weapon;

	private void Start()
	{
		weapon = GetComponentInParent<Weapon>();
	}

	private void Update()
	{
		ParticleSystem[] array = parts;
		foreach (ParticleSystem particleSystem in array)
		{
			if (weapon.isActive)
			{
				particleSystem.enableEmission = true;
			}
			else
			{
				particleSystem.enableEmission = false;
			}
		}
	}

	private void OnDisable()
	{
		ParticleSystem[] array = parts;
		foreach (ParticleSystem particleSystem in array)
		{
			particleSystem.enableEmission = false;
		}
	}
}
