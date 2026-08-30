using UnityEngine;

public class AddShake : MonoBehaviour
{
	private ScreenshakeHandler screenShakeHandler;

	public Vector3 worldDirection;

	public Vector3 localDirection;

	public float directionToCamera;

	public float multiplier = 1f;

	public bool auto;

	public float secondsRequired;

	private void Start()
	{
		screenShakeHandler = ScreenshakeHandler.Instance;
		if (auto)
		{
			Shake();
		}
	}

	private void Update()
	{
		secondsRequired -= Time.deltaTime;
	}

	private Vector3 GetDirection()
	{
		Vector3 result = Vector3.zero;
		result += worldDirection;
		result += base.transform.TransformDirection(localDirection);
		result += directionToCamera * (Camera.main.transform.position - base.transform.position).normalized;
		if (result.magnitude > 1f)
		{
			result = result.normalized;
		}
		return result;
	}

	public void Shake()
	{
		if (!(secondsRequired > 0f))
		{
			screenShakeHandler.AddShake(GetDirection() * multiplier);
		}
	}
}
