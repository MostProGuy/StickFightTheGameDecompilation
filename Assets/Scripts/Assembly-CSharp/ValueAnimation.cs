using UnityEngine;

public class ValueAnimation : MonoBehaviour
{
	private Wobble wobble;

	public ValueCurve[] curves;

	public int currentAnimationID;

	private float currentAnimationPosition;

	private float wait;

	private void Start()
	{
		wobble = GetComponent<Wobble>();
	}

	private void Update()
	{
		wait -= Time.deltaTime;
		if (!(wait > 0f))
		{
			currentAnimationPosition += Time.deltaTime / curves[currentAnimationID].duration;
			if (currentAnimationPosition > 1f)
			{
				currentAnimationPosition = 0f;
				wait = curves[currentAnimationID].waitDuration;
			}
			float inputVelocity = curves[currentAnimationID].animation.Evaluate(currentAnimationPosition);
			wobble.inputVelocity = inputVelocity;
		}
	}
}
