using UnityEngine;

public class AddForceByWobble : MonoBehaviour
{
	public Rigidbody rig;

	public float amount;

	private Wobble wobble;

	private void Start()
	{
		wobble = GetComponentInChildren<Wobble>();
	}

	private void FixedUpdate()
	{
		rig.AddForce(Vector3.up * amount * wobble.currentVelocity);
	}
}
