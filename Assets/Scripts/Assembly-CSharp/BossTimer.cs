using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossTimer : MonoBehaviour
{
	public Weapon weapon;

	public Image phaseRing;

	private GameObject canvas;

	public TextMeshProUGUI text;

	private string phaseText;

	private Fighting fight;

	private bool hasSwitched;

	public CountDown count;

	private void Start()
	{
		fight = base.transform.root.GetComponent<Fighting>();
	}

	private void Update()
	{
		float num = 0.5f;
		num = (weapon.loseWeaponOnReload ? 0f : (Mathf.Clamp(weapon.loseWeaponCurrentTime / weapon.loseWeaponAfter, 0f, 1f) * 0.5f));
		phaseRing.fillAmount = num;
		if (weapon.loseWeaponAfter - weapon.loseWeaponCurrentTime < 1f && !hasSwitched)
		{
			count.Countdown();
			hasSwitched = true;
		}
		if (fight.counter < 0f)
		{
			text.text = "WAIT...";
		}
		else
		{
			text.text = phaseText;
		}
	}

	public void ConnectWeapon(Weapon w, string phaseName)
	{
		if (w == null)
		{
			base.gameObject.SetActive(false);
			return;
		}
		phaseText = phaseName;
		text.GetComponent<CodeAnimation>().Play();
		weapon = w;
		base.gameObject.SetActive(true);
		if (w != null && !w.loseWeaponOnReload)
		{
			hasSwitched = false;
		}
	}
}
