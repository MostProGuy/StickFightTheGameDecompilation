using TMPro;
using UnityEngine;

public class BombSpawn : MonoBehaviour
{
	private TextMeshPro text;

	private SpawnObjectAfterDelay bombScipt;

	private Rigidbody rig;

	private void Start()
	{
		text = GetComponentInChildren<TextMeshPro>();
		bombScipt = GetComponentInChildren<SpawnObjectAfterDelay>();
		rig = GetComponent<Rigidbody>();
	}

	private void Update()
	{
		text.text = bombScipt.secondsBeforeSpawn.ToString("F0");
	}

	private void OnDestroy()
	{
		Explode();
	}

	public void Explode()
	{
		HealthHandler[] array = Object.FindObjectsOfType<HealthHandler>();
		foreach (HealthHandler healthHandler in array)
		{
			if (base.transform.position.z > 0f == healthHandler.GetComponentInChildren<Head>().transform.position.z > 0f)
			{
				healthHandler.TakeDamage(200f, null);
			}
		}
	}
}
