using UnityEngine;

public class LavaFlow : MonoBehaviour
{
	public float counter;

	private bool isOn;

	public float target;

	private Material mat;

	public float speed;

	public Transform damageCube;

	public float currentStart;

	public float percentageComplete;

	public Transform startCube;

	public Transform endCube;

	public float currentX;

	private bool hasStarted;

	public Transform startPoint;

	public bool alwaysOn;

	private AudioSource au;

	private void Start()
	{
		mat = GetComponent<Renderer>().material;
		counter = 5f;
		au = base.transform.parent.parent.parent.GetComponentInChildren<AudioSource>();
	}

	public void Sync()
	{
	}

	private void Update()
	{
		counter += Time.deltaTime;
		if (counter > 5f && (!hasStarted || !alwaysOn))
		{
			isOn = !isOn;
			counter = 0f;
			currentStart = target;
			target -= 0.5f;
			speed = 0f;
			hasStarted = true;
		}
		currentX = mat.GetTextureOffset("_MainTex").x;
		if (currentX > target)
		{
			speed += (0f - Time.deltaTime) * 0.01f + speed * Time.deltaTime * 1f;
			mat.SetTextureOffset("_MainTex", new Vector2(currentX + speed * 0.5f * Time.deltaTime, 0f));
			currentX = mat.GetTextureOffset("_MainTex").x;
			if (currentX < target)
			{
				mat.SetTextureOffset("_MainTex", new Vector2(target, 0f));
			}
			currentX = mat.GetTextureOffset("_MainTex").x;
		}
		percentageComplete = (Mathf.Clamp(Mathf.Abs(currentX), 0f, float.PositiveInfinity) - Mathf.Abs(currentStart)) * 2f;
		if (isOn)
		{
			startCube.position = startPoint.position;
			endCube.position = startPoint.position + Vector3.down * percentageComplete * 15f;
		}
		else
		{
			startCube.position = startPoint.position + Vector3.down * percentageComplete * 15f;
			endCube.position = startPoint.position + Vector3.down * 15f;
		}
		if (!hasStarted)
		{
			startCube.position = startPoint.position;
			endCube.position = startPoint.position;
		}
		startCube.transform.position = new Vector3(startCube.transform.position.x, Mathf.Clamp(startCube.transform.position.y, -100f, startPoint.position.y - 2f), startCube.transform.position.z);
		endCube.transform.position = new Vector3(endCube.transform.position.x, Mathf.Clamp(endCube.transform.position.y, -100f, startPoint.position.y - 2f), endCube.transform.position.z);
		Vector3 position = (startCube.transform.position + endCube.transform.position) * 0.5f;
		float num = Vector3.Distance(startCube.transform.position, endCube.transform.position);
		damageCube.transform.position = position;
		damageCube.transform.localScale = new Vector3(0.5f, 1f, num);
		if (num < 0.05f)
		{
			damageCube.gameObject.SetActive(false);
			if ((bool)au)
			{
				au.volume = Mathf.Lerp(au.volume, 0f, Time.deltaTime * 3f);
			}
		}
		else
		{
			damageCube.gameObject.SetActive(true);
			if ((bool)au)
			{
				au.volume = Mathf.Lerp(au.volume, 0.5f, Time.deltaTime * 3f);
			}
		}
	}
}
