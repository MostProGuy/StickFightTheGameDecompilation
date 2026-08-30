using UnityEngine;

public class AddForce : MonoBehaviour
{
	public float force;

	private Rigidbody rig;

	private void Start()
	{
		rig = GetComponent<Rigidbody>();
		rig.AddForce(base.transform.forward * force, ForceMode.VelocityChange);
	}
}
