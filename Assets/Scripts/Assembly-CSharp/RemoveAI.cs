using System.Collections;
using UnityEngine;

public class RemoveAI : MonoBehaviour
{
	private void Start()
	{
		StartCoroutine(removeMe());
	}

	private void Update()
	{
	}

	private IEnumerator removeMe()
	{
		yield return new WaitForSeconds(2f);
		Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
		foreach (Collider collider in componentsInChildren)
		{
			collider.enabled = false;
		}
		yield return new WaitForSeconds(2f);
		Object.Destroy(base.gameObject);
	}
}
