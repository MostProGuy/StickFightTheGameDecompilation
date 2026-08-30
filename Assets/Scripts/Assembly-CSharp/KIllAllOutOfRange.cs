using UnityEngine;

public class KIllAllOutOfRange : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
	}

	public void GO()
	{
		HealthHandler[] array = Object.FindObjectsOfType<HealthHandler>();
		foreach (HealthHandler healthHandler in array)
		{
			if (!healthHandler.GetComponent<Controller>().canFly && Vector3.Distance(base.transform.position, healthHandler.GetComponentInChildren<Torso>().transform.position) > 2.2f)
			{
				healthHandler.TakeDamage(1000f, null);
			}
		}
	}
}
