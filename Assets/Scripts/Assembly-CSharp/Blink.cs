using UnityEngine;

public class Blink : MonoBehaviour
{
	private AimTarget aimTarget;

	private Renderers rends;

	private void Start()
	{
		aimTarget = base.transform.root.GetComponentInChildren<AimTarget>();
	}

	private void Update()
	{
	}

	public void DoBlink()
	{
		Rigidbody[] componentsInChildren = base.transform.root.GetComponentsInChildren<Rigidbody>();
		foreach (Rigidbody rigidbody in componentsInChildren)
		{
			rigidbody.MovePosition(rigidbody.position + aimTarget.transform.forward * 3f);
		}
		LeaveTrail(base.gameObject);
	}

	public void LeaveTrail(GameObject owner)
	{
		rends = owner.transform.root.GetComponentInChildren<Renderers>();
		GameObject gameObject = Object.Instantiate(rends.gameObject, rends.transform.position, rends.transform.rotation);
		MonoBehaviour[] componentsInChildren = gameObject.GetComponentsInChildren<MonoBehaviour>();
		foreach (MonoBehaviour monoBehaviour in componentsInChildren)
		{
			monoBehaviour.enabled = false;
		}
		SpriteRenderer[] componentsInChildren2 = gameObject.GetComponentsInChildren<SpriteRenderer>();
		foreach (SpriteRenderer spriteRenderer in componentsInChildren2)
		{
			spriteRenderer.gameObject.AddComponent<FadeSprite>();
		}
		LineRenderer[] componentsInChildren3 = gameObject.GetComponentsInChildren<LineRenderer>();
		foreach (LineRenderer lineRenderer in componentsInChildren3)
		{
			lineRenderer.gameObject.AddComponent<FadeSprite>();
		}
		gameObject.AddComponent<RemoveOnLevelChange>();
	}

	public void Go(Controller controller)
	{
		if (!(Mathf.Abs(base.transform.position.z) > 18f) && !(Mathf.Abs(base.transform.position.y) > 10f))
		{
			Vector3 vector = base.transform.position - controller.GetComponentInChildren<Torso>().transform.position + Vector3.up;
			Rigidbody[] componentsInChildren = controller.GetComponentsInChildren<Rigidbody>();
			foreach (Rigidbody rigidbody in componentsInChildren)
			{
				rigidbody.MovePosition(rigidbody.position + vector);
			}
			LeaveTrail(controller.gameObject);
		}
	}
}
