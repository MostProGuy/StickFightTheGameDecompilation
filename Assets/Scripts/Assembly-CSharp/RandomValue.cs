using UnityEngine;

public class RandomValue : MonoBehaviour
{
	[HideInInspector]
	public float value;

	public float min = 0.8f;

	public float max = 1.2f;

	private void Awake()
	{
		value = Random.Range(min, max);
	}

	private void Update()
	{
	}
}
