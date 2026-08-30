using UnityEngine;

public class Maps : MonoBehaviour
{
	public static Vector3 myGlobalPosition;

	private void Start()
	{
	}

	private void Update()
	{
		myGlobalPosition = base.transform.position;
	}
}
