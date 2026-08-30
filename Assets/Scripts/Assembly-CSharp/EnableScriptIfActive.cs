using UnityEngine;

public class EnableScriptIfActive : MonoBehaviour
{
	public MonoBehaviour scriptToEnable;

	private Weapon weapon;

	private void Start()
	{
		weapon = GetComponentInParent<Weapon>();
	}

	private void Update()
	{
		if (weapon.isActive)
		{
			scriptToEnable.enabled = true;
		}
		else
		{
			scriptToEnable.enabled = false;
		}
	}
}
