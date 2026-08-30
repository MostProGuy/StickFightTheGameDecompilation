using UnityEngine;

public class LookAtVelocity : MonoBehaviour
{
	private Rigidbody rig;

	private void Start()
	{
		rig = GetComponent<Rigidbody>();
	}

	private void Update()
	{
		rig.AddTorque(rig.velocity.magnitude * Time.deltaTime * 0.1f * (Vector3.Angle(base.transform.forward, rig.velocity) * Vector3.Cross(base.transform.forward, rig.velocity).normalized), ForceMode.Acceleration);
	}
}
