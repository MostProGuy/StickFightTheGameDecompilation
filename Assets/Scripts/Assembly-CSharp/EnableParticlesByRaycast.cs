using UnityEngine;

public class EnableParticlesByRaycast : MonoBehaviour
{
	public RayCast rayCaster;

	private ParticleSystem[] parts;

	private bool isOn;

	private void Start()
	{
		parts = GetComponentsInChildren<ParticleSystem>();
	}

	private void Update()
	{
		if (rayCaster.hitSomething && rayCaster.gameObject.active)
		{
			if (!isOn)
			{
				ParticleSystem[] array = parts;
				foreach (ParticleSystem particleSystem in array)
				{
					particleSystem.enableEmission = true;
				}
				isOn = true;
			}
		}
		else if (isOn)
		{
			ParticleSystem[] array2 = parts;
			foreach (ParticleSystem particleSystem2 in array2)
			{
				particleSystem2.enableEmission = false;
			}
			isOn = false;
		}
	}
}
