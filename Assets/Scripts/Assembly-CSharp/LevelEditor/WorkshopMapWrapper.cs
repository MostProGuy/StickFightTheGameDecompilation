using System;
using Steamworks;
using UnityEngine;

namespace LevelEditor
{
	public class WorkshopMapWrapper
	{
		protected CallResult<RemoteStorageDownloadUGCResult_t> m_DownloadPreviewCallResult;

		public CSteamID AuthorID { get; private set; }

		public string LevelName { get; private set; }

		public string Description { get; private set; }

		public string DateTime { get; private set; }

		public PublishedFileId_t PublishID { get; private set; }

		public byte[] PreviewFileData { get; private set; }

		public ERemoteStoragePublishedFileVisibility Visibility { get; private set; }

		public WorkshopMapWrapper(string mapName, string description, uint dateInSeconds, PublishedFileId_t pID, ulong authorID, UGCHandle_t handle, int fileSize, ERemoteStoragePublishedFileVisibility visibility)
		{
			LevelName = mapName;
			PublishID = pID;
			AuthorID = new CSteamID(authorID);
			Description = description;
			Visibility = visibility;
			DateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(dateInSeconds).ToLocalTime().ToShortDateString();
			m_DownloadPreviewCallResult = CallResult<RemoteStorageDownloadUGCResult_t>.Create(OnPreviewImageDownloaded);
			SteamAPICall_t hAPICall = SteamRemoteStorage.UGCDownload(handle, 0u);
			m_DownloadPreviewCallResult.Set(hAPICall);
		}

		private void OnPreviewImageDownloaded(RemoteStorageDownloadUGCResult_t param, bool bIOFailure)
		{
			if (bIOFailure)
			{
				Debug.LogError("Biofail");
			}
			else if (param.m_eResult == EResult.k_EResultOK)
			{
				int nSizeInBytes = param.m_nSizeInBytes;
				UGCHandle_t hFile = param.m_hFile;
				byte[] array = new byte[nSizeInBytes];
				int num = SteamRemoteStorage.UGCRead(hFile, array, nSizeInBytes, 0u, EUGCReadAction.k_EUGCRead_ContinueReadingUntilFinished);
				Debug.Log("UGC Read: " + num + " FIle Size: " + nSizeInBytes);
				PreviewFileData = array;
			}
			else
			{
				Debug.LogError("Error downloading previewImage: " + param.m_eResult);
			}
		}
	}
}
