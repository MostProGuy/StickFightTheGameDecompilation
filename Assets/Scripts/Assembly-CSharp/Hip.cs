using UnityEngine;

public class Hip : MonoBehaviour
{
	private CharacterInformation info;

	public float cameraImportance = 1f;

	private void Start()
	{
		info = base.transform.root.GetComponent<CharacterInformation>();
	}

	private void Update()
	{
		if (info.isDead)
		{
			cameraImportance = Mathf.Lerp(cameraImportance, 0f, Time.deltaTime * 2f);
		}
		else
		{
			cameraImportance = Mathf.Lerp(cameraImportance, 1f, Time.deltaTime * 2f);
		}
	}
}
