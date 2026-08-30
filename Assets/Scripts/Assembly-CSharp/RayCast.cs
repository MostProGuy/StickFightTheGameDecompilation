using UnityEngine;

public class RayCast : MonoBehaviour
{
	public float distanceToHit;

	public LayerMask mask;

	public bool hitSomething;

	private void Start()
	{
	}

	private void LateUpdate()
	{
		RaycastHit hitInfo;
		Physics.Raycast(base.transform.position, base.transform.forward, out hitInfo, 50f, mask);
		if ((bool)hitInfo.transform)
		{
			distanceToHit = Vector3.Distance(base.transform.position, hitInfo.point);
			hitSomething = true;
		}
		else
		{
			hitSomething = false;
			distanceToHit = 50f;
		}
	}
}
