using UnityEngine;

public class AddCircularForce : MonoBehaviour
{
	private Rigidbody rig;

	public Transform center;

	public float amount;

	private void Start()
	{
		rig = GetComponent<Rigidbody>();
	}

	private void FixedUpdate()
	{
		rig.AddForce(Vector3.Cross(Vector3.right, base.transform.position - center.position).normalized * amount, ForceMode.Acceleration);
	}
}
