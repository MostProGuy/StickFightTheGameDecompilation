using UnityEngine;

public class IsBeingThrown : MonoBehaviour
{
	public Controller owner;

	private Rigidbody rig;

	private ScreenshakeHandler screenshake;

	private bool done;

	private void Start()
	{
		rig = GetComponent<Rigidbody>();
		screenshake = ScreenshakeHandler.Instance;
	}

	private void Update()
	{
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.transform.root == collision.transform || collision.gameObject.layer == 26 || collision.gameObject.layer == 27 || done || collision.transform.root.GetComponent<Controller>() == owner)
		{
			return;
		}
		done = true;
		if ((bool)collision.rigidbody)
		{
			BodyPart component = collision.gameObject.GetComponent<BodyPart>();
			if ((bool)component)
			{
				component.TakeDamageWithParticle(36f, base.transform.position, rig.velocity, owner);
				component.GetComponent<Rigidbody>().AddForce(rig.velocity.normalized * 3000f, ForceMode.Impulse);
				component.transform.root.GetComponent<CharacterInformation>().sinceFallen = -0.5f;
			}
		}
		screenshake.AddShake(rig.velocity.normalized * 0.4f);
		Object.Destroy(this);
	}
}
