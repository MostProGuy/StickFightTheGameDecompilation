using UnityEngine;

public class HelpUp : MonoBehaviour
{
	private Rigidbody rig;

	private ConstantForce force;

	private float forceAmount;

	private void Start()
	{
		rig = GetComponent<Rigidbody>();
		force = GetComponent<ConstantForce>();
		forceAmount = force.force.y;
	}

	private void Update()
	{
		if (rig != null && force != null)
		{
			if (rig.velocity.y > 0f)
			{
				force.force = new Vector3(0f, forceAmount, 0f);
			}
			else
			{
				force.force = new Vector3(0f, forceAmount * 0.75f, 0f);
			}
		}
	}
}
