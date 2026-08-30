using UnityEngine;

public class AddForceOverTime : MonoBehaviour
{
	public enum Direction
	{
		Right = 0,
		Up = 1,
		Forward = 2
	}

	public Direction direction;

	public float amount;

	private Rigidbody rig;

	private void Start()
	{
		rig = GetComponent<Rigidbody>();
	}

	private void FixedUpdate()
	{
		if (direction == Direction.Right)
		{
			rig.AddForce(base.transform.right * amount, ForceMode.Acceleration);
		}
		if (direction == Direction.Up)
		{
			rig.AddForce(base.transform.up * amount, ForceMode.Acceleration);
		}
		if (direction == Direction.Forward)
		{
			rig.AddForce(base.transform.forward * amount, ForceMode.Acceleration);
		}
	}
}
