using UnityEngine;
using UnityEngine.Events;

public class OutOfBoundsEvent : MonoBehaviour
{
	public UnityEvent outOfBoundsEvent;

	public Vector2 min;

	public Vector2 max;

	private bool hasCalledEvent;

	private void Start()
	{
	}

	private void Update()
	{
		if (!hasCalledEvent && (base.transform.position.z > max.x || base.transform.position.y > max.y || base.transform.position.z < min.x || base.transform.position.y < min.y))
		{
			Go();
		}
	}

	public void Go()
	{
		if (!hasCalledEvent)
		{
			outOfBoundsEvent.Invoke();
			hasCalledEvent = true;
		}
	}
}
