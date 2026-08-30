using UnityEngine;

public class Spin : MonoBehaviour
{
	public Vector3 spinVector;

	private float startMultiplier = 1f;

	public AnimationCurve startCurve;

	public float secondsToStart;

	public float secondsBeforeStarting;

	public bool whenShoot;

	private float multiplier = 1f;

	private Fighting fighting;

	private Weapon weapon;

	public bool ignoreCharacterStuff;

	private void Start()
	{
		if (!ignoreCharacterStuff)
		{
			fighting = GetComponentInParent<Fighting>();
		}
		if (!ignoreCharacterStuff)
		{
			weapon = GetComponentInParent<Weapon>();
		}
		if (secondsToStart != 0f)
		{
			startMultiplier = 0f;
		}
	}

	private void Update()
	{
		float num = Mathf.Clamp(Time.deltaTime, 0f, 0.02f);
		if (secondsBeforeStarting > 0f)
		{
			secondsBeforeStarting -= num;
			return;
		}
		if ((bool)fighting)
		{
			if (whenShoot)
			{
				if (fighting.counter < 0.2f || weapon.isActive)
				{
					multiplier = Mathf.Lerp(multiplier, 1f, num * 5f);
				}
				else
				{
					multiplier = Mathf.Lerp(multiplier, 0f, num * 5f);
				}
			}
			else
			{
				multiplier = 0f;
			}
		}
		float num2 = 1f;
		if (secondsToStart != 0f)
		{
			if (startMultiplier < 1f)
			{
				startMultiplier += num / secondsToStart;
			}
			else
			{
				startMultiplier = 1f;
			}
			num2 = startCurve.Evaluate(startMultiplier);
		}
		base.transform.Rotate(spinVector * num * num2 * multiplier, Space.Self);
	}
}
