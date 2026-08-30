using System.Collections;
using UnityEngine;

public class CodeStateAnimation : MonoBehaviour
{
	public enum AnimationType
	{
		Position = 0,
		Scale = 1
	}

	public AnimationType animationType;

	public bool state1 = true;

	public bool rectScale = true;

	public bool scaleWithTimeScale;

	public bool isAnimating;

	private bool isState1 = true;

	public bool useX = true;

	public bool useY = true;

	public bool useZ = true;

	[Header("State 1")]
	public AnimationCurve curve;

	public float duration = 1f;

	public float multiplier = 1f;

	[Header("")]
	[Header("State 2")]
	public AnimationCurve curve2;

	public float duration2 = 1f;

	public float multiplier2 = 1f;

	private float baseX;

	private float baseY;

	private float baseZ;

	private RectTransform rectTrans;

	[HideInInspector]
	public float dontPlayFor;

	private void Start()
	{
		rectTrans = GetComponent<RectTransform>();
		if (animationType == AnimationType.Position)
		{
			if ((bool)rectTrans)
			{
				baseX = rectTrans.anchoredPosition.x;
				baseY = rectTrans.anchoredPosition.y;
				if (!state1)
				{
					float num = curve2.Evaluate(1f) * multiplier2;
					Vector2 anchoredPosition = rectTrans.anchoredPosition;
					if (useX)
					{
						anchoredPosition.x = num;
					}
					if (useY)
					{
						anchoredPosition.y = num;
					}
					rectTrans.anchoredPosition = anchoredPosition;
					isState1 = false;
				}
			}
			else
			{
				baseX = base.transform.localPosition.x;
				baseY = base.transform.localPosition.y;
				baseZ = base.transform.localPosition.z;
			}
		}
		if (animationType != AnimationType.Scale)
		{
			return;
		}
		if ((bool)rectTrans && rectScale)
		{
			baseX = rectTrans.sizeDelta.x;
			baseY = rectTrans.sizeDelta.y;
			if (!state1)
			{
				float num2 = curve2.Evaluate(1f);
				Vector3 vector = rectTrans.sizeDelta;
				if (useX)
				{
					vector.x = num2 * baseX;
				}
				if (useY)
				{
					vector.y = num2 * baseY;
				}
				rectTrans.sizeDelta = vector * multiplier;
				isState1 = false;
			}
			return;
		}
		baseX = base.transform.localScale.x;
		baseY = base.transform.localScale.y;
		baseZ = base.transform.localScale.z;
		if (!state1)
		{
			float num3 = curve2.Evaluate(1f);
			Vector3 localScale = base.transform.localScale;
			if (useX)
			{
				localScale.x = num3 * baseX;
			}
			if (useY)
			{
				localScale.y = num3 * baseY;
			}
			if (useZ)
			{
				localScale.z = num3 * baseZ;
			}
			base.transform.localScale = localScale;
			isState1 = false;
		}
	}

	public void Switch()
	{
		state1 = !state1;
	}

	private void Update()
	{
		if (dontPlayFor > 0f)
		{
			dontPlayFor -= Time.deltaTime;
		}
		if (state1 && !isState1 && !isAnimating)
		{
			if (dontPlayFor <= 0f)
			{
				StartCoroutine(Animation1());
			}
			isState1 = true;
		}
		if (!state1 && isState1 && !isAnimating)
		{
			if (dontPlayFor <= 0f)
			{
				StartCoroutine(Animation2());
			}
			isState1 = false;
		}
	}

	private IEnumerator Animation1()
	{
		isAnimating = true;
		float t = 0f;
		while (t < duration)
		{
			t = ((!scaleWithTimeScale) ? (t + Mathf.Clamp(Time.unscaledDeltaTime, 0f, 0.02f)) : (t + Mathf.Clamp(Time.deltaTime, 0f, 0.02f)));
			float curveValue = curve.Evaluate(t / duration) * multiplier;
			if (animationType == AnimationType.Position)
			{
				if ((bool)rectTrans)
				{
					Vector2 anchoredPosition = rectTrans.anchoredPosition;
					if (useX)
					{
						anchoredPosition.x = curveValue + baseX;
					}
					if (useY)
					{
						anchoredPosition.y = curveValue + baseY;
					}
					rectTrans.anchoredPosition = anchoredPosition;
				}
				else
				{
					Vector3 localPosition = base.transform.localPosition;
					if (useX)
					{
						localPosition.x = curveValue + baseX;
					}
					if (useY)
					{
						localPosition.y = curveValue + baseY;
					}
					if (useZ)
					{
						localPosition.z = curveValue + baseZ;
					}
					base.transform.localPosition = localPosition;
				}
			}
			if (animationType == AnimationType.Scale)
			{
				if ((bool)rectTrans && rectScale)
				{
					Vector2 sizeDelta = rectTrans.sizeDelta;
					if (useX)
					{
						sizeDelta.x = curveValue * baseX;
					}
					if (useY)
					{
						sizeDelta.y = curveValue * baseY;
					}
					rectTrans.sizeDelta = sizeDelta;
				}
				else
				{
					Vector3 localScale = base.transform.localScale;
					if (useX)
					{
						localScale.x = curveValue * baseX;
					}
					if (useY)
					{
						localScale.y = curveValue * baseY;
					}
					if (useZ)
					{
						localScale.z = curveValue * baseZ;
					}
					base.transform.localScale = localScale;
				}
			}
			yield return new WaitForEndOfFrame();
		}
		isAnimating = false;
	}

	private IEnumerator Animation2()
	{
		isAnimating = true;
		float t = 0f;
		while (t < duration2)
		{
			t = ((!scaleWithTimeScale) ? (t + Mathf.Clamp(Time.unscaledDeltaTime, 0f, 0.02f)) : (t + Mathf.Clamp(Time.deltaTime, 0f, 0.02f)));
			float curveValue = curve2.Evaluate(t / duration2) * multiplier2;
			if (animationType == AnimationType.Position)
			{
				if ((bool)rectTrans)
				{
					Vector2 anchoredPosition = rectTrans.anchoredPosition;
					if (useX)
					{
						anchoredPosition.x = curveValue + baseX;
					}
					if (useY)
					{
						anchoredPosition.y = curveValue + baseY;
					}
					rectTrans.anchoredPosition = anchoredPosition;
				}
				else
				{
					Vector3 localPosition = base.transform.localPosition;
					if (useX)
					{
						localPosition.x = curveValue + baseX;
					}
					if (useY)
					{
						localPosition.y = curveValue + baseY;
					}
					if (useZ)
					{
						localPosition.z = curveValue + baseZ;
					}
					base.transform.localPosition = localPosition;
				}
			}
			if (animationType == AnimationType.Scale)
			{
				if ((bool)rectTrans && rectScale)
				{
					Vector2 sizeDelta = rectTrans.sizeDelta;
					if (useX)
					{
						sizeDelta.x = curveValue * baseX;
					}
					if (useY)
					{
						sizeDelta.y = curveValue * baseY;
					}
					rectTrans.sizeDelta = sizeDelta;
				}
				else
				{
					Vector3 localScale = base.transform.localScale;
					if (useX)
					{
						localScale.x = curveValue * baseX;
					}
					if (useY)
					{
						localScale.y = curveValue * baseY;
					}
					if (useZ)
					{
						localScale.z = curveValue * baseZ;
					}
					base.transform.localScale = localScale;
				}
			}
			yield return new WaitForEndOfFrame();
		}
		isAnimating = false;
	}
}
