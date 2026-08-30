using UnityEngine;

public class BowHandler : MonoBehaviour
{
	[Range(0f, 1f)]
	public float drawAmount;

	private float drawVelocity;

	private float currentDraw;

	public float velocityIncrease;

	public Transform top1;

	public Transform top2;

	public Transform bot1;

	public Transform bot2;

	public Transform stringPos;

	private float bendAmount = 20f;

	private Weapon weapon;

	private void Start()
	{
		weapon = GetComponentInParent<Weapon>();
	}

	private void FixedUpdate()
	{
		drawVelocity *= 0.8f;
	}

	private void Update()
	{
		drawAmount = weapon.currentCharge / weapon.maxChargeTime;
		if (drawAmount > currentDraw)
		{
			drawVelocity += Time.deltaTime * Mathf.Clamp(Mathf.Abs(drawAmount - currentDraw), 0f, 1f);
		}
		else
		{
			drawVelocity -= Time.deltaTime * Mathf.Clamp(Mathf.Abs(drawAmount - currentDraw), 0f, 1f);
		}
		currentDraw += drawVelocity * velocityIncrease;
		top1.transform.localRotation = Quaternion.Euler(105f + currentDraw * bendAmount, 0f, 0f);
		top2.transform.localRotation = Quaternion.Euler(15f + currentDraw * bendAmount, 0f, 0f);
		bot1.transform.localRotation = Quaternion.Euler(-105f - currentDraw * bendAmount, 0f, 0f);
		bot2.transform.localRotation = Quaternion.Euler(-15f - currentDraw * bendAmount, 0f, 0f);
		stringPos.localPosition = new Vector3(0f, 0f, 0.3f + currentDraw * 0.8f);
	}
}
