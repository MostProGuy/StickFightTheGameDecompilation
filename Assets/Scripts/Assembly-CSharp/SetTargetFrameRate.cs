using UnityEngine;

public class SetTargetFrameRate : MonoBehaviour
{
	private void Start()
	{
		int num = PlayerPrefs.GetInt("Framerate", 0);
		if (num >= 0 && num < OptionsHolder.AvailableFramerates.Length)
		{
			Application.targetFrameRate = OptionsHolder.AvailableFramerates[num];
		}
	}

	private void Update()
	{
	}
}
