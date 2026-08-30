using UnityEngine;

public class RigidBodyIndexHolder : MonoBehaviour
{
	private byte mIndex;

	private bool mInited;

	public byte Index
	{
		get
		{
			return mIndex;
		}
	}

	public void InitIndex(byte index)
	{
		if (mInited)
		{
			Debug.LogError("Rigidbody on object, " + base.gameObject.name + " Has already been inited!");
			return;
		}
		mIndex = index;
		mInited = true;
	}
}
