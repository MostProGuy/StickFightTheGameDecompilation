using UnityEngine;

public class FallingRock : MonoBehaviour
{
	private CodeAnimation animation;

	private Rigidbody rig;

	private bool done;

	private void Start()
	{
		animation = GetComponent<CodeAnimation>();
		rig = GetComponent<Rigidbody>();
	}

	private void Update()
	{
	}

	public void Go()
	{
		if (!done)
		{
			done = true;
			animation.Play();
		}
	}

	public void Fall()
	{
		Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
		foreach (Collider collider in componentsInChildren)
		{
			collider.enabled = false;
		}
		rig.isKinematic = false;
		rig.AddTorque(Vector3.right * Random.Range(-2, 2), ForceMode.VelocityChange);
		rig.AddForce(Vector3.up * Random.Range(2f, 3f), ForceMode.VelocityChange);
	}
}
