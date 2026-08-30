using System.Collections;
using UnityEngine;

public class TestChamberSyncHandler : MonoBehaviour
{
	[SerializeField]
	private uint m_SimulatedMS;

	[SerializeField]
	private uint m_MessagesPerSecond = 2u;

	private float mSendRate;

	private float mCurrentSendTick;

	private Vector3 mLocalPositionDiff;

	private TestChamberSyncObjectReciever mRecieverObject;

	private TestChamberSyncObjectSender mSenderObject;

	private void Start()
	{
		mRecieverObject = Object.FindObjectOfType<TestChamberSyncObjectReciever>();
		mSenderObject = Object.FindObjectOfType<TestChamberSyncObjectSender>();
		mLocalPositionDiff = mRecieverObject.transform.position - mSenderObject.transform.position;
	}

	private void LateUpdate()
	{
		mSendRate = 1f / (float)m_MessagesPerSecond;
		TickCurrentSendTimer();
	}

	private void TickCurrentSendTimer()
	{
		mCurrentSendTick += Time.deltaTime;
		if (mCurrentSendTick >= mSendRate)
		{
			SendNewPositionPackage();
			ResetCurrentSendTimer();
		}
	}

	private void ResetCurrentSendTimer()
	{
		mCurrentSendTick = 0f;
	}

	private void SendNewPositionPackage()
	{
		Vector3 newPositionPackage = mSenderObject.GetNewPositionPackage();
		StartCoroutine(SendPackageWithMS(newPositionPackage, mSenderObject.transform.up, m_SimulatedMS));
	}

	private IEnumerator SendPackageWithMS(Vector3 pos, Vector3 rot, uint milliSeconds)
	{
		yield return new WaitForSecondsRealtime((float)milliSeconds / 1000f);
		pos += mLocalPositionDiff;
		mRecieverObject.AssignNewPoint(pos, rot);
	}
}
