using UnityEngine;

public class AddRandomTorqueAndForce : MonoBehaviour
{
	public bool local;

	public Vector3 force;

	public Vector3 torque;

	[Range(0f, 1f)]
	public float random;

	private Rigidbody rig;

	private void Start()
	{
		rig = GetComponent<Rigidbody>();
		force *= 1f - random;
		torque *= 1f - random;
		force += Random.insideUnitSphere * random;
		torque += Random.insideUnitSphere * random;
		if (local)
		{
			force = base.transform.InverseTransformDirection(force);
		}
		rig.AddForce(force, ForceMode.VelocityChange);
		if (local)
		{
			torque = base.transform.InverseTransformDirection(torque);
		}
		rig.AddTorque(torque, ForceMode.VelocityChange);
	}

	private void Update()
	{
	}
}
