using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SetImageColorByID : MonoBehaviour
{
	public Color orang;

	public Color blu;

	public Color rad;

	public Color gren;

	public TextMeshProUGUI text;

	private Controller controller;

	private Image img;

	private void Start()
	{
		img = GetComponent<Image>();
		text = GetComponent<TextMeshProUGUI>();
		controller = base.transform.root.GetComponent<Controller>();
		if (!controller)
		{
			return;
		}
		if (controller.playerID == 0)
		{
			if ((bool)img)
			{
				img.color = orang;
			}
			if ((bool)text)
			{
				text.color = orang;
			}
		}
		if (controller.playerID == 1)
		{
			if ((bool)img)
			{
				img.color = blu;
			}
			if ((bool)text)
			{
				text.color = blu;
			}
		}
		if (controller.playerID == 2)
		{
			if ((bool)img)
			{
				img.color = rad;
			}
			if ((bool)text)
			{
				text.color = rad;
			}
		}
		if (controller.playerID == 3)
		{
			if ((bool)img)
			{
				img.color = gren;
			}
			if ((bool)text)
			{
				text.color = gren;
			}
		}
	}

	private void Update()
	{
	}
}
