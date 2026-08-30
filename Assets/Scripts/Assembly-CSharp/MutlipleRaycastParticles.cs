using UnityEngine;

public class MutlipleRaycastParticles : MonoBehaviour
{
	public Transform[] raycastObjects;

	public float cd = 0.1f;

	private ParticleSystem part;

	public LayerMask mask;

	private AudioSource au;

	private AudioLowPassFilter filter;

	private float hitValue;

	private void Start()
	{
		au = GetComponent<AudioSource>();
		filter = GetComponent<AudioLowPassFilter>();
		part = GetComponentInChildren<ParticleSystem>();
	}

	private void Update()
	{
		hitValue = Mathf.Lerp(hitValue, 0.1f, Time.deltaTime * 15f);
		filter.cutoffFrequency = Mathf.Lerp(filter.cutoffFrequency, 22000f * hitValue, Time.deltaTime * 25f);
		au.volume = Mathf.Lerp(au.volume, hitValue * 0.8f, Time.deltaTime * 25f);
		Transform[] array = raycastObjects;
		foreach (Transform t in array)
		{
			RayCast(t);
		}
		ScreenshakeHandler.Instance.AddShake(Random.insideUnitSphere * Time.deltaTime * 0.2f);
	}

	public void RayCast(Transform t)
	{
		float num = 0.8f;
		Ray ray = new Ray(t.position - t.up * 0.5f, t.up);
		RaycastHit hitInfo;
		Physics.Raycast(ray, out hitInfo, 0.8f, mask);
		if ((bool)hitInfo.transform)
		{
			part.transform.position = hitInfo.point;
			part.transform.rotation = Quaternion.LookRotation(hitInfo.normal);
			part.Emit(1);
			ScreenshakeHandler.Instance.AddShake(hitInfo.point.normalized * Random.value * Time.deltaTime * 1f);
			num = Vector3.Distance(hitInfo.point, t.position);
			hitValue += (0.8f - hitValue + 0.2f) * Time.deltaTime * 8f;
		}
	}
}
