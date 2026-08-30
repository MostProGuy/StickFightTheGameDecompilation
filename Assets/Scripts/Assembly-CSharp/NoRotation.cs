using UnityEngine;

public class NoRotation : MonoBehaviour
{
	private void Start()
	{
	}

	private void LateUpdate()
	{
		base.transform.rotation = Quaternion.identity;
	}
}
