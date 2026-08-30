using UnityEngine;

public class Gravity : MonoBehaviour
{
	public float amount;

	public float force;

	private void Start()
	{
	}

	private void Update()
	{
		force += Time.deltaTime * amount;
		base.transform.position += Vector3.down * force * Time.deltaTime;
	}
}
