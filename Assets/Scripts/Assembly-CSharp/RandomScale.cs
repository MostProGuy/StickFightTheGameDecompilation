using UnityEngine;

public class RandomScale : MonoBehaviour
{
	public float min;

	public float max = 1f;

	public AnimationCurve curve;

	private void Awake()
	{
		base.transform.localScale *= curve.Evaluate(Random.Range(min, max));
	}

	private void Update()
	{
	}
}
