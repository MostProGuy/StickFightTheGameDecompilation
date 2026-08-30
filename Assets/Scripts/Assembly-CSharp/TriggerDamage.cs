using UnityEngine;

public class TriggerDamage : MonoBehaviour
{
	public float damagePerSecond;

	public float forcePerSecond;

	private ScreenshakeHandler screenshake;

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
			if (damagePerSecond != 0f)
			{
				component.TakeDamage(damagePerSecond * Time.deltaTime, null);
			}
			if (forcePerSecond != 0f)
			{
				component.GetComponent<Rigidbody>().AddForce(base.transform.forward * Time.deltaTime * forcePerSecond, ForceMode.Acceleration);
			}
			component.GetComponent<Rigidbody>().velocity *= 0f;
			screenshake.AddShake(base.transform.forward * 1f * Time.deltaTime);
		}
		else
		{
			if (damagePerSecond != 0f)
			{
				component.TakeDamage(damagePerSecond * Time.deltaTime, null);
			}
			if (forcePerSecond != 0f)
			{
				component.GetComponent<Rigidbody>().AddForce(base.transform.forward * Time.deltaTime * forcePerSecond, ForceMode.Acceleration);
			}
			component.GetComponent<Rigidbody>().velocity *= 0f;
			screenshake.AddShake(base.transform.forward * 1f * Time.deltaTime);
		}
	}
}
