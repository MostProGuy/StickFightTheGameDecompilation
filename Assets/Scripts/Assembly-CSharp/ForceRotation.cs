using UnityEngine;

public class ForceRotation : MonoBehaviour
{
	private float rotationVelocity;

	public float turnSpeed;

	public float friction = 0.95f;

	public float cap = 0.5f;

	public Transform target;

	public bool aiTarget;

	private Vector3 relativePosition;

	[HideInInspector]
	public new bool enabled = true;

	private AI ai;

	private void Start()
	{
		ai = GetComponentInParent<AI>();
	}

	private void FixedUpdate()
	{
		if (aiTarget && (bool)ai.target)
		{
			target = ai.target.transform;
		}
		if ((bool)target)
		{
			relativePosition = base.transform.InverseTransformPoint(target.position);
			float y = relativePosition.y;
			y = Mathf.Clamp(y, 0f - cap, cap);
			if (enabled)
			{
				rotationVelocity += y;
			}
			rotationVelocity *= friction;
			base.transform.Rotate(Vector3.right * rotationVelocity * turnSpeed, Space.World);
		}
	}

	public void Enable()
	{
		enabled = true;
	}

	public void Disable()
	{
		enabled = false;
	}
}
