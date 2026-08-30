using UnityEngine;

public class InitWeaponPickUpOnAwake : MonoBehaviour
{
	private void Awake()
	{
		GetComponentInChildren<WeaponPickUp>(true).InitGroundWeapon();
	}
}
