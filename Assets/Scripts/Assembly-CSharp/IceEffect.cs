using UnityEngine;

public class IceEffect : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
	}

	public void IceSpecial(Rigidbody rig)
	{
		if ((bool)rig)
		{
			DragHandler component = rig.GetComponent<DragHandler>();
			if ((bool)component)
			{
				component.extraDrag += 80f;
			}
		}
		else
		{
			Debug.LogWarning("IceSpecial on invalid rig");
		}
	}
}
