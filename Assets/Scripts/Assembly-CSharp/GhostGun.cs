using UnityEngine;

public class GhostGun : MonoBehaviour
{
	private Renderer[] rends;

	public Material on;

	public Material off;

	private WeaponPickUp pickup;

	private bool isOn = true;

	private void Start()
	{
		rends = GetComponentsInChildren<Renderer>();
		pickup = GetComponent<WeaponPickUp>();
	}

	private void Update()
	{
		if (pickup.cantBePickledUpFor > 0f)
		{
			if (isOn)
			{
				Renderer[] array = rends;
				foreach (Renderer renderer in array)
				{
					renderer.sharedMaterial = off;
				}
			}
			isOn = false;
			return;
		}
		if (isOn)
		{
			Renderer[] array2 = rends;
			foreach (Renderer renderer2 in array2)
			{
				renderer2.sharedMaterial = on;
			}
		}
		isOn = true;
	}
}
