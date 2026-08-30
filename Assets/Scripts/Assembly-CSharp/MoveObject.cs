using UnityEngine;

public class MoveObject : MonoBehaviour
{
	public float speed;

	private void Start()
	{
	}

	private void Update()
	{
		base.transform.position += Time.deltaTime * speed * base.transform.forward;
	}
}
