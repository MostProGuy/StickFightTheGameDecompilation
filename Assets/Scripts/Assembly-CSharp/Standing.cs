using UnityEngine;

public class Standing : MonoBehaviour
{
	public RigidbodyMovement[] rigsToLift;

	public AnimationCurve standForceCurve;

	public AnimationCurve getUpCurve;

	public float standForceMultiplier = 1f;

	public float gravityForceMultiplayer = 1f;

	private CharacterInformation info;

	public float gravity;

	public Rigidbody duckRig;

	private ConstantForce leftKneeForce;

	private ConstantForce rightKneeForce;

	private Controller controller;

	private GrabHandler grabbing;

	private Rigidbody[] rigs;

	private bool useGravity = true;

	private bool mHasControl;

	private float mLeftStickYValue;

	private HealthHandler health;

	public float LeftStickYValue
	{
		get
		{
			return mLeftStickYValue;
		}
	}

	private void Start()
	{
		rigs = GetComponentsInChildren<Rigidbody>();
		info = GetComponent<CharacterInformation>();
		health = GetComponent<HealthHandler>();
		controller = GetComponent<Controller>();
		grabbing = GetComponent<GrabHandler>();
		LeftKnee componentInChildren = GetComponentInChildren<LeftKnee>();
		RightKnee componentInChildren2 = GetComponentInChildren<RightKnee>();
		if ((bool)componentInChildren)
		{
			leftKneeForce = componentInChildren.GetComponent<ConstantForce>();
		}
		if ((bool)componentInChildren2)
		{
			rightKneeForce = componentInChildren2.GetComponent<ConstantForce>();
		}
		if (!MatchmakingHandler.IsNetworkMatch)
		{
			mHasControl = true;
		}
	}

	public void TakeLocalControl()
	{
		mHasControl = true;
	}

	public void StartGravity()
	{
		useGravity = true;
	}

	public void StopGravity()
	{
		useGravity = false;
	}

	private void FixedUpdate()
	{
		if (Time.timeScale < 0.1f)
		{
			return;
		}
		if (!controller.canFly && !controller.isAI && !grabbing.isHoldingSomething && controller.HasControl && !ChatManager.isTyping && !info.isDead)
		{
			duckRig.AddForce(Vector3.up * 60f * Time.fixedDeltaTime * Mathf.Clamp(((!mHasControl) ? mLeftStickYValue : ((!PauseManager.isPaused) ? controller.PlayerActions.Movement.Y : 0f)) + 0.5f, -1f, 0f) * 2000f, ForceMode.Acceleration);
		}
		if ((bool)leftKneeForce && (bool)rightKneeForce)
		{
			if (info.sinceGrounded < 0.3f && info.sinceFallen > 0f)
			{
				leftKneeForce.enabled = true;
				rightKneeForce.enabled = true;
			}
			else
			{
				leftKneeForce.enabled = false;
				rightKneeForce.enabled = false;
			}
		}
		if (info.isGrounded || grabbing.isHoldingSomethingAnchored || !useGravity || controller.canFly)
		{
			gravity = 0f;
		}
		else
		{
			gravity += Time.fixedDeltaTime;
		}
		if (info.sinceFallen > 0f)
		{
			ApplyGravity();
			if (info.isGrounded)
			{
				Stand();
			}
		}
	}

	private void ApplyGravity()
	{
		Rigidbody[] array = rigs;
		foreach (Rigidbody rigidbody in array)
		{
			if (!(rigidbody.transform.parent.tag == "IgnoreRigidbody"))
			{
				rigidbody.AddForce(Vector3.down * Time.fixedDeltaTime * gravity * gravityForceMultiplayer, ForceMode.Acceleration);
			}
		}
	}

	private void Stand()
	{
		float num = 0f;
		if (!controller.isAI && !grabbing.isHoldingSomething && !PauseManager.isPaused && !ChatManager.isTyping && !info.isDead)
		{
			num = Mathf.Clamp(((!mHasControl) ? mLeftStickYValue : controller.PlayerActions.Movement.Y) + 0.5f, -1f, 0f) * 2f;
		}
		float num2 = standForceCurve.Evaluate(info.shortestDistanceFromHeadToGround - num) * standForceMultiplier;
		if (info.sinceFallen < 1f)
		{
			num2 *= getUpCurve.Evaluate(info.sinceFallen * 1f);
		}
		int num3 = rigsToLift.Length;
		for (int i = 0; i < num3; i++)
		{
			rigsToLift[i].rigidbody.AddForce(Vector3.up * (num2 + 100f * rigsToLift[i].rigidbody.velocity.magnitude) * rigsToLift[i].forceMultiplier * Time.fixedDeltaTime, ForceMode.Acceleration);
		}
	}

	public void SetNewLeftStickYValue(float yValue)
	{
		mLeftStickYValue = yValue;
	}
}
