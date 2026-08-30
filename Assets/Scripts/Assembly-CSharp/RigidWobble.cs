using UnityEngine;

public class RigidWobble : MonoBehaviour
{
	public Rigidbody rig;

	private Wobble wobble;

	private void Start()
	{
		wobble = GetComponent<Wobble>();
	}

	private void Update()
	{
		wobble.inputVelocity += rig.velocity.z * 0.01f;
	}
}
