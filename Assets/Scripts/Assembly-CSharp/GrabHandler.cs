using System.Collections;
using UnityEngine;

public class GrabHandler : MonoBehaviour
{
	public bool isGrabbing;

	public bool isHoldingSomething;

	public bool isHoldingSomethingAnchored;

	public bool isCarryingSomething;

	public Grabber[] grabbers;

	private AimTarget aim;

	public float animationLength = 0.4f;

	public AnimationCurve animationCurve;

	private Rigidbody torso;

	public Rigidbody heldRig;

	private float counter;

	private CharacterInformation info;

	private CharacterInformation ourInfo;

	public bool isHoldingLeft;

	public bool isHoldingRight;

	public Collider[] collidersToDisable;

	public float grabStrength = 1f;

	private ScreenshakeHandler screenshake;

	private Controller controller;

	private AudioSource au;

	public AudioClip[] grabClips;

	private void Start()
	{
		grabbers = GetComponentsInChildren<Grabber>();
		aim = GetComponentInChildren<AimTarget>();
		torso = GetComponentInChildren<Torso>().GetComponent<Rigidbody>();
		screenshake = ScreenshakeHandler.Instance;
		controller = GetComponent<Controller>();
		ourInfo = GetComponent<CharacterInformation>();
		au = GetComponentInChildren<AudioSource>();
	}

	public void StartGrab(Rigidbody rig)
	{
		for (int i = 0; i < collidersToDisable.Length; i++)
		{
			collidersToDisable[i].enabled = false;
		}
		if (rig == null || (bool)rig.transform.root.GetComponent<Controller>())
		{
			isHoldingSomethingAnchored = true;
		}
		else
		{
			info = rig.transform.root.GetComponent<CharacterInformation>();
			if (!info)
			{
				isHoldingSomethingAnchored = true;
			}
		}
		isHoldingSomething = true;
		if (grabClips.Length > 0)
		{
			au.PlayOneShot(grabClips[Random.Range(0, grabClips.Length)], 0.4f);
		}
	}

	public void EndGrab()
	{
		if (isGrabbing || isHoldingSomething || isHoldingSomethingAnchored || isCarryingSomething)
		{
			for (int i = 0; i < collidersToDisable.Length; i++)
			{
				collidersToDisable[i].enabled = true;
			}
			if (isHoldingSomethingAnchored)
			{
				ourInfo.sinceWall = 0f;
			}
			isGrabbing = false;
			isHoldingSomething = false;
			isHoldingSomethingAnchored = false;
			isCarryingSomething = false;
			heldRig = null;
			if ((bool)info)
			{
				info.isBeingGrabbed = false;
			}
			info = null;
		}
	}

	private void FixedUpdate()
	{
		counter += Time.fixedDeltaTime;
		grabStrength += Time.fixedDeltaTime * 0.3f;
		grabStrength = Mathf.Clamp(grabStrength, -0.5f, 1f);
		if (grabStrength < 0f)
		{
			EndGrab();
		}
		if (isCarryingSomething && !info)
		{
			if (heldRig.position.y > torso.position.y - 0.3f)
			{
				heldRig.AddForce((new Vector3(heldRig.position.x, torso.position.y + 1.7f, heldRig.position.z) - heldRig.position) * 30000f, ForceMode.Force);
			}
			heldRig.velocity *= 0.85f;
		}
		if (isHoldingSomething)
		{
			counter = 0f;
		}
		else
		{
			counter += Time.deltaTime;
		}
	}

	public void StartNewGrab()
	{
		if (counter > 0.5f)
		{
			isGrabbing = true;
		}
	}

	public void StopGrabbing()
	{
		EndGrab();
	}

	private IEnumerator ThrowObject(Rigidbody target)
	{
		float t = 0f;
		float m = 2f;
		RigidbodySettings rigSettings = null;
		Vector3 dir = aim.transform.forward;
		if ((bool)info)
		{
			rigSettings = info.GetComponent<RigidbodySettings>();
			m = 2f;
			info.sinceFallen = -0.5f;
			for (int i = 0; i < rigSettings.dragHandlers.Length; i++)
			{
				rigSettings.dragHandlers[i].setDragNow(3f);
			}
		}
		while (t < 1f)
		{
			t += Time.deltaTime * m / animationLength;
			if ((bool)info)
			{
				info.mainRig.AddForce(dir * animationCurve.Evaluate(t) * 500f, ForceMode.Acceleration);
				target.AddForce(dir * animationCurve.Evaluate(t) * Time.deltaTime * 700f, ForceMode.Acceleration);
			}
			else
			{
				target.AddForce(dir * animationCurve.Evaluate(t) * Time.deltaTime * 3000000f, ForceMode.Force);
			}
			yield return null;
		}
		if ((bool)info)
		{
			info.mainRig.AddForce(dir * 40f, ForceMode.VelocityChange);
			target.AddForce(dir * 70f, ForceMode.VelocityChange);
			for (int j = 0; j < rigSettings.dragHandlers.Length; j++)
			{
				rigSettings.dragHandlers[j].setDragNow(2f);
			}
			info.sinceThrown = 0f;
		}
		else
		{
			target.AddForce(dir * 1000f, ForceMode.Impulse);
			target.gameObject.AddComponent<IsBeingThrown>().owner = controller;
		}
		screenshake.AddShake(0.5f * aim.transform.forward);
		for (int k = 0; k < grabbers.Length; k++)
		{
			grabbers[k].LetItGo();
		}
		EndGrab();
	}

	public void Throw()
	{
		if ((bool)heldRig)
		{
			StartCoroutine(ThrowObject(heldRig));
		}
	}
}
