using UnityEngine;

public class ScreenshakeHandler : MonoBehaviour
{
	private Rigidbody rig;

	private static ScreenshakeHandler _instance;

	public static ScreenshakeHandler Instance
	{
		get
		{
			return _instance;
		}
	}

	private void Awake()
	{
		if (_instance != null)
		{
			Object.Destroy(base.gameObject);
		}
		else
		{
			_instance = this;
		}
	}

	private void Start()
	{
		rig = GetComponent<Rigidbody>();
		rig.maxAngularVelocity = 10000f;
	}

	private void FixedUpdate()
	{
		if (Time.timeScale < 1f)
		{
			rig.velocity *= 0.95f;
			rig.angularVelocity *= 0.95f;
		}
		float num = Vector3.Angle(base.transform.forward, Vector3.right);
		Vector3 vector = Vector3.Cross(base.transform.forward, Vector3.right);
		rig.AddTorque(vector * num * 100000f * Time.fixedDeltaTime, ForceMode.Acceleration);
	}

	private void Update()
	{
	}

	public void AddShake(Vector3 direction)
	{
		direction *= (float)OptionsHolder.shake * 0.01f;
		direction *= 5f;
		float magnitude = direction.magnitude;
		direction.x = 0f;
		direction = direction.normalized * magnitude;
		float num = 1f;
		if (rig.velocity.magnitude > 0f && rig.velocity.magnitude * 10f > 1f)
		{
			num = 1f / (rig.velocity.magnitude * 10f);
		}
		rig.AddForce(direction * num, ForceMode.VelocityChange);
	}
}
