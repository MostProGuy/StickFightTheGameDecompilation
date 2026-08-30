using System.Collections;
using UnityEngine;

public class EnableAfterFrame : MonoBehaviour
{
	public GameObject obj;

	private void Start()
	{
		StartCoroutine(DoIt());
	}

	private IEnumerator DoIt()
	{
		yield return new WaitForEndOfFrame();
		obj.SetActive(true);
	}
}
