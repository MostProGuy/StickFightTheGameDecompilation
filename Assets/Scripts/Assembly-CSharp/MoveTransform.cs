using UnityEngine;

public class MoveTransform : MonoBehaviour
{
	public float amount;

	private void Awake()
	{
		base.transform.position += amount * base.transform.forward;
	}

	private void Update()
	{
	}
}
