using UnityEngine;

public class AddForceToAllRigidbodyChildren : MonoBehaviour
{
	public float force;

	public bool selfForward;

	private void Start()
	{
		Rigidbody[] componentsInChildren = GetComponentsInChildren<Rigidbody>();
		foreach (Rigidbody rigidbody in componentsInChildren)
		{
			if (!selfForward)
			{
				rigidbody.AddForce(base.transform.forward * force, ForceMode.VelocityChange);
			}
			else
			{
				rigidbody.AddForce(rigidbody.transform.forward * force, ForceMode.VelocityChange);
			}
		}
	}

	private void Update()
	{
	}
}
