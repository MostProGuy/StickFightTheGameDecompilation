using System.Collections.Generic;
using UnityEngine;

public class TriggerDamageOnce : MonoBehaviour
{
	public float damage;

	public float force;

	public float shake = 1f;

	private ScreenshakeHandler screenshake;

	private List<Controller> controllers = new List<Controller>();

	private Controller mUser;

	private void Awake()
	{
		screenshake = ScreenshakeHandler.Instance;
		mUser = GetComponentInParent<Controller>();
	}

	private void OnTriggerStay(Collider other)
	{
		if (other.transform.root == base.transform.root)
		{
			return;
		}
		BodyPart component = other.transform.GetComponent<BodyPart>();
		if (!component)
		{
			return;
		}
		Controller component2 = component.transform.root.GetComponent<Controller>();
		if (controllers.Contains(component2))
		{
			return;
		}
		if (MatchmakingHandler.IsNetworkMatch)
		{
			if ((bool)mUser)
			{
				if (!mUser.HasControl)
				{
					return;
				}
			}
			else if (!component.ParentController.HasControl)
			{
				return;
			}
			if (damage != 0f)
			{
				component.transform.root.GetComponent<HealthHandler>().TakeDamage(damage, null);
			}
			if (force != 0f)
			{
				component.transform.root.GetComponentInChildren<Torso>().GetComponent<Rigidbody>().AddForce(base.transform.forward * force, ForceMode.VelocityChange);
			}
			screenshake.AddShake(base.transform.forward * shake);
			controllers.Add(component2);
		}
		else
		{
			if (damage != 0f)
			{
				component.transform.root.GetComponent<HealthHandler>().TakeDamage(damage, null);
			}
			if (force != 0f)
			{
				component.transform.root.GetComponentInChildren<Torso>().GetComponent<Rigidbody>().AddForce(base.transform.forward * force, ForceMode.VelocityChange);
			}
			screenshake.AddShake(base.transform.forward * shake);
			controllers.Add(component2);
		}
	}
}
