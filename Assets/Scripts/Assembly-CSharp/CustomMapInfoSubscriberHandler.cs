using System;
using System.Collections;
using System.IO;
using InControl;
using Steamworks;
using TMPro;
using UnityEngine;

public class CustomMapInfoSubscriberHandler : MonoBehaviour
{
	private CodeStateAnimation m_MapInfoAnim;

	private CodeStateAnimation m_SubscribeAnim;

	private CodeStateAnimation[] m_Punches = new CodeStateAnimation[3];

	private TextMeshProUGUI m_MapNameText;

	private TextMeshProUGUI m_AuthorText;

	private PublishedFileId_t m_CurrentMapID;

	private bool m_Active;

	private int m_NumberOfPunches;

	private CallResult<SteamUGCQueryCompleted_t> m_UGCHandleQueryCompleted;

	private void Awake()
	{
		InitReferences();
	}

	private void InitReferences()
	{
		Transform transform = base.transform.Find("MapInfo");
		m_MapInfoAnim = transform.GetComponent<CodeStateAnimation>();
		m_MapNameText = transform.Find("CurrentMapText").GetComponent<TextMeshProUGUI>();
		m_AuthorText = transform.Find("By").GetComponent<TextMeshProUGUI>();
		Transform transform2 = base.transform.Find("Subscribe");
		m_SubscribeAnim = transform2.GetComponent<CodeStateAnimation>();
		int num = m_Punches.Length;
		for (int i = 0; i < num; i++)
		{
			Transform transform3 = transform2.Find("Image" + (i + 1));
			m_Punches[i] = transform3.GetComponent<CodeStateAnimation>();
		}
		m_UGCHandleQueryCompleted = CallResult<SteamUGCQueryCompleted_t>.Create(OnSteamUGCQueryCompleted);
	}

	private void OnSteamUGCQueryCompleted(SteamUGCQueryCompleted_t param, bool bIOFailure)
	{
		SteamUGCDetails_t pDetails;
		if (bIOFailure)
		{
			Debug.LogError("BioFail");
		}
		else if (param.m_eResult == EResult.k_EResultOK && SteamUGC.GetQueryUGCResult(param.m_handle, 0u, out pDetails))
		{
			CSteamID cSteamID = new CSteamID(pDetails.m_ulSteamIDOwner);
			if (!SteamFriends.RequestUserInformation(cSteamID, true))
			{
				string friendPersonaName = SteamFriends.GetFriendPersonaName(cSteamID);
				Debug.Log("Map Info Recieved: " + pDetails.m_rgchTitle + " Author: " + friendPersonaName);
				m_CurrentMapID = pDetails.m_nPublishedFileId;
				AssignText(pDetails.m_rgchTitle, friendPersonaName);
				ShowMapInfo();
			}
			else
			{
				AssignMapText(pDetails.m_rgchTitle);
				StartCoroutine(WaitingForLove(cSteamID));
			}
		}
	}

	private IEnumerator WaitingForLove(CSteamID user)
	{
		while (SteamFriends.RequestUserInformation(user, true))
		{
			yield return null;
		}
		string authorName = SteamFriends.GetFriendPersonaName(user);
		AssignAuthorText(authorName);
		ShowMapInfo();
	}

	private void ShowMapInfo()
	{
		m_MapInfoAnim.state1 = false;
	}

	public void HideMapInfo()
	{
		m_MapInfoAnim.state1 = true;
	}

	private void AssignAuthorText(string author)
	{
		m_AuthorText.text = author;
	}

	private void AssignMapText(string mapTitle)
	{
		m_MapNameText.text = mapTitle;
	}

	private void AssignText(string mapTitle, string authorName)
	{
		m_MapNameText.text = mapTitle;
		m_AuthorText.text = authorName;
	}

	public void AssignNewMap(MapWrapper newMap)
	{
		MapType mapType = (MapType)newMap.MapType;
		if (mapType != MapType.CustomOnline)
		{
			m_CurrentMapID = new PublishedFileId_t(0uL);
		}
		SingleMapUI singleMapUI = null;
		switch (mapType)
		{
		case MapType.Landfall:
		{
			int num2 = BitConverter.ToInt32(newMap.MapData, 0);
			singleMapUI = MapSelectionHandler.Instance.FindSingleMapByIndex(num2.ToString());
			if (singleMapUI != null)
			{
				AssignText(singleMapUI.MapName, "Landfall");
				ShowMapInfo();
			}
			break;
		}
		case MapType.CustomLocal:
		{
			string mapIndex;
			using (MemoryStream input = new MemoryStream(newMap.MapData))
			{
				using (BinaryReader binaryReader = new BinaryReader(input))
				{
					mapIndex = binaryReader.ReadString();
				}
			}
			singleMapUI = MapSelectionHandler.Instance.FindSingleMapByIndex(mapIndex);
			if (singleMapUI != null)
			{
				AssignText(singleMapUI.MapName, SteamFriends.GetPersonaName());
				ShowMapInfo();
			}
			break;
		}
		case MapType.CustomOnline:
		{
			ulong num = BitConverter.ToUInt64(newMap.MapData, 0);
			Debug.Log("Assigning new Map: " + num);
			UGCQueryHandle_t handle = SteamUGC.CreateQueryUGCDetailsRequest(new PublishedFileId_t[1]
			{
				new PublishedFileId_t(num)
			}, 1u);
			SteamAPICall_t hAPICall = SteamUGC.SendQueryUGCRequest(handle);
			m_UGCHandleQueryCompleted.Set(hAPICall);
			break;
		}
		}
	}

	public void HideSubscribe()
	{
		Debug.Log("Hiding Subscribe");
		m_SubscribeAnim.state1 = true;
		m_Active = false;
		ResetAllPunches();
	}

	public void ShowSubscribe()
	{
		if (!(m_CurrentMapID == new PublishedFileId_t(0uL)))
		{
			EItemState itemState = (EItemState)SteamUGC.GetItemState(m_CurrentMapID);
			if ((itemState & EItemState.k_EItemStateSubscribed) == 0)
			{
				Debug.Log("Showing Subscribe");
				m_SubscribeAnim.state1 = false;
				m_Active = true;
			}
		}
	}

	private void Update()
	{
		if (m_Active)
		{
			ListenForPunches();
		}
	}

	private void ListenForPunches()
	{
		CharacterActions characterActions = CharacterActions.CreateWithAnyBindings();
		InputDevice activeDevice = InputManager.ActiveDevice;
		if (activeDevice.RightTrigger.WasPressed || activeDevice.Action3.WasPressed || Input.GetKeyDown(KeyCode.C) || Input.GetMouseButtonDown(0))
		{
			Debug.Log("Punch!");
			DoPunch();
		}
	}

	private void DoPunch()
	{
		m_NumberOfPunches++;
		if (m_NumberOfPunches >= m_Punches.Length)
		{
			m_Punches[m_Punches.Length - 1].state1 = false;
			SubscribeTo();
		}
		else
		{
			m_Punches[m_NumberOfPunches - 1].state1 = false;
		}
	}

	private void SubscribeTo()
	{
		Debug.Log("Subscribing!");
		SteamUGC.SubscribeItem(m_CurrentMapID);
	}

	private void ResetAllPunches()
	{
		int num = m_Punches.Length;
		for (int i = 0; i < num; i++)
		{
			m_Punches[i].state1 = true;
		}
		m_NumberOfPunches = 0;
	}
}
