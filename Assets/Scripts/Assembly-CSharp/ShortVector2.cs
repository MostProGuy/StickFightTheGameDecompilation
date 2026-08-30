using UnityEngine;

public class ShortVector2
{
	private short mXValue;

	private short mYValue;

	private float mFractor = 100f;

	public short X
	{
		get
		{
			return mXValue;
		}
	}

	public short Y
	{
		get
		{
			return mYValue;
		}
	}

	public ShortVector2(Vector3 vec3, bool bigNumbers = false)
	{
		if (bigNumbers)
		{
			Vector2 vector = new Vector2(vec3.y, vec3.z);
			float num = float.Parse(vector.x.ToString("F2"));
			float num2 = float.Parse(vector.y.ToString("F2"));
			mXValue = (short)(num / mFractor);
			mYValue = (short)(num2 / mFractor);
		}
		else
		{
			Vector2 vector2 = new Vector2(vec3.y, vec3.z);
			float num3 = float.Parse(vector2.x.ToString("F2"));
			float num4 = float.Parse(vector2.y.ToString("F2"));
			mXValue = (short)(num3 * mFractor);
			mYValue = (short)(num4 * mFractor);
		}
	}

	public ShortVector2(Vector2 vec2)
	{
		float num = float.Parse(vec2.x.ToString("F2"));
		float num2 = float.Parse(vec2.y.ToString("F2"));
		mXValue = (short)(num * mFractor);
		mYValue = (short)(num2 * mFractor);
	}

	public ShortVector2(short xValue, short yValue)
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

	public override string ToString()
	{
		string empty = string.Empty;
		string text = empty;
		return text + "(" + mXValue + "," + mYValue + ")  Fracotred: (" + (float)mXValue / 100f + "," + (float)mYValue / 100f + ")";
	}
}
