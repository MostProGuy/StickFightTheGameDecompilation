using UnityEngine;

public class ByteFloat
{
	private sbyte mValue;

	private float mFractor = 10f;

	public sbyte Value
	{
		get
		{
			return mValue;
		}
	}

	public ByteFloat(float value)
	{
		float num = Mathf.Clamp(float.Parse(value.ToString("F1")), -128f, 127f);
		mValue = (sbyte)(num * mFractor);
	}
}
