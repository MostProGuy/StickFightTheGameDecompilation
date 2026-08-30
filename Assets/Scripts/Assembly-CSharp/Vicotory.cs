using UnityEngine;

public class Vicotory : MonoBehaviour
{
	public string[] standard;

	public string[] snakes;

	public string[] fire;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public string GetRandomWinText()
	{
		return standard[Random.Range(0, standard.Length)];
	}
}
