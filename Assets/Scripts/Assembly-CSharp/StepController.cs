using System.Collections;
using UnityEngine;

public class StepController : MonoBehaviour
{
	public float legAngle = 70f;

	public float switchTime = 0.3f;

	private float counter;

	private Transform rightLeg;

	private Transform leftLeg;

	private CharacterInformation info;

	private AudioSource au;

	public AudioClip[] steps;

	private Controller controller;

	private void Start()
	{
		RightLeg componentInChildren = GetComponentInChildren<RightLeg>();
		if ((bool)componentInChildren)
		{
			rightLeg = componentInChildren.transform;
		}
		LeftLeg componentInChildren2 = GetComponentInChildren<LeftLeg>();
		if ((bool)componentInChildren2)
		{
			leftLeg = componentInChildren2.transform;
		}
		info = GetComponent<CharacterInformation>();
		au = GetComponentInChildren<AudioSource>();
		controller = GetComponent<Controller>();
	}

	private void Update()
	{
		counter += Time.deltaTime;
		if ((bool)leftLeg && (bool)rightLeg && Vector3.Angle(rightLeg.up, leftLeg.up) > legAngle && counter > switchTime && info.sinceGrounded < 0.1f && info.paceState != 0)
		{
			info.leftSideForward = !info.leftSideForward;
			counter = 0f;
			StartCoroutine(PlayStepSound());
		}
	}

	private IEnumerator PlayStepSound()
	{
		yield return new WaitForSeconds(0.1f);
		if (controller != null && controller.PlayerActions != null)
		{
			au.PlayOneShot(steps[Random.Range(0, steps.Length)], 0.2f * Mathf.Abs(controller.PlayerActions.Movement.X));
		}
	}
}
