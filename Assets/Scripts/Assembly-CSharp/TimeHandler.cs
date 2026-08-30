using UnityEngine;

public class TimeHandler : MonoBehaviour
{
	public static float managerTime = 1f;

	public static float pauseTime = 1f;

	private bool rigsAreInterpolating;

	private void Start()
	{
	}

	private void Update()
	{
		if (pauseTime != 1f)
		{
			Time.timeScale = pauseTime;
		}
		else
		{
			Time.timeScale = managerTime;
		}
		if (Time.timeScale != 1f && !rigsAreInterpolating)
		{
			Rigidbody[] array = Object.FindObjectsOfType<Rigidbody>();
			foreach (Rigidbody rigidbody in array)
			{
				rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
			}
			rigsAreInterpolating = true;
			Debug.Log("Made all rigidbodies interpolate");
		}
		if (Time.timeScale == 1f && rigsAreInterpolating)
		{
			Rigidbody[] array2 = Object.FindObjectsOfType<Rigidbody>();
			foreach (Rigidbody rigidbody2 in array2)
			{
				rigidbody2.interpolation = RigidbodyInterpolation.None;
			}
			Time.timeScale = 1f;
			rigsAreInterpolating = false;
			Debug.Log("Made all rigidbodies stop interpolating");
		}
	}
}
