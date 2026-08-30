using UnityEngine;

public class ScaleRandomGameObjects : MonoBehaviour
{
	private float counter;

	private GameManager gameman;

	private void Start()
	{
		gameman = GameManager.Instance;
	}

	private void Update()
	{
		counter += Time.deltaTime;
		if (gameman.matchTime < 2f)
		{
			counter = 0f;
		}
		if (counter > 0.3f && (bool)gameman.currentMapInfo)
		{
			Transform[] componentsInChildren = gameman.currentMapInfo.GetComponentsInChildren<Transform>();
			counter = 0f;
			Transform transform = componentsInChildren[Random.Range(0, componentsInChildren.Length)];
			counter = 0f;
			transform.gameObject.AddComponent<Spin>().spinVector.x = Random.Range(-10, 10);
			transform.gameObject.AddComponent<Scaler>().speed = Random.Range(-0.3f, 0.3f);
		}
	}
}
