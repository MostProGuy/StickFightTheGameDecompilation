using UnityEngine;

public class RigidbodySettings : MonoBehaviour
{
	public float drag;

	public float angularDrag;

	public float maxAngularVelocity = 1000f;

	public float weightMultiplier = 1f;

	public DragHandler[] dragHandlers;

	private CharacterInformation info;

	private bool fallen;

	private void Start()
	{
		info = GetComponent<CharacterInformation>();
		Rigidbody[] componentsInChildren = GetComponentsInChildren<Rigidbody>();
		foreach (Rigidbody rigidbody in componentsInChildren)
		{
			if (!(rigidbody.transform.parent.tag == "IgnoreRigidbody"))
			{
				if (rigidbody.drag == 0f && rigidbody.transform.tag != "IgnoreDrag")
				{
					rigidbody.drag = drag;
				}
				rigidbody.mass *= weightMultiplier;
				rigidbody.maxAngularVelocity = maxAngularVelocity;
				if (rigidbody.transform.tag != "IgnoreDrag")
				{
					DragHandler dragHandler = rigidbody.gameObject.AddComponent<DragHandler>();
					dragHandler.startDrag = rigidbody.drag;
					dragHandler.startAngularDrag = rigidbody.angularDrag;
				}
			}
		}
		dragHandlers = GetComponentsInChildren<DragHandler>();
	}

	private void Update()
	{
		if (info.sinceFallen < 0f && !fallen)
		{
			Fall();
		}
		if (info.sinceFallen > 0f && fallen)
		{
			GetUp();
		}
	}

	private void Fall()
	{
		fallen = true;
		DragHandler[] array = dragHandlers;
		foreach (DragHandler dragHandler in array)
		{
			dragHandler.SetFallenDrag();
		}
	}

	private void GetUp()
	{
		fallen = false;
		DragHandler[] array = dragHandlers;
		foreach (DragHandler dragHandler in array)
		{
			dragHandler.SetStandingDrag();
		}
	}
}
