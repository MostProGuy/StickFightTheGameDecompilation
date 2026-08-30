using UnityEngine;

public class DestroyObject : MonoBehaviour
{
	public GameObject obj;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void GO()
	{
		Object.Destroy(obj);
	}
}
