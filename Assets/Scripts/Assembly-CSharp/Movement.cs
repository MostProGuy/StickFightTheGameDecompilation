using UnityEngine;

public class Movement : MonoBehaviour
{
	public RigidbodyMovement[] rigsToMove;

	public float forceMultiplier;

	public float jumpForceMultiplier;

	public float jumpTime = 0.5f;

	private Standing standing;

	private CharacterInformation info;

	private Controller controller;

	private GrabHandler grabHandler;

	private Fighting fighting;

	private Rigidbody[] rigidbodies;

	private ScreenshakeHandler screenshake;

	private AudioSource au;

	public AudioClip[] jumpClips;

	public Vector3 flyVelocity = Vector3.zero;

	private Rigidbody leftHand;

	private Rigidbody rightHand;

	private void Start()
	{
		fighting = GetComponent<Fighting>();
		standing = GetComponent<Standing>();
		info = GetComponent<CharacterInformation>();
		controller = GetComponent<Controller>();
		grabHandler = GetComponent<GrabHandler>();
		au = GetComponentInChildren<AudioSource>();
		BodyPart[] componentsInChildren = GetComponentsInChildren<BodyPart>();
		rigidbodies = new Rigidbody[componentsInChildren.Length];
		for (int i = 0; i < rigidbodies.Length; i++)
		{
			rigidbodies[i] = componentsInChildren[i].GetComponent<Rigidbody>();
		}
		screenshake = ScreenshakeHandler.Instance;
		rightHand = GetComponentInChildren<RightHand>().GetComponent<Rigidbody>();
		leftHand = GetComponentInChildren<LeftHand>().GetComponent<Rigidbody>();
	}

	private void FixedUpdate()
	{
		flyVelocity *= 0.95f;
		if (controller.canFly)
		{
			MoveFly(flyVelocity);
			MoveFly(Vector3.up * 0.37f);
			leftHand.AddForce(Vector3.down * 2000f * Time.fixedDeltaTime + Vector3.forward * 2000f * Time.fixedDeltaTime, ForceMode.Acceleration);
			rightHand.AddForce(Vector3.down * 2000f * Time.fixedDeltaTime + Vector3.forward * -2000f * Time.fixedDeltaTime, ForceMode.Acceleration);
		}
	}

	private void MoveFly(Vector3 direction)
	{
		if (!(info.sinceFallen < 0f))
		{
			Rigidbody[] array = rigidbodies;
			foreach (Rigidbody rigidbody in array)
			{
				rigidbody.AddForce(direction * forceMultiplier * fighting.movementMultiplier * Time.deltaTime, ForceMode.Acceleration);
			}
			RigidbodyMovement[] array2 = rigsToMove;
			foreach (RigidbodyMovement rigidbodyMovement in array2)
			{
				rigidbodyMovement.rigidbody.AddForce(direction * rigidbodyMovement.forceMultiplier * fighting.movementMultiplier * Time.deltaTime, ForceMode.Acceleration);
			}
		}
	}

	public void Fly(Vector3 direction)
	{
		flyVelocity += direction * Time.deltaTime * 10f;
	}

	public void MoveRight()
	{
		if (!(info.sinceFallen < 0f))
		{
			float num = 1f;
			if (!controller.isAI)
			{
				num = Mathf.Abs(controller.HasControl ? controller.PlayerActions.Movement.X : ((!(standing.LeftStickYValue < -0.5f)) ? 0.6f : 0f));
			}
			if (grabHandler.isHoldingSomething)
			{
				num *= 0.1f;
			}
			Rigidbody[] array = rigidbodies;
			foreach (Rigidbody rigidbody in array)
			{
				rigidbody.AddForce(-Vector3.forward * forceMultiplier * fighting.movementMultiplier * num * Time.deltaTime, ForceMode.Acceleration);
			}
			RigidbodyMovement[] array2 = rigsToMove;
			foreach (RigidbodyMovement rigidbodyMovement in array2)
			{
				rigidbodyMovement.rigidbody.AddForce(-Vector3.forward * rigidbodyMovement.forceMultiplier * fighting.movementMultiplier * num * Time.deltaTime, ForceMode.Acceleration);
			}
		}
	}

