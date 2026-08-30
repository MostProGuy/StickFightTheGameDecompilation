using UnityEngine;

public class SetChildrenFree : MonoBehaviour
{
	public bool allowForParent;

	private void Start()
	{
		if (base.transform.parent == null || allowForParent)
		{
			SetFree();
		}
	}

	public void SetFree()
	{
		for (int num = base.transform.childCount - 1; num >= 0; num--)
		{
			base.transform.GetChild(num).SetParent(null);
		}
		Object.Destroy(base.gameObject);
	}
}
