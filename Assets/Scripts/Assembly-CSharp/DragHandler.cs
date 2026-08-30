using UnityEngine;

public class DragHandler : MonoBehaviour
{
	[HideInInspector]
	public float startDrag;

	[HideInInspector]
	public float startAngularDrag;

	public float extraDrag;

	private Rigidbody rig;

	private bool isFallen;

	private void Start()
	{
		rig = GetComponent<Rigidbody>();
	}

	private void Update()
	{
		if (isFallen)
		{
			rig.drag = Mathf.Lerp(rig.drag, 0f, Time.unscaledDeltaTime * 5f);
			rig.angularDrag = Mathf.Lerp(rig.angularDrag, 0f, Time.unscaledDeltaTime * 5f);
		}
		else
		{
			rig.drag = Mathf.Lerp(rig.drag, startDrag + extraDrag, Time.unscaledDeltaTime * 5f);
			rig.angularDrag = Mathf.Lerp(rig.angularDrag, startAngularDrag + extraDrag, Time.unscaledDeltaTime * 5f);
		}
		if (extraDrag > 0f)
		{
			extraDrag -= Time.deltaTime * 25f;
		}
		else
		{
			extraDrag = 0f;
		}
	}

	public void SetDrag()
	{
		if (isFallen)
		{
			SetFallenDrag();
		}
		else
		{
			SetStandingDrag();
		}
	}

	public void SetFallenDrag()
	{
		isFallen = true;
	}

	public void SetStandingDrag()
	{
		isFallen = false;
	}

	public void SetFallenDragNow()
	{
		rig.drag = 0f;
		rig.angularDrag = 0f;
	}

	public void setDragNow(float drag)
	{
		rig.drag = drag;
		rig.angularDrag = drag;
	}
}
