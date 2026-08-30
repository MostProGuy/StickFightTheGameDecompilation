using UnityEngine;

public class Grabber : MonoBehaviour
{
	private GrabHandler grabHandler;

	public ConfigurableJoint joint;

	private Rigidbody rig;

	private void Start()
	{
		grabHandler = base.transform.root.GetComponent<GrabHandler>();
		rig = GetComponent<Rigidbody>();
	}

	private void Update()
	{
		if (!grabHandler.isGrabbing && (bool)joint)
		{
			Object.Destroy(joint);
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (!grabHandler || grabHandler.grabStrength < 0.5f || !grabHandler.isGrabbing || (bool)joint || collision.gameObject.layer == 26 || collision.gameObject.layer == 27)
		{
			return;
		}
		if (!collision.rigidbody)
		{
			for (int i = 0; i < grabHandler.grabbers.Length; i++)
			{
				if ((bool)grabHandler.grabbers[i].joint && grabHandler.grabbers[i].joint.connectedBody != null)
				{
					return;
				}
			}
			grabHandler.StartGrab(collision.rigidbody);
			grabHandler.StartGrab(null);
		}
		else
		{
			for (int j = 0; j < grabHandler.grabbers.Length; j++)
			{
				if ((bool)grabHandler.grabbers[j].joint && (bool)grabHandler.grabbers[j].joint.connectedBody && grabHandler.grabbers[j].joint.connectedBody != collision.rigidbody)
				{
					return;
				}
			}
			grabHandler.StartGrab(collision.rigidbody);
		}
		joint = rig.gameObject.AddComponent<ConfigurableJoint>();
		joint.xMotion = ConfigurableJointMotion.Locked;
		joint.yMotion = ConfigurableJointMotion.Locked;
		joint.zMotion = ConfigurableJointMotion.Locked;
		joint.angularXMotion = ConfigurableJointMotion.Locked;
		joint.angularYMotion = ConfigurableJointMotion.Locked;
		joint.angularZMotion = ConfigurableJointMotion.Locked;
		joint.projectionMode = JointProjectionMode.PositionAndRotation;
		joint.anchor = rig.transform.InverseTransformPoint(collision.contacts[0].point);
		if ((bool)collision.rigidbody)
		{
			joint.connectedBody = collision.rigidbody;
		}
	}

	public void LetItGo()
	{
		if ((bool)joint)
		{
			Object.Destroy(joint);
		}
	}
}
