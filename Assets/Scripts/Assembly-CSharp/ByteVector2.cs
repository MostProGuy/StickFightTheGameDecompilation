using UnityEngine;

public class ByteVector2
{
	private sbyte mXValue;

	private sbyte mYValue;

	private float mFractor = 100f;

	private bool mUsingBigNumbers;

	public sbyte X
	{
		get
		{
			return mXValue;
		}
	}

	public sbyte Y
	{
		get
		{
			return mYValue;
		}
	}

	public bool UsingBigNumbers
	{
		get
		{
			return mUsingBigNumbers;
		}
	}

	public ByteVector2(Vector3 vec3)
	{
		Vector2 vector = new Vector2(vec3.y, vec3.z);
		float num = Mathf.Clamp(float.Parse(vector.x.ToString("F2")), -128f, 127f);
		float num2 = Mathf.Clamp(float.Parse(vector.y.ToString("F2")), -128f, 127f);
		mXValue = (sbyte)(num * mFractor);
		mYValue = (sbyte)(num2 * mFractor);
	}

	public ByteVector2(Vector3 vec3, bool bigNumbers)
	{
		Vector2 vector = new Vector2(vec3.y, vec3.z);
		float num = float.Parse(vector.x.ToString("F2"));
		float num2 = float.Parse(vector.y.ToString("F2"));
		mXValue = (sbyte)(num / mFractor);
		mYValue = (sbyte)(num2 / mFractor);
	}

	public ByteVector2(Vector2 vec2)
	{
		float num = Mathf.Clamp(float.Parse(vec2.x.ToString("F2")), -128f, 127f);
		float num2 = Mathf.Clamp(float.Parse(vec2.y.ToString("F2")), -128f, 127f);
		mXValue = (sbyte)(num * mFractor);
		mYValue = (sbyte)(num2 * mFractor);
	}

	public ByteVector2(sbyte xValue, sbyte yValue)
	{
		mXValue = xValue;
		mYValue = yValue;
	}

	public Vector3 ToVector3()
	{
		Vector3 zero = Vector3.zero;
		zero.y = (float)mXValue / 100f;
		zero.z = (float)mYValue / 100f;
		return zero;
	}
}
