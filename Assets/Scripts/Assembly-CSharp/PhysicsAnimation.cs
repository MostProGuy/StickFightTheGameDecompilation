using System.Collections;
using UnityEngine;

public class PhysicsAnimation : MonoBehaviour
{
	public Vector3 worldDirection;

	public Vector3 localDirection;

	public Transform targetDirection;

	public bool aiTargetDirection;

	public AnimationCurve curve;

	public float duration;

	public float force;

	public bool keepGoing;

	public bool playOnAwake;

	private bool done;

	public float cap = 1f;

	private Rigidbody rig;

	private void Start()
	{
		rig = GetComponent<Rigidbody>();
		if (playOnAwake)
		{
			Play();
		}
	}

	private void Update()
	{
		if (done && keepGoing)
		{
			rig.AddForce(GetDirection() * curve.Evaluate(1f) * force * Time.deltaTime, ForceMode.Acceleration);
		}
	}

	public void Stop()
	{
		done = false;
	}

	public void Play()
	{
		done = false;
		StartCoroutine(PlayAnimation());
	}

	private IEnumerator PlayAnimation()
	{
		float f = 0f;
		while (f < 1f)
		{
			f += Time.deltaTime / duration;
			float curveValue = curve.Evaluate(f);
			rig.AddForce(GetDirection() * curveValue * force * Time.deltaTime, ForceMode.Acceleration);
			yield return null;
		}
		done = true;
	}

	private Vector3 GetDirection()
	{
		Vector3 result = Vector3.zero;
		result += worldDirection;
		result += base.transform.TransformDirection(localDirection);
		if ((bool)targetDirection)
		{
			Vector3 vector = targetDirection.position - base.transform.position;
			if (vector.magnitude > 1f)
			{
				vector = vector.normalized;
			}
			result += vector;
		}
		if (result.magnitude > cap)
		{
			result = result.normalized * cap;
		}
		return result;
	}
}
