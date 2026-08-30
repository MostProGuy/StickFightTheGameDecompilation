using UnityEngine;

public class KeepUpUsingDot : MonoBehaviour
{
	public enum torgueForceDir
	{
		Up = 0,
		Right = 1,
		Forward = 2
	}

	public torgueForceDir localDirection;

	public torgueForceDir worldDirection;

	public torgueForceDir localTorgueDirection;

	public float force;

	public float dotThreshold = -1f;

	public float cap = float.PositiveInfinity;

	private Rigidbody rig;

	private void Start()
	{
		rig = GetComponent<Rigidbody>();
	}

	private void FixedUpdate()
	{
		Vector3 lhs = Vector3.zero;
		switch (localDirection)
		{
		case torgueForceDir.Up:
			lhs = base.transform.up;
			break;
		case torgueForceDir.Right:
			lhs = base.transform.right;
			break;
		case torgueForceDir.Forward:
			lhs = base.transform.forward;
			break;
		}
		Vector3 vector = Vector3.zero;
		switch (worldDirection)
		{
		case torgueForceDir.Up:
			vector = Vector3.up;
			break;
		case torgueForceDir.Right:
			vector = Vector3.right;
			break;
		case torgueForceDir.Forward:
			vector = Vector3.forward;
			break;
		}
		Vector3 vector2 = Vector3.zero;
		switch (localTorgueDirection)
		{
		case torgueForceDir.Up:
			vector2 = base.transform.up;
			break;
		case torgueForceDir.Right:
			vector2 = base.transform.right;
			break;
		case torgueForceDir.Forward:
			vector2 = base.transform.forward;
			break;
		}
		if (Vector3.Dot(lhs, -vector) > dotThreshold)
		{
			float num = 0f;
			switch (localTorgueDirection)
			{
			case torgueForceDir.Up:
				num = base.transform.rotation.eulerAngles.y;
				break;
			case torgueForceDir.Right:
				num = base.transform.rotation.eulerAngles.x;
				break;
			case torgueForceDir.Forward:
				num = base.transform.rotation.eulerAngles.z;
				break;
			}
			if (num > 0f && num < 180f)
			{
				num = Mathf.Clamp(Mathf.Abs(num), 0f, cap);
				rig.AddTorque(vector2 * (0f - force) * num, ForceMode.Acceleration);
			}
			else
			{
				num = Mathf.Clamp(Mathf.Abs(num - 360f), 0f, cap);
				rig.AddTorque(vector2 * force * num, ForceMode.Acceleration);
			}
		}
	}
}
