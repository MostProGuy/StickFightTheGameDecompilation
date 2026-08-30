using UnityEngine;

public class Wobble : MonoBehaviour
{
	public float friction = 0.9f;

	public float movementMultiplier = 1f;

	[Range(-1f, 1f)]
	public float inputVelocity;

	[HideInInspector]
	public float currentVelocity;

	public float currentValue;

	public float lerpBackSpeed;

	private void Start()
	{
	}

	private void FixedUpdate()
	{
		float value = (inputVelocity - currentValue) * 0.01f * movementMultiplier;
		value = Mathf.Clamp(value, -0.1f, 0.1f);
		currentVelocity += value;
		currentVelocity *= friction;
		currentValue += currentVelocity;
		if (lerpBackSpeed > 0f)
		{
			inputVelocity = Mathf.Lerp(inputVelocity, 0f, lerpBackSpeed);
		}
	}
}
