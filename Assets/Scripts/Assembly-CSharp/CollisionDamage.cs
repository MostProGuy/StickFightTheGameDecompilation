using UnityEngine;

public class CollisionDamage : MonoBehaviour
{
	public float damage;

	public float force;

	private Rigidbody rig;

	public float selfForce;

	private void Start()
	{
		rig = GetComponent<Rigidbody>();
	}

	private void OnCollisionEnter(Collision collision)
	{
		HealthHandler component = collision.transform.root.GetComponent<HealthHandler>();
		if ((bool)component)
		{
			component.TakeDamage(damage, null);
			Rigidbody component2 = component.GetComponentInChildren<Torso>().GetComponent<Rigidbody>();
			Vector3 vector = (component2.position - base.transform.position).normalized * force;
			if (MatchmakingHandler.IsNetworkMatch)
			{
				byte index = component2.GetComponent<RigidBodyIndexHolder>().Index;
				component.GetComponent<NetworkPlayer>().SendAddedForce(index, vector, ForceMode.VelocityChange);
			}
			else
			{
				component2.AddForce(vector, ForceMode.VelocityChange);
			}
			Vector3 vector2 = (component2.position - base.transform.position).normalized * (0f - selfForce);
			rig.AddForce(vector2, ForceMode.VelocityChange);
		}
	}
}
