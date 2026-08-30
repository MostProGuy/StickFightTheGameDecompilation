using UnityEngine;

public class FollowRotation : MonoBehaviour
{
	public Transform target;

	private void Start()
	{
	}

	private void Update()
	{
		base.transform.rotation = target.rotation;
	}
}
