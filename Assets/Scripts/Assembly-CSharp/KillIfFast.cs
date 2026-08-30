using UnityEngine;

public class KillIfFast : MonoBehaviour
{
	private Rigidbody rig;

	public float threshold;

	public bool onlyDown;

	private void Start()
	{
		rig = GetComponent<Rigidbody>();
		if (!rig)
		{
			rig = GetComponentInParent<Rigidbody>();
		}
	}

	private void Update()
	{
		if ((rig.velocity.magnitude > threshold && !onlyDown) || rig.velocity.y < 0f - threshold)
		{
			base.transform.tag = "Kill";
		}
		else
		{
			base.transform.tag = "Untagged";
		}
	}
}
