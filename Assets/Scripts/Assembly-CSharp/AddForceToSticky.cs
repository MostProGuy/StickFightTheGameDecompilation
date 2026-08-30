using UnityEngine;

public class AddForceToSticky : MonoBehaviour
{
	private StickyObject sticky;

	private Rigidbody rig;

	public float force;

	private void Start()
	{
		sticky = GetComponent<StickyObject>();
	}

	private void FixedUpdate()
	{
		if ((bool)sticky.stickObject)
		{
			if (!rig)
			{
				rig = sticky.hitR;
			}
			else
			{
				rig.AddForce(base.transform.forward * force, ForceMode.Force);
			}
		}
	}
}
