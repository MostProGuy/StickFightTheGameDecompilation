using InControl;

public struct KeybindingInfo
{
	public readonly int key;

	public readonly bool mouse;

	public KeybindingInfo(int key, bool mouse = false)
	{
		this.key = key;
		this.mouse = mouse;
	}

	public KeybindingInfo(Key key)
	{
		this.key = (int)key;
		mouse = false;
	}

	public KeybindingInfo(Mouse key)
	{
		this.key = (int)key;
		mouse = true;
	}
}
