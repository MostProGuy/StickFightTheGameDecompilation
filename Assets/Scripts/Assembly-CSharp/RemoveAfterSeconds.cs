using UnityEngine;

public class RemoveAfterSeconds : MonoBehaviour
{
	public float time = 5f;

	private void Start()
	{
	}

	private void Update()
	{
		time -= Time.deltaTime;
		if (time < 0f)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
