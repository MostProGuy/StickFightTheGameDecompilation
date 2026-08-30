using UnityEngine;

public class MovePlayers : MonoBehaviour
{
	private Vector3 lastPosition;

	private void Start()
	{
		lastPosition = base.transform.position;
	}

	private void FixedUpdate()
	{
		Controller[] array = Object.FindObjectsOfType<Controller>();
		foreach (Controller controller in array)
		{
			if (GameManager.inFight)
			{
				controller.transform.position += base.transform.position - lastPosition;
			}
		}
		lastPosition = base.transform.position;
	}
}
