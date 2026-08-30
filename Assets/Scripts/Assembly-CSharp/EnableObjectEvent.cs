using UnityEngine;

public class EnableObjectEvent : MonoBehaviour
{
	public GameObject obj;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void Enable()
	{
		obj.SetActive(true);
	}

	public void Disable()
	{
		obj.SetActive(false);
	}
}
