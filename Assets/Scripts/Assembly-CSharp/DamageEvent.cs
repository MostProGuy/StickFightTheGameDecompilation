using UnityEngine;

public class DamageEvent : MonoBehaviour
{
	public float damage = 25f;

	public bool inverse;

	private TargetHolder th;

	private void Start()
	{
		th = GetComponent<TargetHolder>();
	}

	public void GO()
	{
		if ((bool)th.part)
		{
			if (inverse)
			{
				th.part.TakeDamageWithParticle(damage, base.transform.position, -base.transform.forward, th.controller);
			}
			else
			{
				th.part.TakeDamageWithParticle(damage, base.transform.position, base.transform.forward, th.controller);
			}
		}
	}

	private void Update()
	{
	}
}
