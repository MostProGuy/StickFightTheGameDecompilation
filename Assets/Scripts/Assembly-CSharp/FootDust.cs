using UnityEngine;

public class FootDust : MonoBehaviour
{
	private ParticleSystem particle;

	public float shake;

	private ScreenshakeHandler screenShake;

	private void Start()
	{
		particle = base.transform.root.GetComponentInChildren<FootParticle>().GetComponent<ParticleSystem>();
		screenShake = ScreenshakeHandler.Instance;
	}

	private void Update()
	{
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (!(Vector3.Angle(Vector3.up, collision.contacts[0].normal) > 70f) && !(collision.transform.root == base.transform.root) && !collision.transform.root.GetComponent<Controller>())
		{
			particle.transform.position = collision.contacts[0].point;
			particle.Play();
		}
	}
}
