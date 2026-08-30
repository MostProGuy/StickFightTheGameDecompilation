using LevelEditor;
using UnityEngine;

public class AspectFix : MonoBehaviour
{
	private Camera cam;

	private float mapSize = 10f;

	private float currentMapSize = 20f;

	private float mapSizeVelocity;

	private float drag = 0.5f;

	private float spring = 0.2f;

	public bool scale;

	private void Start()
	{
		cam = GetComponent<Camera>();
	}

	public void SetMapSize(float aMapSize)
	{
		mapSize = aMapSize;
	}

	public void UpdateSize()
	{
		float num = 1f;
		if ((bool)MapSizeHandler.Instance)
		{
			if (WorkshopStateHandler.IsPlayTestingMode || scale)
			{
				num = MapSizeHandler.Instance.mapSize / 10f;
			}
		}
		else
		{
			mapSizeVelocity += (mapSize - currentMapSize) * spring;
			mapSizeVelocity *= drag;
			currentMapSize += mapSizeVelocity;
			num = currentMapSize / 10f;
		}
		float num2 = Mathf.Max((float)Screen.width / (float)Screen.height, 1.78f);
		cam.orthographicSize = num2 * 10f / cam.aspect * num;
	}

	private void FixedUpdate()
	{
		UpdateSize();
	}
}
