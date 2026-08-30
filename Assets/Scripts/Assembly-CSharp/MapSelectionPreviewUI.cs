using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapSelectionPreviewUI : MonoBehaviour
{
	[SerializeField]
	private Image m_Image;

	private TextMeshProUGUI m_InfoText;

	private SingleWeaponCellUI m_CurrentMap;

	private void Awake()
	{
		m_InfoText = GetComponentInChildren<TextMeshProUGUI>();
	}

	public void AssignNewPreview(Texture2D tex, SingleWeaponCellUI map)
	{
		m_CurrentMap = map;
		AssignTexture(tex);
		AssignText(map);
	}

	private void AssignTexture(Texture2D tex)
	{
		m_Image.color = Color.white;
		m_Image.sprite = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), Vector2.zero);
	}

	private void AssignText(SingleWeaponCellUI map)
	{
		string mapName = map.MapName;
		string author = map.Author;
		string description = map.Description;
		string dateTime = map.DateTime;
		m_InfoText.text = mapName;
		TextMeshProUGUI infoText = m_InfoText;
		infoText.text = infoText.text + "\n" + author;
		TextMeshProUGUI infoText2 = m_InfoText;
		infoText2.text = infoText2.text + "\n" + description;
		TextMeshProUGUI infoText3 = m_InfoText;
		infoText3.text = infoText3.text + "\n" + dateTime;
		map.AssignPreview(this);
	}

	public void TextUpdated(SingleWeaponCellUI map)
	{
		if (!(m_CurrentMap != map))
		{
			AssignText(map);
		}
	}
}
