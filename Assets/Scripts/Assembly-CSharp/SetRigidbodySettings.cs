using UnityEngine;

public class SetRigidbodySettings : MonoBehaviour
{
	public float maxAngular;

	private void Start()
	{
		Rigidbody component = GetComponent<Rigidbody>();
		if (component != null)
		{
			component.maxAngularVelocity = maxAngular;
		}
	}
}
