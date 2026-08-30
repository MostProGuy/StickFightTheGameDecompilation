using UnityEngine;

public class BlinkObject : MonoBehaviour
{
	public GameObject obj;

	public float timeOn;

	public float timeOff;

	private float counter;

	private bool isOn;

	private void Start()
	{
	}

	private void Update()
	{
		counter += Time.deltaTime;
		if (counter > timeOn && isOn)
		{
			counter = 0f;
			isOn = false;
			obj.SetActive(false);
		}
		if (counter > timeOff && !isOn)
		{
			counter = 0f;
			isOn = true;
			obj.SetActive(true);
		}
	}
}
