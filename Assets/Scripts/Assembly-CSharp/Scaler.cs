using UnityEngine;

public class Scaler : MonoBehaviour
{
	public float speed;

	private void Start()
	{
	}

	private void Update()
	{
		if (base.transform.localScale + Time.deltaTime * speed * Vector3.one != Vector3.zero)
		{
			base.transform.localScale += Time.deltaTime * speed * Vector3.one;
		}
	}
}
