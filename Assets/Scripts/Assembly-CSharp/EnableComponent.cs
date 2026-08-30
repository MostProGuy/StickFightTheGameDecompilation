using UnityEngine;

public class EnableComponent : MonoBehaviour
{
	public MonoBehaviour component;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void Enable()
	{
		component.enabled = true;
	}

	public void Disable()
	{
		component.enabled = false;
	}
}
