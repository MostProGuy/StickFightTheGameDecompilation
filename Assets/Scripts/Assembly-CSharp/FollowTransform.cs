using UnityEngine;

public class FollowTransform : MonoBehaviour
{
	public Transform target;

	public bool local;

	public Vector2 cap = Vector2.zero;

	public Vector2 capTop = Vector2.zero;

	public float multiplier = 1f;

	private void Start()
	{
	}

	private void LateUpdate()
	{
		if (local)
		{
			base.transform.localPosition = target.position * multiplier;
		}
		else
		{
			base.transform.position = target.position * multiplier;
		}
		if (cap != Vector2.zero)
		{
			base.transform.position = new Vector3(base.transform.position.x, Mathf.Clamp(base.transform.position.y, cap.y, capTop.y), Mathf.Clamp(base.transform.position.z, cap.x, capTop.x));
		}
	}
}
