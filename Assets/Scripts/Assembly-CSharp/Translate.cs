using UnityEngine;

public class Translate : MonoBehaviour
{
	public Vector3 moveVector;

	private float startMultiplier = 1f;

	public AnimationCurve startCurve;

	public float secondsToStart;

	public float secondsBeforeStarting;

	public bool whenShoot;

	private float multiplier = 1f;

	private void Start()
	{
		if (secondsToStart != 0f)
		{
			startMultiplier = 0f;
		}
	}

	private void FixedUpdate()
	{
		if (secondsBeforeStarting > 0f)
		{
			secondsBeforeStarting -= Time.deltaTime;
			return;
		}
		float num = 1f;
		if (secondsToStart != 0f)
		{
			if (startMultiplier < 1f)
			{
				startMultiplier += Time.deltaTime / secondsToStart;
			}
			else
			{
				startMultiplier = 1f;
			}
			num = startCurve.Evaluate(startMultiplier);
		}
		base.transform.Translate(moveVector * Time.fixedDeltaTime * num * multiplier, Space.Self);
	}
}
