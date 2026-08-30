using UnityEngine;

public class JointAnimation : MonoBehaviour
{
	public AnimationInfo[] animations;

	private Rigidbody rig;

	private Controller controller;

	private Rigidbody hip;

	private void Start()
	{
		rig = GetComponent<Rigidbody>();
		controller = base.transform.root.GetComponent<Controller>();
		hip = base.transform.root.GetComponentInChildren<Hip>().GetComponentInChildren<Rigidbody>();
	}

	private void Update()
	{
	}

	public void Animate(int paceState, bool leftForward)
	{
		AnimationInfo[] array = animations;
		foreach (AnimationInfo animationInfo in array)
		{
			Vector3 vector = Vector3.zero;
			if (animationInfo.direction == AnimationInfo.Direction.SelfRight)
			{
				vector = base.transform.right;
			}
			if (animationInfo.direction == AnimationInfo.Direction.MainBodyVelocity)
			{
				vector = hip.velocity.normalized;
			}
			if (animationInfo.type == AnimationInfo.AnimationType.Torque)
			{
				float num = 1f;
				if (!controller.isAI && controller.HasControl)
				{
					num = Mathf.Abs(controller.PlayerActions.Movement.X);
				}
				if ((leftForward && animationInfo.isLeftSide) || (!leftForward && !animationInfo.isLeftSide))
				{
					rig.AddTorque(vector * Time.deltaTime * num * (0f - animationInfo.backForceMultiplier), ForceMode.Acceleration);
				}
				else
				{
					rig.AddTorque(vector * Time.deltaTime * num * (0f - animationInfo.forwardForceMultiplier), ForceMode.Acceleration);
				}
			}
			if (animationInfo.type == AnimationInfo.AnimationType.Force)
			{
				float num2 = 1f;
				if (!controller.isAI && controller.HasControl)
				{
					num2 = Mathf.Abs(controller.PlayerActions.Movement.X);
				}
				if ((leftForward && animationInfo.isLeftSide) || (!leftForward && !animationInfo.isLeftSide))
				{
					rig.AddForce(vector * Time.deltaTime * num2 * (0f - animationInfo.backForceMultiplier), ForceMode.Acceleration);
				}
				else
				{
					rig.AddForce(vector * Time.deltaTime * num2 * (0f - animationInfo.forwardForceMultiplier), ForceMode.Acceleration);
				}
			}
		}
	}
}
