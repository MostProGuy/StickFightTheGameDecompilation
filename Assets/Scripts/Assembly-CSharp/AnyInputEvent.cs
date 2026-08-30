using System.Collections.ObjectModel;
using InControl;
using UnityEngine;
using UnityEngine.Events;

public class AnyInputEvent : MonoBehaviour
{
	public UnityEvent anyInputEvent;

	private int frames;

	private void Start()
	{
	}

	private void Update()
	{
		bool flag = false;
		ReadOnlyCollection<InputDevice> devices = InputManager.Devices;
		foreach (InputDevice item in devices)
		{
			if (item.AnyButton.WasPressed)
			{
				flag = true;
			}
		}
		if (Input.anyKey)
		{
			flag = true;
		}
		if (flag && frames > 5)
		{
			anyInputEvent.Invoke();
			base.enabled = false;
		}
		frames++;
	}
}
