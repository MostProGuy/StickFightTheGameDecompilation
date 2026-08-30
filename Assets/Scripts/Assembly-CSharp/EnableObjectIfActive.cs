using UnityEngine;

public class EnableObjectIfActive : MonoBehaviour
{
	public GameObject obj;

	private Weapon weapon;

	private void Start()
	{
		weapon = GetComponent<Weapon>();
	}

	private void Update()
	{
		if (weapon.isActive)
		{
			obj.SetActive(true);
		}
		else
		{
			obj.SetActive(false);
		}
	}
}
