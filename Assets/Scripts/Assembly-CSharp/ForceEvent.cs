using UnityEngine;

public class ForceEvent : MonoBehaviour
{
	public float force;

	public float torsoForce;

	private TargetHolder th;

	private void Start()
	{
		th = GetComponent<TargetHolder>();
	}

	public void GO()
	{
		if (!th.rig)
		{
			return;
		}
		th.rig.AddForce(base.transform.forward * force, ForceMode.Impulse);
		if (torsoForce != 0f)
		{
			Torso componentInChildren = th.rig.transform.root.GetComponentInChildren<Torso>();
			if ((bool)componentInChildren)
			{
				componentInChildren.GetComponent<Rigidbody>().AddForce(base.transform.forward * torsoForce, ForceMode.Impulse);
			}
		}
	}

	private void Update()
	{
	}
}
