using UnityEngine;

public class CollisionParticle : MonoBehaviour
{
	private ParticleSystem part;

	private float counter;

	private void Start()
	{
		part = GetComponentInChildren<ParticleSystem>();
	}

	private void Update()
	{
		counter += Time.deltaTime;
	}

	private void OnCollisionStay(Collision collision)
	{
		Debug.Log("HELLO");
		if (counter > 0.1f)
		{
			part.transform.position = collision.contacts[0].point;
			part.Emit(15);
			counter = 0f;
		}
	}
}
