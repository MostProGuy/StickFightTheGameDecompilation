using UnityEngine;

public class SetScaleByRayCast : MonoBehaviour
{
	private RayCast rayCast;

	private void Start()
	{
		rayCast = GetComponent<RayCast>();
	}

	private void LateUpdate()
	{
		base.transform.localScale = new Vector3(base.transform.localScale.x, base.transform.localScale.y, rayCast.distanceToHit);
	}
}
