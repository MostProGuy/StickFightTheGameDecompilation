using UnityEngine;

public class SetWorldPosition : MonoBehaviour
{
	public Vector3 worldPosition;

	private void Awake()
	{
		base.transform.position = worldPosition;
	}

	private void LateUpdate()
	{
		base.transform.position = worldPosition;
	}
}
