using UnityEngine;

public class SetRandomEnabledObjects : MonoBehaviour
{
	private float counter = 10f;

	private void Start()
	{
	}

	private void Update()
	{
		counter -= Time.deltaTime;
		if (counter < 0f)
		{
			ChangeMode();
		}
	}

	private void ChangeMode()
	{
		counter = Random.Range(10, 20);
		Transform[] componentsInChildren = GetComponentsInChildren<Transform>(true);
		foreach (Transform transform in componentsInChildren)
		{
			if (!(transform == base.transform) && !(transform.transform.parent != base.transform))
			{
				transform.gameObject.SetActive(false);
			}
		}
		Transform[] componentsInChildren2 = GetComponentsInChildren<Transform>(true);
		foreach (Transform transform2 in componentsInChildren2)
		{
			if (!(transform2 == base.transform) && !(transform2.transform.parent != base.transform) && Random.value > 0.9f)
			{
				transform2.gameObject.SetActive(true);
			}
		}
	}
}
