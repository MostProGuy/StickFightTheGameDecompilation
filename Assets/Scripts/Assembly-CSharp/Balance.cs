using UnityEngine;

public class Balance : MonoBehaviour
{
	private CharacterInformation info;

	private Rigidbody leftKnee;

	private Rigidbody rightKnee;

	private Rigidbody leftLeg;

	private Rigidbody rightLeg;

	public float balanceForce = 1f;

	private void Start()
	{
		info = GetComponent<CharacterInformation>();
		LeftKnee componentInChildren = GetComponentInChildren<LeftKnee>();
		if ((bool)componentInChildren)
		{
			leftKnee = componentInChildren.GetComponent<Rigidbody>();
		}
		RightKnee componentInChildren2 = GetComponentInChildren<RightKnee>();
		if ((bool)componentInChildren2)
		{
			rightKnee = componentInChildren2.GetComponent<Rigidbody>();
		}
		LeftLeg componentInChildren3 = GetComponentInChildren<LeftLeg>();
		if ((bool)componentInChildren3)
		{
			leftLeg = componentInChildren3.GetComponent<Rigidbody>();
		}
		RightLeg componentInChildren4 = GetComponentInChildren<RightLeg>();
		if ((bool)componentInChildren4)
		{
			rightLeg = componentInChildren4.GetComponent<Rigidbody>();
		}
	}

	private void Update()
	{
		if (info.sinceFallen > 0f && info.isGrounded)
		{
			StayBalanced();
		}
	}

	private void StayBalanced()
	{
		if ((bool)rightLeg || (bool)leftLeg)
		{
			Rigidbody rigidbody = rightKnee;
			if (Vector3.Angle(leftLeg.transform.up, Vector3.up) < Vector3.Angle(rightLeg.transform.up, Vector3.up))
			{
				rigidbody = leftKnee;
			}
			Vector3 vector = leftLeg.transform.up + rightLeg.transform.up;
			vector.y = 0f;
			float num = 1f;
			if (info.paceState == 0)
			{
				num = 1.5f;
			}
			rigidbody.AddForce(vector * 10000f * balanceForce * num * Time.deltaTime, ForceMode.Acceleration);
		}
	}
}
