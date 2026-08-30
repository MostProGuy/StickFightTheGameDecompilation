using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class SetTextColorOnSelected : MonoBehaviour, IPointerEnterHandler, ISelectHandler, IEventSystemHandler
{
	private TextMeshProUGUI text;

	private Color c;

	private void Start()
	{
		text = GetComponentInChildren<TextMeshProUGUI>();
		c = text.color;
	}

	private void Update()
	{
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
	}

	public void OnPointerExit(PointerEventData eventData)
	{
	}

	public void OnSelect(BaseEventData eventData)
	{
	}
}
