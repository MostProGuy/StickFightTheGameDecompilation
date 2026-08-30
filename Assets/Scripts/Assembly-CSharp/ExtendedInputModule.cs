using UnityEngine.EventSystems;

public class ExtendedInputModule : StandaloneInputModule
{
	private static ExtendedInputModule _instance;

	public static PointerEventData GetPointerEventData(int pointerId = -1)
	{
		PointerEventData data;
		_instance.GetPointerData(pointerId, out data, true);
		return data;
	}

	protected override void Awake()
	{
		base.Awake();
		_instance = this;
	}
}
