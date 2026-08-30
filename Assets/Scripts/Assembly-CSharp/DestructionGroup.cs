using UnityEngine;

public class DestructionGroup : MonoBehaviour
{
	private void Awake()
	{
		Transform[] componentsInChildren = base.transform.GetComponentsInChildren<Transform>();
		foreach (Transform transform in componentsInChildren)
		{
			transform.SetParent(base.transform);
		}
	}
}
