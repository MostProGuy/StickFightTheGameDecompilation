using UnityEngine;

public class RayCastForward : MonoBehaviour
{
	private ProjectileCollision projectileCollision;

	private Vector3 lastPosition;

	public float speed = 150f;

	public float friction;

	public bool hardFriction;

	public LayerMask mask;

	public bool dontChangeMask;

	public float spread;

	public bool ignorePlayers;

	public bool ignoreRotation;

	public bool onlyBosses;

	private void Start()
	{
		base.transform.position = new Vector3(0f, base.transform.position.y, base.transform.position.z);
		if (!ignoreRotation)
		{
			base.transform.rotation = Quaternion.Euler(base.transform.rotation.eulerAngles.x + Random.Range(0f - spread, spread), base.transform.rotation.eulerAngles.y + Random.Range(0f - spread, spread), base.transform.rotation.eulerAngles.z + Random.Range(0f - spread, spread));
		}
		if (!ignoreRotation)
		{
			base.transform.rotation = Quaternion.LookRotation(new Vector3(0f, base.transform.forward.y, base.transform.forward.z));
		}
		projectileCollision = GetComponent<ProjectileCollision>();
		lastPosition = base.transform.position;
	}

	private void FixedUpdate()
	{
		if (!ignoreRotation)
		{
			base.transform.rotation = Quaternion.LookRotation(new Vector3(0f, base.transform.forward.y, base.transform.forward.z));
		}
		base.transform.position += 0.0166f * speed * base.transform.forward;
		Vector3 direction = base.transform.position - lastPosition;
		if (friction != 0f)
		{
			if (hardFriction)
			{
				speed -= speed * 0.01f * friction;
			}
			else
			{
				speed -= Mathf.Pow(speed, 2f) * 0.001f * friction;
			}
		}
		Ray ray = new Ray(lastPosition, direction);
		lastPosition = base.transform.position;
		RaycastHit hitInfo;
		Physics.Raycast(ray, out hitInfo, direction.magnitude, mask);
		if (!hitInfo.transform || (ignorePlayers && (bool)hitInfo.transform.root.GetComponent<CharacterInformation>()))
		{
			Physics.SphereCast(ray, 0.3f, out hitInfo, direction.magnitude, mask);
		}
		else
		{
			Debug.Log("I DID NOT NEED A SPHERECAST");
		}
		if (!hitInfo.transform || (ignorePlayers && (bool)hitInfo.transform.root.GetComponent<CharacterInformation>()))
		{
			return;
		}
		if (onlyBosses)
		{
			Controller component = hitInfo.transform.root.GetComponent<Controller>();
			if ((bool)component && !component.canFly)
			{
				return;
			}
		}
		bool flag = projectileCollision.HitObject(hitInfo);
		Gravity component2 = GetComponent<Gravity>();
		if (!flag)
		{
			if ((bool)component2)
			{
				component2.enabled = false;
			}
		}
		else if ((bool)component2)
		{
			component2.force = 0f;
		}
	}
}
