using System.Collections;
using System.Collections.Generic;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;

public class LoadingScreenManager : MonoBehaviour
{
	private static Dictionary<ConnectionErrorType, ushort> mConnectionTries = new Dictionary<ConnectionErrorType, ushort>();

	public bool isShowingLoadingScreen;

	public Image img;

	public ParticleSystem part;

	public ParticleSystem crossPart;

	private float sinceStopped;

	[SerializeField]
	private GameObject m_ErrorText;

	private static LoadingScreenManager _instance;

	private bool mIsPlayingDisconnect;

	public static LoadingScreenManager Instance
	{
		get
		{
			return _instance;
		}
	}

	private void Awake()
	{
		_instance = this;
	}

	private void Update()
	{
		if (isShowingLoadingScreen)
		{
			sinceStopped = 0f;
			img.color = new Color(img.color.r, img.color.g, img.color.b, Mathf.Clamp(img.color.a + Time.deltaTime * 2f, 0f, 1f));
			if (!part.isPlaying)
			{
				part.Play();
			}
		}
		else
		{
			sinceStopped += Time.deltaTime;
			if (sinceStopped > 0.5f)
			{
				img.color = new Color(img.color.r, img.color.g, img.color.b, Mathf.Clamp(img.color.a - Time.deltaTime * 2f, 0f, 1f));
			}
			part.Stop();
		}
	}

	private void ConnectionFailed()
	{
		crossPart.Play();
	}

	public void StartLoading()
	{
		isShowingLoadingScreen = true;
	}

	public void StopLoading()
	{
		ChangeLoadingScreenText(string.Empty);
		isShowingLoadingScreen = false;
	}

	public void LoadForSeconds(float time)
	{
		StartCoroutine(Load(time));
	}

	private IEnumerator Load(float time)
	{
		isShowingLoadingScreen = true;
		yield return new WaitForSeconds(time);
		isShowingLoadingScreen = false;
	}

	public void LoadThenFail(ConnectionErrorType type = ConnectionErrorType.None, string extraArgument = "")
	{
		StartCoroutine(LoadFail(2f, type, extraArgument));
	}

	private IEnumerator LoadFail(float time, ConnectionErrorType type, string extraArgument)
	{
		if (mIsPlayingDisconnect)
		{
			yield break;
		}
		mIsPlayingDisconnect = true;
		isShowingLoadingScreen = true;
		MatchmakingHandler.Instance.Disconnect(false);
		yield return new WaitForSecondsRealtime(time / 2f);
		ConnectionFailed();
		bool showError = type != ConnectionErrorType.None;
		if (showError)
		{
			string errorMessageFor = GetErrorMessageFor(type);
			ChangeLoadingScreenText(errorMessageFor + "\n" + extraArgument);
			m_ErrorText.SetActive(true);
			Analytics.CustomEvent(AnalyticsEvents.CONNECTION_ERROR_EVENT, new Dictionary<string, object> { 
			{
				"ErrorType",
				type.ToString()
			} });
			if (!mConnectionTries.ContainsKey(type))
			{
				mConnectionTries.Add(type, 1);
			}
			else
			{
				mConnectionTries[type]++;
			}
		}
		yield return new WaitForSecondsRealtime(time / 2f);
		if (showError)
		{
			float timeToWait = 3f;
			float currTime = 0f;
			while (currTime < timeToWait)
			{
				if (type == ConnectionErrorType.NoConnectionToHost && Input.GetKeyDown(KeyCode.Return))
				{
					string pchURL = "https://support.steampowered.com/kb_article.php?ref=8571-GLVN-8711";
					SteamFriends.ActivateGameOverlayToWebPage(pchURL);
				}
				currTime += Time.unscaledDeltaTime;
				yield return 0;
			}
		}
		isShowingLoadingScreen = false;
		mIsPlayingDisconnect = false;
		GameManager.Instance.RestartGame();
	}

	private string GetErrorMessageFor(ConnectionErrorType type)
	{
		switch (type)
		{
		case ConnectionErrorType.TimeOut:
			return "Host time out";
		case ConnectionErrorType.MatchFull:
			return "Match is full";
		case ConnectionErrorType.NoConnection:
			return "No connection to steam";
		case ConnectionErrorType.NoConnectionToHost:
			return "No connection to the host could be made, please check firewall, press enter for help";
		case ConnectionErrorType.Unknown:
			return "An unknown error occured!";
		case ConnectionErrorType.SteamNotInit:
			return "STEAM is not initialized";
		case ConnectionErrorType.InvalidVersion:
			return "Game version of host does not match local version";
		case ConnectionErrorType.Kicked:
			return "Kicked due to inactivity";
		case ConnectionErrorType.DownloadFailure:
			return "Failed to download custom map";
		case ConnectionErrorType.None:
			return string.Empty;
		default:
			Debug.LogError("Invalid Error Type: " + type.ToString() + " Not Setup!");
			return string.Empty;
		}
	}

	public void ChangeLoadingScreenText(string text)
	{
		m_ErrorText.GetComponent<TextMeshProUGUI>().text = text;
		if (!m_ErrorText.activeSelf)
		{
			m_ErrorText.SetActive(true);
		}
		Debug.Log("Changing LoadingText to: " + text);
	}

	public static ConnectionErrorType GetMostFrequentError()
	{
		ConnectionErrorType result = ConnectionErrorType.None;
		ushort num = 0;
		foreach (KeyValuePair<ConnectionErrorType, ushort> mConnectionTry in mConnectionTries)
		{
			if (mConnectionTry.Value > num)
			{
				result = mConnectionTry.Key;
				num = mConnectionTry.Value;
			}
		}
		return result;
	}
}
