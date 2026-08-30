using UnityEngine;

public class EnableAll : MonoBehaviour
{
	private void Awake()
	{
		EnableOnAwake[] componentsInChildren = GetComponentsInChildren<EnableOnAwake>(true);
		foreach (EnableOnAwake enableOnAwake in componentsInChildren)
		{
			enableOnAwake.gameObject.SetActive(true);
		}
	}

	private void Update()
	{
	}
}
