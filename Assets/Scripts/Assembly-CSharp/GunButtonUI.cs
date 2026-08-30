using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GunButtonUI : MonoBehaviour
{
	private string m_WeaponName;

	private int m_WeaponIndex;

	public void Init(string weaponName, int index, Action<string, int> a)
	{
		m_WeaponName = weaponName;
		m_WeaponIndex = index;
		GetComponentInChildren<TextMeshProUGUI>().text = m_WeaponName.ToUpper();
		Button component = GetComponent<Button>();
		component.onClick.AddListener(() =>
		{
			a(m_WeaponName, m_WeaponIndex);
		});
	}
}
