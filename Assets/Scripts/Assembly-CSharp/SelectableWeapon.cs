using UnityEngine;

public class SelectableWeapon
{
	public int Index { get; private set; }

	public string WeaponName { get; private set; }

	public GameObject WeaponObject { get; private set; }

	public SelectableWeapon(int index, string name, GameObject pickup)
	{
		Index = index;
		WeaponName = name;
		WeaponObject = pickup;
	}
}
