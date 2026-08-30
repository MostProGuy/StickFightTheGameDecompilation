using UnityEngine;

public class StickyObject : MonoBehaviour
{
	public Transform stickObject;

	public Rigidbody hitR;

	public bool ignoreRotation;

	public float rotationSpeed = 25f;

	private bool done;

	public float offset = 1f;

	public Controller controller;

	private void Start()
	{
	}

	private void OnDestroy()
	{
		if ((bool)stickObject)
		{
			Object.Destroy(stickObject.gameObject);
		}
	}

	private void Update()
	{
		if ((bool)stickObject)
		{
			base.transform.position = stickObject.position + Vector3.right * offset;
			if (!ignoreRotation)
			{
				base.transform.rotation = Quaternion.Lerp(base.transform.rotation, stickObject.rotation, Time.deltaTime * rotationSpeed);
			}
			if (!stickObject.gameObject.activeInHierarchy)
			{
				Object.Destroy(base.gameObject);
			}
		}
	}

	public void Stick(Rigidbody hitRig, Quaternion rot, Controller c)
	{
		if (!done)
		{
			done = true;
			stickObject = new GameObject().transform;
			stickObject.position = base.transform.position;
			stickObject.rotation = rot;
			if ((bool)hitRig)
			{
				stickObject.SetParent(hitRig.transform, true);
			}
			if ((bool)hitRig)
			{
				hitR = hitRig;
			}
			if ((bool)c)
			{
				controller = c;
			}
			TargetHolder component = GetComponent<TargetHolder>();
			if ((bool)component && (bool)controller && (bool)hitR)
			{
				component.Set(hitR, controller);
			}
		}
	}
}
