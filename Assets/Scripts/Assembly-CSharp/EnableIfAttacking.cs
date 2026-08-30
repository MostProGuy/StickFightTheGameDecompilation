using UnityEngine;

public class EnableIfAttacking : MonoBehaviour
{
	public GameObject obj;

	private AI ai;

	private void Start()
	{
		ai = base.transform.root.GetComponent<AI>();
	}

	private void Update()
	{
		if (ai.attacking)
		{
			obj.SetActive(true);
		}
		else
		{
			obj.SetActive(false);
		}
	}
}
