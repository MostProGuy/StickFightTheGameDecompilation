using UnityEngine;
using UnityEngine.Events;

public class CollisionEvent : MonoBehaviour
{
	public enum AcceptedCollisions
	{
		Any = 0,
		Player = 1,
		NonPlayer = 2
	}

	public UnityEvent collisionEvent;

	public float mustLifeForSecondsBeforeCollision;

	public AcceptedCollisions acceptedCollisions;

	private bool done;

	private void Start()
	{
	}

	private void Update()
	{
		mustLifeForSecondsBeforeCollision -= Time.deltaTime;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (!done && !(mustLifeForSecondsBeforeCollision > 0f) && (acceptedCollisions != AcceptedCollisions.NonPlayer || !collision.transform.root.GetComponent<CharacterInformation>()) && (acceptedCollisions != AcceptedCollisions.Player || (bool)collision.transform.root.GetComponent<CharacterInformation>()))
		{
			done = true;
			collisionEvent.Invoke();
		}
	}
}