	public void Move(float direction)
	{
		if (!(info.sinceFallen < 0f))
		{
			float num = 1f;
			if (!controller.isAI)
			{
				num = Mathf.Abs(controller.PlayerActions.Movement.X);
			}
			if (grabHandler.isHoldingSomething)
			{
				num *= 0.1f;
			}
			Rigidbody[] array = rigidbodies;
			foreach (Rigidbody rigidbody in array)
			{
				rigidbody.AddForce(direction * Vector3.forward * forceMultiplier * fighting.movementMultiplier * num * Time.deltaTime, ForceMode.Acceleration);
			}
			RigidbodyMovement[] array2 = rigsToMove;
			foreach (RigidbodyMovement rigidbodyMovement in array2)
			{
				rigidbodyMovement.rigidbody.AddForce(direction * Vector3.forward * rigidbodyMovement.forceMultiplier * fighting.movementMultiplier * num * Time.deltaTime, ForceMode.Acceleration);
			}
		}
	}

	public void MoveLeft()
	{
		if (!(info.sinceFallen < 0f))
		{
			float num = 1f;
			if (!controller.isAI)
			{
				num = Mathf.Abs(controller.HasControl ? controller.PlayerActions.Movement.X : ((!(standing.LeftStickYValue < -0.5f)) ? 0.6f : 0f));
			}
			if (grabHandler.isHoldingSomething)
			{
				num *= 0.1f;
			}
			Rigidbody[] array = rigidbodies;
			foreach (Rigidbody rigidbody in array)
			{
				rigidbody.AddForce(Vector3.forward * forceMultiplier * fighting.movementMultiplier * num * Time.deltaTime, ForceMode.Acceleration);
			}
			RigidbodyMovement[] array2 = rigsToMove;
			foreach (RigidbodyMovement rigidbodyMovement in array2)
			{
				rigidbodyMovement.rigidbody.AddForce(Vector3.forward * rigidbodyMovement.forceMultiplier * fighting.movementMultiplier * num * Time.deltaTime, ForceMode.Acceleration);
			}
		}
	}

	public bool Jump(bool force = false, bool forceWallJump = false)
	{
		bool result = DoJump(force, forceWallJump);
		au.PlayOneShot(jumpClips[Random.Range(0, jumpClips.Length)]);
		return result;
	}

	private bool DoJump(bool force = false, bool forceWallJump = false)
	{
		bool result = false;
		standing.gravity = jumpTime * 0.5f;
		float num = 0.3f;
		Rigidbody[] array = rigidbodies;
		foreach (Rigidbody rigidbody in array)
		{
			rigidbody.velocity = new Vector3(rigidbody.velocity.x, 0f, rigidbody.velocity.z);
			if (!force)
			{
				if (info.wallNormal != Vector3.zero)
				{
					rigidbody.AddForce(info.wallNormal * jumpForceMultiplier * 0.75f, ForceMode.VelocityChange);
					rigidbody.AddForce(Vector3.up * jumpForceMultiplier * 0.85f, ForceMode.VelocityChange);
					result = true;
				}
				else
				{
					rigidbody.AddForce(Vector3.up * jumpForceMultiplier, ForceMode.VelocityChange);
					result = false;
				}
			}
			else if (forceWallJump)
			{
				rigidbody.AddForce(info.wallNormal * num * jumpForceMultiplier * 0.75f, ForceMode.VelocityChange);
				rigidbody.AddForce(Vector3.up * num * jumpForceMultiplier * 0.85f, ForceMode.VelocityChange);
			}
			else
			{
				rigidbody.AddForce(Vector3.up * num * jumpForceMultiplier, ForceMode.VelocityChange);
			}
		}
		screenshake.AddShake(Vector3.up * 0.01f);
		return result;
	}
}
