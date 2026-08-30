using UnityEngine;

public class GoOnlineWhenTouched : MonoBehaviour
{
	private bool mCalled;

	public void OnCollisionEnter(Collision collision)
	{
		if ((bool)collision.transform.GetComponentInParent<Controller>())
		{
			GoOnline();
		}
	}

	private void GoOnline()
	{
		if (!mCalled)
		{
			mCalled = true;
			MatchmakingHandler.Instance.JoinRandomServer();
		}
	}

	public void ConnectionFailed()
	{
		mCalled = false;
	}
}
