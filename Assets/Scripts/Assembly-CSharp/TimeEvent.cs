using UnityEngine;
using UnityEngine.Events;

public class TimeEvent : MonoBehaviour
{
	public float time = 5f;

	public UnityEvent timeEvent;

	public bool repeat;

	private float counter;

	public float disableAfter = float.PositiveInfinity;

	public bool canOnlyBeCalledOnce;

	private bool called;

	private void Start()
	{
	}

	private void Update()
	{
		float num = Mathf.Clamp(Time.deltaTime, 0f, 0.02f);
		counter += num;
		disableAfter -= num;
		if (!(disableAfter < 0f) && counter > time)
		{
			timeEvent.Invoke();
			called = true;
			if (repeat)
			{
				counter = 0f;
			}
			else
			{
				base.enabled = false;
			}
		}
	}

	public void Go()
	{
		if (!canOnlyBeCalledOnce || !called)
		{
			timeEvent.Invoke();
			called = true;
			base.enabled = false;
		}
	}

	public void Stop()
	{
		base.enabled = false;
	}

	public void StopAndHide()
	{
		base.transform.gameObject.SetActive(false);
		base.enabled = false;
	}
}
