using UnityEngine;

public class ShrinkOverTime : MonoBehaviour
{
	public float secondsToShrink = 1f;

	private Vector3 localScale;

	private float counter = 1f;

	private void Start()
	{
		localScale = base.transform.localScale;
	}

	private void Update()
	{
		counter -= Time.deltaTime / secondsToShrink;
		base.transform.localScale = localScale * counter;
		if (counter < 0f)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
