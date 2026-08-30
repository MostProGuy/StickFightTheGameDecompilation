using UnityEngine;

public class StayInDimension : MonoBehaviour
{
	public enum Type
	{
		position = 0,
		rotation = 1,
		positionAndRotation = 2
	}

	public Type type;

	private void Start()
	{
	}

	private void LateUpdate()
	{
		if (type == Type.position || type == Type.positionAndRotation)
		{
			base.transform.position = new Vector3(0f, base.transform.position.y, base.transform.position.z);
		}
		if (type == Type.rotation || type == Type.positionAndRotation)
		{
			base.transform.rotation = Quaternion.LookRotation(new Vector3(0f, base.transform.parent.forward.y, base.transform.parent.forward.z));
		}
	}
}
