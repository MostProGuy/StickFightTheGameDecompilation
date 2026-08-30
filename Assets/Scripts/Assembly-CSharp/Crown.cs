using System.Collections;
using UnityEngine;

public class Crown : MonoBehaviour
{
	private Transform target;

	public Controller crownBarrer;

	private int crownLevel;

	public GameObject[] crownLevels;

	private void Start()
	{
	}

	private void Update()
	{
		if ((bool)target)
		{
			base.transform.position = target.position;
		}
	}

	public void SetNewKing(Controller character, bool win)
	{
		if (!(character == null))
		{
			target = character.GetComponentInChildren<HeadRenderer>().transform;
			crownBarrer = character;
		}
	}

	private void ResetKing()
	{
		crownLevel = 0;
		SetCrownLevel(crownLevel);
	}

	private void SetCrownLevel(int index)
	{
		if (index >= crownLevels.Length)
		{
			ResetKing();
			return;
		}
		for (int i = 0; i < crownLevels.Length; i++)
		{
			crownLevels[i].SetActive(false);
		}
		crownLevels[index].SetActive(true);
		if (index == crownLevels.Length - 1)
		{
			BeKing();
		}
	}

	private void BeKing()
	{
		Debug.Log("IS KING");
	}

	private IEnumerator ChangeOwner()
	{
		float t = 0f;
		yield return new WaitForSeconds(1f);
	}
}
