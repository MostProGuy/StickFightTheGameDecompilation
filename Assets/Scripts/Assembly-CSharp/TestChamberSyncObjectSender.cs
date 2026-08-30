using UnityEngine;

public class TestChamberSyncObjectSender : MonoBehaviour
{
	private Vector3 mDestination;

	private void Start()
	{
		mDestination = new Vector3(0f, 5f, 0f);
	}

	private void Update()
	{
	}

	private void FakeRandomMovement()
	{
		base.transform.position = Vector3.Lerp(base.transform.position, mDestination, Time.deltaTime * 10f);
	}

	public void AssignNewDestination()
	{
		float num = Random.Range(-2f, 2f);
		float num2 = Random.Range(-2f, 2f);
		float value = mDestination.z + num;
		float num3 = mDestination.y + num2;
		value = Mathf.Clamp(value, -20f, 20f);
		num3 = Mathf.Clamp(value, -20f, 20f);
		mDestination = new Vector3(0f, num3, value);
	}

	public Vector3 GetNewPositionPackage()
	{
		return base.transform.position;
	}
}
