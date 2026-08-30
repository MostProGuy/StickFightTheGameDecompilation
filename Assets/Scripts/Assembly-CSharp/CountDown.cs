using System.Collections;
using UnityEngine;

public class CountDown : MonoBehaviour
{
	public GameObject obj3;

	public GameObject obj2;

	public GameObject obj1;

	public GameObject objFight;

	public float timePerMessage = 0.3f;

	public void Countdown()
	{
		StartCoroutine(Count());
	}

	private IEnumerator Count()
	{
		yield return null;
		yield return null;
		yield return null;
		obj3.SetActive(true);
		yield return new WaitForSecondsRealtime(timePerMessage);
		obj2.SetActive(true);
		yield return new WaitForSecondsRealtime(timePerMessage);
		obj1.SetActive(true);
		yield return new WaitForSecondsRealtime(timePerMessage);
		objFight.SetActive(true);
	}
}
