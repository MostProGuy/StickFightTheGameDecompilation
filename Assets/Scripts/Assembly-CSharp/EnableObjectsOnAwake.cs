using UnityEngine;

public class EnableObjectsOnAwake : MonoBehaviour
{
	public GameObject[] objects;

	private void Awake()
	{
		GameObject[] array = objects;
		foreach (GameObject gameObject in array)
		{
			gameObject.SetActive(true);
		}
	}
}
