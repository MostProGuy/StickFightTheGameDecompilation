using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BossHealth : MonoBehaviour
{
	public HealthHandler bossHealth;

	public Image red;

	public Image white;

	public Image black;

	private float bossHp;

	public AnimationCurve redCurve;

	public AnimationCurve whiteCurve;

	public float duration;

	private CodeStateAnimation codeAnim;

	private void Start()
	{
		codeAnim = GetComponentInChildren<CodeStateAnimation>();
		AttachBoss(bossHealth);
	}

	public void AttachBoss(HealthHandler healthHandler)
	{
		red.fillAmount = 1f;
		white.fillAmount = 1f;
		bossHealth = healthHandler;
		bossHp = bossHealth.health;
	}

	private void Update()
	{
		if (codeAnim.state1 && (bool)bossHealth)
		{
			if (bossHealth.health > bossHp)
			{
				AttachBoss(bossHealth);
			}
			else if (bossHealth.health != bossHp)
			{
				StartCoroutine(ShowDamage(bossHp - bossHealth.health));
				bossHp = bossHealth.health;
			}
		}
	}

	private IEnumerator ShowDamage(float damage)
	{
		float f = 0f;
		float percentageLost = damage / bossHealth.maxHealth;
		float lastValue = 0f;
		while (f < 1f)
		{
			f += Time.deltaTime / duration;
			float curveValue = redCurve.Evaluate(f);
			float deltaValue = curveValue - lastValue;
			lastValue = curveValue;
			red.fillAmount -= deltaValue * percentageLost;
			yield return null;
		}
		yield return new WaitForSeconds(0.3f);
		f = 0f;
		lastValue = 0f;
		while (f < 1f)
		{
			f += Time.deltaTime / duration;
			float curveValue2 = whiteCurve.Evaluate(f);
			float deltaValue2 = curveValue2 - lastValue;
			lastValue = curveValue2;
			white.fillAmount -= deltaValue2 * percentageLost;
			yield return null;
		}
	}
}
