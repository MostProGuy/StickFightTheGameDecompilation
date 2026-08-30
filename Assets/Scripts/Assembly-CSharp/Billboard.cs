using UnityEngine;

public class Billboard : MonoBehaviour
{
	private void Start()
	{
	}

	private void LateUpdate()
	{
		base.transform.rotation = Quaternion.LookRotation(-Vector3.right);
	}
}
