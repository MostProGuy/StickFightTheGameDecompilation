using UnityEngine;

public class SpawnStickyObject : MonoBehaviour
{
	public GameObject stickyObject;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void Hit(Vector3 position, Quaternion rotation, Rigidbody r, Controller c)
	{
		Object.Instantiate(stickyObject, position, rotation).GetComponent<StickyObject>().Stick(r, rotation, c);
	}
}
