using System.Collections;
using UnityEngine;

public class LaserRaycast : MonoBehaviour
{
	private Vector3 startPoint;

	private Vector3 endPoint;

	public LayerMask mask;

	public float countDown = 0.8f;

	public ParticleSystem[] hitParts;

	public int[] particleNumbers;

	public Explosion explosion;

	public float damage;

	public float force;

	public float shake = 1f;

	public Controller damager;

	public Transform startTransform;

	public Transform endTransform;

	private bool done;

	private void Start()
	{
		startPoint = startTransform.position;
		endPoint = endTransform.position;
		damager = GetComponent<DamagerHolder>().damager;
	}

	private void Update()
	{
		if (!done)
		{
			countDown -= Time.deltaTime;
			if (countDown < 0f)
			{
				done = true;
				Fire();
			}
		}
	}

	private void RayCast(Vector3 start, Vector3 end)
	{
		Ray ray = new Ray(start, end - start);
		float maxDistance = Vector3.Distance(start, end);
		RaycastHit[] array = Physics.RaycastAll(ray, maxDistance, mask);
		RaycastHit[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			RaycastHit raycastHit = array2[i];
			if (Vector3.Distance(raycastHit.point, base.transform.position) < 0.6f)
			{
				continue;
			}
			Controller component = raycastHit.transform.root.GetComponent<Controller>();
			if ((bool)component && (bool)damager && component == damager)
			{
				continue;
			}
			for (int j = 0; j < hitParts.Length; j++)
			{
				hitParts[j].transform.position = raycastHit.point;
				hitParts[j].transform.rotation = Quaternion.LookRotation(raycastHit.normal);
				hitParts[j].Emit(particleNumbers[j]);
			}
			explosion.transform.position = raycastHit.point;
			explosion.Explode();
			if ((bool)raycastHit.rigidbody)
			{
				BodyPart component2 = raycastHit.rigidbody.GetComponent<BodyPart>();
				if ((bool)component2)
				{
					component2.TakeDamageWithParticle(damage, raycastHit.point, base.transform.forward, damager);
				}
			}
		}
	}

	private IEnumerator RayCasts()
	{
		ScreenshakeHandler.Instance.AddShake(base.transform.forward * shake);
		RayCast(startPoint, endPoint);
		yield return new WaitForEndOfFrame();
		RayCast(endPoint, startPoint);
	}

	private void Fire()
	{
		StartCoroutine(RayCasts());
	}

	private void HitSomething(RaycastHit hit)
	{
	}
}
