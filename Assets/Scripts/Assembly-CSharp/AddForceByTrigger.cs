using UnityEngine;

public class AddForceByTrigger : MonoBehaviour
{
	public float force;

	public float range;

	private void Start()
	{
	}

	private void Update()
	{
	}

	private void OnTriggerStay(Collider other)
	{
		Rigidbody componentInParent = other.GetComponentInParent<Rigidbody>();
		if ((bool)componentInParent)
		{
			float value = (range - Vector3.Distance(base.transform.parent.position, componentInParent.position)) / range;
			value = Mathf.Clamp(value, 0f, 1f);
			componentInParent.AddForce(base.transform.forward * force * value, ForceMode.Force);
			Standing component = componentInParent.transform.root.GetComponent<Standing>();
			if ((bool)component)
			{
				component.gravity = 0f;
			}
		}
	}
}
