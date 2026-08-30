using UnityEngine;

public class AttachObject : MonoBehaviour
{
	public float limit;

	public float spring;

	private Rigidbody rig;

	private Rigidbody otherR;

	private bool done;

	private LineRenderer line;

	private void Start()
	{
		rig = GetComponent<Rigidbody>();
		line = GetComponentInChildren<LineRenderer>();
	}

	private void Update()
	{
		if (done && (bool)line)
		{
			line.SetPosition(0, base.transform.position);
			line.SetPosition(1, otherR.position);
		}
	}

	private void Attach(Rigidbody otherRig)
	{
		if (!done)
		{
			done = true;
			otherR = otherRig;
			ConfigurableJoint configurableJoint = rig.gameObject.AddComponent<ConfigurableJoint>();
			configurableJoint.xMotion = ConfigurableJointMotion.Limited;
			configurableJoint.yMotion = ConfigurableJointMotion.Limited;
			configurableJoint.zMotion = ConfigurableJointMotion.Limited;
			configurableJoint.angularXMotion = ConfigurableJointMotion.Free;
			configurableJoint.angularYMotion = ConfigurableJointMotion.Free;
			configurableJoint.angularZMotion = ConfigurableJointMotion.Free;
			SoftJointLimit linearLimit = configurableJoint.linearLimit;
			linearLimit.limit = limit;
			configurableJoint.linearLimit = linearLimit;
			SoftJointLimitSpring linearLimitSpring = configurableJoint.linearLimitSpring;
			linearLimitSpring.spring = spring;
			configurableJoint.linearLimitSpring = linearLimitSpring;
			configurableJoint.projectionMode = JointProjectionMode.PositionAndRotation;
			configurableJoint.connectedBody = otherRig;
		}
	}
}
