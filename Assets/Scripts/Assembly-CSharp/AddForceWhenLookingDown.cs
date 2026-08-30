using UnityEngine;

public class AddForceWhenLookingDown : MonoBehaviour
{
	public float forceAmount;

	private Rigidbody rig;

	private void Start()
	{
		rig = GetComponent<Rigidbody>();
	}

	private void FixedUpdate()
	{
		float num = Mathf.Clamp((30f - Vector3.Angle(Vector3.down, base.transform.forward)) / 30f, 0f, 1f);
		rig.AddForce(base.transform.forward * num * forceAmount, ForceMode.Acceleration);
	}
}
