using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
	public float radius;

	public float force;

	public float nonPlayerForce;

	public float nonPlayerVelocityChange;

	public float damage;

	public float screenShakeForce;

	public float fall;

	private ScreenshakeHandler screenShake;

	public Controller damager;

	public bool auto = true;

	public int onlyThisLayer = -1;

	public bool spam;

	public bool removeForce;

	public float stopAfter;

	private DamagerHolder th;

	private void Start()
	{
		th = GetComponent<DamagerHolder>();
		screenShake = ScreenshakeHandler.Instance;
		if (auto)
		{
			Explode();
		}
	}

	private void FixedUpdate()
	{
		stopAfter -= Time.deltaTime;
		if (spam && stopAfter > 0f)
		{
			Explode();
		}
	}

	public void Explode()
	{
		List<RigidbodySettings> list = new List<RigidbodySettings>();
		if ((bool)screenShake)
		{
			screenShake.AddShake((screenShake.transform.position - base.transform.position).normalized * screenShakeForce);
		}
		Collider[] array = Physics.OverlapSphere(base.transform.position, radius);
		foreach (Collider collider in array)
		{
			if (!collider)
			{
				continue;
			}
			Rigidbody componentInParent = collider.GetComponentInParent<Rigidbody>();
			if (!componentInParent || !(componentInParent.transform.root != base.transform.root) || (onlyThisLayer != -1 && componentInParent.gameObject.layer != onlyThisLayer))
			{
				continue;
			}
			float num = Mathf.Clamp(componentInParent.mass / 500f, 0.01f, 1f);
			BodyPart component = componentInParent.GetComponent<BodyPart>();
			if ((bool)component && damage > 0f)
			{
				if ((bool)th)
				{
					Controller component2 = component.transform.root.GetComponent<Controller>();
					if ((bool)component2 && component2 == th.damager)
					{
						continue;
					}
				}
				float num2 = (radius - Vector3.Distance(base.transform.position, component.transform.position)) / radius;
				component.TakeDamage(damage, damager);
				if (fall != 0f)
				{
					RigidbodySettings component3 = component.transform.root.GetComponent<RigidbodySettings>();
					if ((bool)component3 && !list.Contains(component3))
					{
						list.Add(component3);
						for (int j = 0; j < component3.dragHandlers.Length; j++)
						{
							component3.dragHandlers[j].setDragNow(2f);
						}
						component.transform.root.GetComponent<CharacterInformation>().sinceFallen = (0f - fall) * num2;
					}
				}
			}
			else
			{
				componentInParent.AddExplosionForce(nonPlayerForce * num, base.transform.position, radius, 1f, ForceMode.Impulse);
				componentInParent.AddExplosionForce(nonPlayerVelocityChange, base.transform.position, radius, 1f, ForceMode.VelocityChange);
			}
			componentInParent.AddExplosionForce(force * num, base.transform.position, radius, 1f, ForceMode.Impulse);
			if (removeForce && !componentInParent.GetComponent<Weapon>())
			{
				componentInParent.angularVelocity *= 0f;
				componentInParent.velocity *= 0f;
				componentInParent.drag = 999f;
				componentInParent.angularDrag = 999f;
			}
			DestructiblePiece component4 = componentInParent.GetComponent<DestructiblePiece>();
			if ((bool)component4)
			{
				component4.Collide((componentInParent.transform.position - base.transform.position).normalized * damage * 5f, 1f);
				component4.damager = damager;
			}
		}
	}
}
