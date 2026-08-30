using UnityEngine;

public class SetPositionByRayCast : MonoBehaviour
{
	private RayCast rayCast;

	public Transform target;

	private void Start()
	{
		rayCast = GetComponent<RayCast>();
	}

	private void LateUpdate()
	{
		if ((bool)target)
		{
			target.position = rayCast.transform.position + rayCast.transform.forward * rayCast.distanceToHit;
		}
	}
}
