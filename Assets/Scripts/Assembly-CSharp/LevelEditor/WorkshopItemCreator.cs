using System;
using Steamworks;
using UnityEngine;

namespace LevelEditor
{
	public class WorkshopItemCreator
	{
		private PublishedFileId_t m_publishedFileID;

		private UGCUpdateHandle_t m_UGCUpdateHandle;

		private Callback<DownloadItemResult_t> m_DownloadItemCallResult;

		private CallResult<CreateItemResult_t> m_CreateItemResult;

		private CallResult<SubmitItemUpdateResult_t> m_SubmitItemUpdateResult;

		private static readonly AppId_t APP_ID = new AppId_t(674940u);

		private Action mOnCreateAction;

		private Action mOnItemUpdtedAction;

		public WorkshopItemCreator()
		{
			m_CreateItemResult = CallResult<CreateItemResult_t>.Create(OnItemCreated);
			m_SubmitItemUpdateResult = CallResult<SubmitItemUpdateResult_t>.Create(OnSubmitItemUpdateResult);
			m_DownloadItemCallResult = Callback<DownloadItemResult_t>.Create(OnItemDownloaded);
		}

		public void Upload(string path, string levelName, string description)
		{
			Debug.Log("Uploading item: " + path + " Name: " + levelName + " Description: " + description);
			CreateNewItem();
			OnItemCreatedAction(() =>
			{
				UpdateItem(GetPublishFileID(), path, levelName, description);
			});
		}

		public void CreateNewItem()
		{
			Debug.Log("Creating New Item...");
			SteamAPICall_t hAPICall = SteamUGC.CreateItem(APP_ID, EWorkshopFileType.k_EWorkshopFileTypeFirst);
			m_CreateItemResult.Set(hAPICall, OnItemCreated);
		}

		public void UpdateItem(PublishedFileId_t pID, string path, string levelName, string description)
		{
			m_publishedFileID = pID;
			m_UGCUpdateHandle = SteamUGC.StartItemUpdate(SteamUtils.GetAppID(), m_publishedFileID);
			SteamUGC.SetItemTitle(m_UGCUpdateHandle, levelName);
			SteamUGC.SetItemDescription(m_UGCUpdateHandle, description);
			SteamUGC.SetItemContent(m_UGCUpdateHandle, path);
			SteamUGC.SetItemPreview(m_UGCUpdateHandle, WorkshopDataHolder.Instance.workshopData.previewImagePath);
			Debug.Log(string.Concat("Updating Item: ", m_publishedFileID, " Path: ", path, " LevelName: ", levelName));
			SteamAPICall_t hAPICall = SteamUGC.SubmitItemUpdate(m_UGCUpdateHandle, string.Empty);
			m_SubmitItemUpdateResult.Set(hAPICall);
			OnItemUpdatedAction(() =>
			{
				WorkshopDataHolder.Instance.workshopData.publishedFileID = m_publishedFileID;
				Action noAction = () =>
				{
				};
				string page = "http://steamcommunity.com/sharedfiles/filedetails/?id=" + m_publishedFileID.ToString();
				Action yesAction = () =>
				{
					SteamFriends.ActivateGameOverlayToWebPage(page);
				};
				DialougePanelUI.Instance.GiveChoice("Upload Successful! Do you want to go to the steampage?", yesAction, noAction);
				SubscribeToItem(m_publishedFileID);
				Reset();
			});
		}

		private void OnItemCreated(CreateItemResult_t param, bool bIOFailure)
		{
			if (bIOFailure)
			{
				Debug.LogError("Bio failure");
				return;
			}
			if (param.m_eResult == EResult.k_EResultOK)
			{
				Debug.Log("Item Created!");
				m_publishedFileID = param.m_nPublishedFileId;
				if (mOnCreateAction != null)
				{
					mOnCreateAction();
				}
			}
			else
			{
				Debug.LogError("There was an error creating a new workshop item: " + param.m_eResult);
			}
			if (param.m_bUserNeedsToAcceptWorkshopLegalAgreement)
			{
				string pchURL = "steam://url/CommunityFilePage/674940";
				SteamFriends.ActivateGameOverlayToWebPage(pchURL);
				Debug.Log("User need to accept Steams Workshop legal agreement!");
			}
		}

		private void OnSubmitItemUpdateResult(SubmitItemUpdateResult_t pCallback, bool bIOFailure)
		{
			if (pCallback.m_eResult != EResult.k_EResultOK)
			{
				Debug.LogError("Updating item failed, Error: " + pCallback.m_eResult);
				DialougePanelUI.Instance.Prompt("Upload Error: " + pCallback.m_eResult);
				return;
			}
			Debug.Log("Item Successfully Updated!");
			if (mOnItemUpdtedAction != null)
			{
				mOnItemUpdtedAction();
			}
		}

		private void OnItemDownloaded(DownloadItemResult_t param)
		{
			Debug.Log(string.Concat("Download Result: For Item: ", param.m_nPublishedFileId, " : ", param.m_eResult, " TIME: ", Time.unscaledTime));
		}

		public PublishedFileId_t GetPublishFileID()
		{
			return m_publishedFileID;
		}

		private void SubscribeToItem(PublishedFileId_t item)
		{
			Debug.Log("Subscribing and Downloading item: " + item.ToString() + " TIme: " + Time.unscaledTime);
			SteamUGC.SubscribeItem(item);
			SteamUGC.DownloadItem(item, true);
		}

		public void Reset()
		{
			m_publishedFileID = PublishedFileId_t.Invalid;
			m_UGCUpdateHandle = UGCUpdateHandle_t.Invalid;
			mOnItemUpdtedAction = null;
			mOnCreateAction = null;
		}

		public void OnItemCreatedAction(Action a)
		{
			mOnCreateAction = (Action)Delegate.Combine(mOnCreateAction, a);
		}

		public void OnItemUpdatedAction(Action a)
		{
			mOnItemUpdtedAction = (Action)Delegate.Combine(mOnItemUpdtedAction, a);
		}
	}
}
