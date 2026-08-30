using UnityEngine;

public class PlayParticles : MonoBehaviour
{
	private ParticleSystem[] parts;

	private void Start()
	{
		parts = GetComponentsInChildren<ParticleSystem>();
	}

	private void Update()
	{
	}

	public void MoveAndGo(Vector3 pos)
	{
		base.transform.position = pos;
		GO();
	}

	public void GO()
	{
		ParticleSystem[] array = parts;
		foreach (ParticleSystem particleSystem in array)
		{
			particleSystem.Play();
		}
	}
}
