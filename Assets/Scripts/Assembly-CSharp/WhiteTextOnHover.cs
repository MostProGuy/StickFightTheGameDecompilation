using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class WhiteTextOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IEventSystemHandler
{
	public static GameObject CURRENTLY_SELECTED;

	private TextMeshProUGUI m_text;

	private void Start()
	{
		m_text = GetComponentInChildren<TextMeshProUGUI>();
	}

	private void OnDestroy()
	{
	}

	private void Update()
	{
		GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
		PointerEventData pointerEventData = ExtendedInputModule.GetPointerEventData();
		bool flag = pointerEventData.hovered.Contains(base.gameObject);
		if (currentSelectedGameObject != null)
		{
			if (currentSelectedGameObject == base.gameObject)
			{
				BeginHover();
			}
			else if (flag)
			{
				BeginHover();
			}
			else
			{
				EndHover();
			}
		}
		else if (flag)
		{
			BeginHover();
		}
		else
		{
			EndHover();
		}
	}

	public void BeginHover()
	{
		m_text.color = Color.black;
	}

	public void EndHover()
	{
		m_text.color = Color.white;
	}

	public void BeginSelect()
	{
		m_text.color = Color.black;
	}

	public void EndSelect()
	{
		m_text.color = Color.white;
	}

	public void OnPointerDown(PointerEventData eventData)
	{
	}

	public void OnPointerUp(PointerEventData eventData)
	{
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (!(CURRENTLY_SELECTED == base.gameObject))
		{
			EndHover();
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		BeginHover();
	}
}
