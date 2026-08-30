using System;
using System.Collections;
using Steamworks;
using UnityEngine;
using UnityEngine.EventSystems;

public class SingleWeaponCellUI : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler
{
	private Action<SingleWeaponCellUI> m_PreviewAction;

	private static MapSelectionPreviewUI m_Preview;

	public byte[] ImageData { get; private set; }

	public string MapIndex { get; private set; }

	public string MapName { get; private set; }

	public string Description { get; private set; }

	public string Author { get; private set; }

	public string DateTime { get; private set; }

	public void Init(SingleMapUI map, Action<SingleWeaponCellUI> previewAction)
	{
		MapIndex = map.MapIndex;
		ImageData = map.GetImageData();
		MapName = map.MapName;
		Author = GetAuthor(map);
		Description = GetDescription(map);
		DateTime = GetDate(map);
		m_PreviewAction = previewAction;
	}

	private string GetAuthor(SingleMapUI map)
	{
		switch (map.MapTypeEnum)
		{
		case MapType.Landfall:
			return "LANDFALL";
		case MapType.CustomLocal:
			return SteamFriends.GetPersonaName();
		case MapType.CustomOnline:
		{
			CSteamID authorID = map.CustomWrapper.AuthorID;
			if (SteamFriends.RequestUserInformation(authorID, true))
			{
				StartCoroutine(WaitingForLove(authorID));
			}
			return SteamFriends.GetFriendPersonaName(authorID);
		}
		default:
			return "Unknown";
		}
	}

	public void AssignPreview(MapSelectionPreviewUI mapSelectionPreviewUI)
	{
		m_Preview = mapSelectionPreviewUI;
	}

	private IEnumerator WaitingForLove(CSteamID user)
	{
		while (SteamFriends.RequestUserInformation(user, true))
		{
			yield return null;
		}
		Author = SteamFriends.GetFriendPersonaName(user);
		if (m_Preview != null)
		{
			m_Preview.TextUpdated(this);
		}
	}

	private string GetDescription(SingleMapUI map)
	{
		return string.Empty;
	}

	private string GetDate(SingleMapUI map)
	{
		switch (map.MapTypeEnum)
		{
		case MapType.Landfall:
			return "2017/09/27";
		case MapType.CustomLocal:
			return map.DateTime;
		case MapType.CustomOnline:
			return map.DateTime;
		default:
			return "Map";
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		m_PreviewAction(this);
	}
}
