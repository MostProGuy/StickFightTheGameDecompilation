using UnityEngine;

public class TargetHolder : MonoBehaviour
{
	public Rigidbody rig;

	public BodyPart part;

	public Controller controller;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void Set(Rigidbody r, Controller c)
	{
		rig = r;
		part = r.GetComponent<BodyPart>();
		controller = c;
	}
}
