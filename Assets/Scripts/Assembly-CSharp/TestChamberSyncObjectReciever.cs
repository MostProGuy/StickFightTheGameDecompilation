using UnityEngine;

public class TestChamberSyncObjectReciever : MonoBehaviour
{
	private Vector3 mEndPos;

	private float timeBetweenPackages;

	private float timeOfLastPackage;

	private Vector3 distanceToTravel;

	private float angledToTravel;

	private float speed;

	private float rotationSpeed;

	private Vector3 targetAngle;

	private void FixedUpdate()
	{
	}

	private void LerpLocalObject()
	{
		base.transform.position += distanceToTravel.normalized * speed * 1f * Time.deltaTime;
		Vector3 vector = Vector3.RotateTowards(base.transform.up, targetAngle, rotationSpeed * 0.01f * Time.deltaTime, 0f);
		base.transform.rotation = Quaternion.LookRotation(Vector3.Cross(Vector3.right, vector), vector);
	}

	public void AssignNewPoint(Vector3 newPos, Vector3 rot)
	{
		timeBetweenPackages = Time.time - timeOfLastPackage;
		timeOfLastPackage = Time.time;
		mEndPos = newPos;
		distanceToTravel = mEndPos - base.transform.position;
		speed = distanceToTravel.magnitude / timeBetweenPackages;
		angledToTravel = Vector3.Angle(base.transform.up, rot);
		rotationSpeed = angledToTravel / timeBetweenPackages;
		targetAngle = rot;
	}

	private void Update()
	{
		LerpLocalObject();
	}
}
