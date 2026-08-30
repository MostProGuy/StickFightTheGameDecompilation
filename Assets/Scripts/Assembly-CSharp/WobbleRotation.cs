using UnityEngine;

public class WobbleRotation : MonoBehaviour
{
	public Wobble wobble;

	public float rotationAmount = 10f;

	public bool saveStartValue;

	private float startValue;

	public float speed;

	private float usedValue;

	private void Start()
	{
		if (!wobble)
		{
			wobble = base.transform.parent.GetComponentInChildren<Wobble>();
		}
		if (saveStartValue)
		{
			startValue = base.transform.localRotation.eulerAngles.x;
		}
	}

	private void Update()
	{
		if (speed == 0f)
		{
			usedValue = wobble.currentValue;
		}
		else
		{
			usedValue = Mathf.Lerp(usedValue, wobble.currentValue, speed * Time.deltaTime);
		}
		base.transform.localRotation = Quaternion.Euler(rotationAmount * usedValue + startValue, 0f, 0f);
	}
}
