using UnityEngine;

public class Glue : MonoBehaviour
{
	private ConfigurableJoint joint;

	private float counter;

	private Rigidbody rig;

	private Standing info;

	private Collider col;

	private void Start()
	{
		rig = GetComponent<Rigidbody>();
		col = GetComponentInChildren<SphereCollider>();
	}

	private void Update()
	{
		counter += Time.deltaTime;
		if ((bool)joint && joint.connectedBody != null && joint.connectedBody.gameObject.activeInHierarchy)
		{
			col.enabled = false;
			if ((bool)info)
			{
				info.gravity = 0f;
			}
			return;
		}
		if ((bool)joint)
		{
			Object.Destroy(joint);
			joint = null;
		}
		col.enabled = true;
		info = null;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if ((bool)collision.rigidbody && !joint && !collision.gameObject.GetComponent<Glue>())
		{
			joint = base.gameObject.AddComponent<ConfigurableJoint>();
			joint.xMotion = ConfigurableJointMotion.Locked;
			joint.yMotion = ConfigurableJointMotion.Locked;
			joint.zMotion = ConfigurableJointMotion.Locked;
			joint.angularXMotion = ConfigurableJointMotion.Locked;
			joint.angularYMotion = ConfigurableJointMotion.Locked;
			joint.angularZMotion = ConfigurableJointMotion.Locked;
			joint.projectionMode = JointProjectionMode.PositionAndRotation;
			joint.connectedBody = collision.rigidbody;
			joint.breakForce = 8000f;
			info = collision.transform.root.GetComponent<Standing>();
		}
	}
}
