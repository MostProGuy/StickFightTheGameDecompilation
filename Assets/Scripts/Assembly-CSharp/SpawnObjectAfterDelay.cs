using UnityEngine;

public class SpawnObjectAfterDelay : MonoBehaviour
{
	public GameObject objectToSpawn;

	public float secondsBeforeSpawn;

	public bool removeSelf = true;

	public Vector3 specificSpawnRotation;

	public bool identitySpawnRotation;

	private bool done;

	public Controller damager;

	public bool dontStartCounting;

	private void Start()
	{
		RandomValue component = GetComponent<RandomValue>();
		if ((bool)component)
		{
			secondsBeforeSpawn *= component.value;
		}
	}

	public void StartCounting()
	{
		dontStartCounting = false;
	}

	private void Update()
	{
		if (!dontStartCounting)
		{
			secondsBeforeSpawn -= Time.deltaTime;
			if (secondsBeforeSpawn < 0f && !done)
			{
				Spawn();
			}
		}
	}

	public void Spawn()
	{
		if (!done)
		{
			done = true;
			Quaternion rotation = base.transform.rotation;
			if (specificSpawnRotation != Vector3.zero)
			{
				rotation = Quaternion.Euler(specificSpawnRotation);
			}
			if (identitySpawnRotation)
			{
				rotation = Quaternion.identity;
			}
			GameObject gameObject = Object.Instantiate(objectToSpawn, base.transform.position, rotation);
			if (removeSelf)
			{
				Object.Destroy(base.gameObject);
			}
			Explosion componentInChildren = gameObject.GetComponentInChildren<Explosion>();
			if ((bool)componentInChildren && (bool)damager)
			{
				componentInChildren.damager = damager;
			}
		}
	}
}
