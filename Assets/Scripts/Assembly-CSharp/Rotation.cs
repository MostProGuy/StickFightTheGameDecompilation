using UnityEngine;

public class Rotation : MonoBehaviour
{
	private Vector3 targetPosition = Vector3.zero;

	public bool lookingRight;

	public float rotationForce;

	private CharacterInformation info;

	private Rigidbody hip;

	private void Start()
	{
		info = GetComponent<CharacterInformation>();
		hip = GetComponentInChildren<Hip>().GetComponent<Rigidbody>();
	}

	private void Update()
	{
		if (lookingRight)
		{
			targetPosition = hip.position + Vector3.forward * 10f;
		}
		else
		{
			targetPosition = hip.position - Vector3.forward * 10f;
		}
		float x = hip.transform.InverseTransformPoint(targetPosition).x;
		hip.AddTorque(Vector3.up * Time.deltaTime * rotationForce * (0f - x), ForceMode.Acceleration);
	}
}
