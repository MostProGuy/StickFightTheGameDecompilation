using UnityEngine;

public class DynamicCam : MonoBehaviour
{
	private Wobble wobble;

	private Camera cam;

	private Rigidbody rig;

	public float speed;

	public AnimationCurve cameraAllowedMovement;

	private void Start()
	{
		cam = GetComponentInChildren<Camera>();
		wobble = GetComponent<Wobble>();
		rig = GetComponent<Rigidbody>();
	}

	private void Update()
	{
		if (Object.FindObjectsOfType<Hip>().Length < 2)
		{
			return;
		}
		float num = 0f;
		Hip[] array = Object.FindObjectsOfType<Hip>();
		foreach (Hip hip in array)
		{
			Hip[] array2 = Object.FindObjectsOfType<Hip>();
			foreach (Hip hip2 in array2)
			{
				float num2 = Vector3.Distance(hip.transform.position * hip.cameraImportance, hip2.transform.position * hip2.cameraImportance);
				if (num < num2)
				{
					num = num2;
				}
			}
		}
		if (GameManager.inFight)
		{
			cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, Mathf.Clamp(5f + num / 5f, 0f, 10f), Time.deltaTime * 2f);
		}
		else
		{
			cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, 10f, Time.deltaTime * 2f);
		}
	}

	private void FixedUpdate()
	{
		float num = cameraAllowedMovement.Evaluate(cam.orthographicSize);
		Vector3 zero = Vector3.zero;
		float num2 = 0f;
		Hip[] array = Object.FindObjectsOfType<Hip>();
		foreach (Hip hip in array)
		{
			zero += (hip.transform.position - Vector3.up) * num * hip.cameraImportance;
			num2 += hip.cameraImportance;
		}
		zero /= num2;
		zero.x = -5f;
		Vector3 position = base.transform.position;
		position.x = -5f;
		if (GameManager.inFight)
		{
			base.transform.position = Vector3.Lerp(position, zero, Time.deltaTime * 2f);
		}
		else
		{
			base.transform.position = Vector3.Lerp(position, Vector3.right * -5f, Time.deltaTime * 2f);
		}
	}
}
