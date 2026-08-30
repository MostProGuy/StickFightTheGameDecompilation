using UnityEngine;

public class BluntCollision : MonoBehaviour
{
	public string specialEffect;

	private TeamHolder th;

	private void Start()
	{
		th = GetComponent<TeamHolder>();
	}

	private void Update()
	{
	}

	private void OnCollisionEnter(Collision collision)
	{
		if ((bool)collision.rigidbody)
		{
			Controller component = collision.transform.root.GetComponent<Controller>();
			if ((bool)component && component.playerID != th.team && specialEffect != string.Empty)
			{
				base.transform.SendMessage(specialEffect, collision.rigidbody);
			}
		}
	}
}
