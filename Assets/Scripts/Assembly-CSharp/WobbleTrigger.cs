using UnityEngine;

public class WobbleTrigger : MonoBehaviour
{
	private Wobble wobble;

	public float multiplier;

	private void Start()
	{
		wobble = GetComponent<Wobble>();
	}

	private void Update()
	{
	}

	private void OnTriggerStay(Collider other)
	{
		Rigidbody component = other.GetComponent<Rigidbody>();
		if ((bool)component)
		{
			wobble.inputVelocity += component.velocity.z * multiplier;
		}
	}
}
