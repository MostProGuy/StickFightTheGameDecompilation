using UnityEngine;

public class AddJoints : MonoBehaviour
{
	public float breakForce = 100f;

	private void Start()
	{
		Rigidbody[] componentsInChildren = GetComponentsInChildren<Rigidbody>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (i > 0)
			{
				ConfigurableJoint configurableJoint = componentsInChildren[i].gameObject.AddComponent<ConfigurableJoint>();
				configurableJoint.xMotion = ConfigurableJointMotion.Locked;
				configurableJoint.yMotion = ConfigurableJointMotion.Locked;
				configurableJoint.zMotion = ConfigurableJointMotion.Locked;
				configurableJoint.angularXMotion = ConfigurableJointMotion.Locked;
				configurableJoint.angularYMotion = ConfigurableJointMotion.Locked;
				configurableJoint.angularZMotion = ConfigurableJointMotion.Locked;
				configurableJoint.breakForce = breakForce;
				configurableJoint.breakTorque = breakForce;
				configurableJoint.connectedBody = componentsInChildren[i - 1];
				configurableJoint.projectionMode = JointProjectionMode.PositionAndRotation;
			}
		}
	}

	private void Update()
	{
	}
}
