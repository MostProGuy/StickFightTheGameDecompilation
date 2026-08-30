using UnityEngine;

public class AddForceByRay : MonoBehaviour
{
	public float range = 5f;

	public float force;

	private Rigidbody rig;

	private Standing standing;

	private void Start()
	{
		rig = GetComponentInParent<Rigidbody>();
		standing = GetComponentInParent<Standing>();
		if (!rig && (bool)standing)
		{
			rig = base.transform.root.GetComponentInChildren<Torso>().GetComponent<Rigidbody>();
		}
	}

	private void Update()
	{
		Ray ray = new Ray(base.transform.position, base.transform.forward);
		RaycastHit hitInfo;
		Physics.SphereCast(ray, 0.3f, out hitInfo, range);
		if ((bool)hitInfo.transform)
		{
			float value = (range - Vector3.Distance(base.transform.position, hitInfo.point)) / range;
			value = Mathf.Clamp(value, 0f, 1f);
			rig.AddForce(-base.transform.forward * force * value, ForceMode.Force);
			if ((bool)standing)
			{
				standing.gravity = 0f;
			}
		}
	}
}
