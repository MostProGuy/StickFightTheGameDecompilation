using UnityEngine;

public class IgnorePlayerWhenOffScreen : MonoBehaviour
{
	private int layer;

	private void Start()
	{
		layer = base.gameObject.layer;
	}

	private void Update()
	{
		if (base.transform.position.y < -11f)
		{
			base.gameObject.layer = 24;
		}
		else
		{
			base.gameObject.layer = layer;
		}
	}
}
