using UnityEngine;

public class KillingFloor : MonoBehaviour
{
	private GameManager manager;

	private void Start()
	{
		manager = GameManager.Instance;
	}

	private void Update()
	{
		foreach (Controller item in manager.playersAlive)
		{
			Torso componentInChildren = item.GetComponentInChildren<Torso>();
			if ((bool)componentInChildren && componentInChildren.transform.position.y < base.transform.position.y && item.HasControl)
			{
				componentInChildren.transform.root.GetComponentInChildren<HealthHandler>().TakeDamage(200f, null, DamageType.Other, false, componentInChildren.transform.position, Vector3.up);
			}
		}
	}
}
