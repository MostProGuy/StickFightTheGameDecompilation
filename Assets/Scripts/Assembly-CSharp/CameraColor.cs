using UnityEngine;

public class CameraColor : MonoBehaviour
{
	public int currentColor;

	public Color[] colors;

	private Camera cam;

	private void Start()
	{
		cam = GetComponent<Camera>();
	}

	private void Update()
	{
		cam.backgroundColor = Color.Lerp(cam.backgroundColor, colors[currentColor], Time.deltaTime * 5f);
	}
}
