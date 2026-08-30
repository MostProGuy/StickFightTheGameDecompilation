using UnityEngine;

public class RandomVariation : MonoBehaviour
{
	public float scaleRange;

	public float positionRange;

	public float rotationRange;

	private void Start()
	{
		base.transform.localScale *= Random.Range(1f - scaleRange, 1f + scaleRange);
		base.transform.Rotate(Vector3.forward * Random.Range(0f - rotationRange, rotationRange));
		base.transform.position = new Vector3(base.transform.position.x, base.transform.position.y + Random.Range(0f - positionRange, positionRange), base.transform.position.z + Random.Range(0f - positionRange, positionRange));
	}

	private void Update()
	{
	}
}
