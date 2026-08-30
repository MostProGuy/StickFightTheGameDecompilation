using LevelEditor;
using UnityEngine;

public class LaserSpecialBehaviour : PropSpecialBehaviourBase
{
	public Transform targetToPosition;

	public LayerMask mask;

	private void Start()
	{
	}

	private void LateUpdate()
	{
		bool flag = false;
		RaycastHit hitInfo;
		Physics.Raycast(base.transform.position, base.transform.forward, out hitInfo, 50f, mask);
		float num;
		if ((bool)hitInfo.transform)
		{
			num = Vector3.Distance(base.transform.position, hitInfo.point);
			flag = true;
		}
		else
		{
			flag = false;
			num = 50f;
		}
		base.transform.localScale = new Vector3(base.transform.localScale.x, base.transform.localScale.y, num);
		if ((bool)targetToPosition)
		{
			targetToPosition.position = base.transform.position + base.transform.forward * num;
		}
	}

	public override void Begin()
	{
	}

	public override void Exit()
	{
	}
}
