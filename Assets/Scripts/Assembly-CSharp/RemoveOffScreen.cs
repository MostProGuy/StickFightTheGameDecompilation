using UnityEngine;

public class RemoveOffScreen : MonoBehaviour
{
	private float counter;

	private void Start()
	{
	}

	private void Update()
	{
		counter += Time.deltaTime;
		if (!(counter < 3f) && (Mathf.Abs(base.transform.position.z) > 19f || base.transform.position.y < -11.5f))
		{
			Object.Destroy(base.gameObject);
		}
	}
}
