using UnityEngine;

public class KeepLocalPositionButStayInDimention : MonoBehaviour
{
	private Vector3 localPosition;

	private void Start()
	{
		localPosition = base.transform.localPosition;
	}

	private void LateUpdate()
	{
		Vector3 position = base.transform.parent.TransformPoint(localPosition);
		position.x = 0f;
		base.transform.position = position;
	}
}
