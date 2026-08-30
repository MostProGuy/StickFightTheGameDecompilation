public class Optional<T>
{
	private bool mIsSet;

	private T mValue;

	public Optional()
	{
		mIsSet = false;
	}

	public Optional(T value)
	{
		SetValue(value);
	}

	public bool IsSet()
	{
		return mIsSet;
	}

	public T GetValue(T fallback)
	{
		if (mIsSet)
		{
			return mValue;
		}
		return fallback;
	}

	public void SetValue(T newValue)
	{
		mValue = newValue;
		mIsSet = true;
	}
}
