using UnityEngine;

public class EnableObject : MonoBehaviour
{
	public float delay;

	public GameObject go;

	private void Start()
	{
	}

	private void Update()
	{
		delay -= Time.deltaTime;
		if (!(delay > 0f))
		{
			go.SetActive(true);
			Object.Destroy(this);
		}
	}
}
