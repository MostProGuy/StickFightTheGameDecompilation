using UnityEngine;
using UnityEngine.UI;

public class BlockAnimation : MonoBehaviour
{
	public bool isBlocking;

	private bool currentlyBlocking;

	private Color startColor;

	public Image img1;

	public Image img2;

	private CodeAnimation anim;

	private float sinceBlockStart = 1f;

	public float blockPower = 1f;

	private float blinkCounter;

	private void Start()
	{
		anim = GetComponentInChildren<CodeAnimation>();
		startColor = base.transform.root.GetComponentInChildren<LineRenderer>().sharedMaterial.color;
	}

	private void Update()
	{
		if (isBlocking)
		{
			img1.enabled = true;
			img2.enabled = true;
			if (!currentlyBlocking)
			{
				sinceBlockStart = 0f;
				anim.Play();
				currentlyBlocking = true;
			}
			blockPower -= Time.deltaTime * 0.5f;
			if (sinceBlockStart < 0.3f)
			{
				if (blinkCounter > 0.05f)
				{
					blinkCounter = 0f;
					if (img1.color == startColor)
					{
						img1.color = Color.white;
						img2.color = Color.white;
					}
					else
					{
						img1.color = startColor;
						img2.color = startColor;
					}
				}
			}
			else
			{
				img1.color = startColor;
				img2.color = startColor;
			}
		}
		else
		{
			img1.color = startColor;
			img2.color = startColor;
			blockPower += Time.deltaTime * 0.3f;
			if (currentlyBlocking)
			{
				blockPower -= 0.3f;
				currentlyBlocking = false;
			}
			img1.enabled = false;
			img2.enabled = false;
		}
		sinceBlockStart += Time.deltaTime;
		blinkCounter += Time.deltaTime;
		blockPower = Mathf.Clamp(blockPower, 0.3f, 1f);
		img1.fillAmount = Mathf.Lerp(img1.fillAmount, 0.11f * blockPower, Time.deltaTime * 10f);
		img2.fillAmount = Mathf.Lerp(img2.fillAmount, 0.11f * blockPower, Time.deltaTime * 10f);
	}
}
