using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace SteamDataSuite
{
	public class SteamDataAttribution : MonoBehaviour
	{
		private static SteamDataAttribution _instance;

		private static bool singletonErrorWasThrown;

		[Tooltip("Paste your license key here")]
		[SerializeField]
		private string licenseKey = "770cca59813a477ba248772569d9aebd";

		[Tooltip("Sends firstRun milestone on very first startup of the app")]
		[SerializeField]
		private bool sendFirstRunEvent = true;

		[Tooltip("Enable debugMode if you want to log errors")]
		[SerializeField]
		private bool debugMode = true;

		public static void PostMilestone(string milestone)
		{
			if (_instance == null)
			{
				_instance = Object.FindObjectOfType<SteamDataAttribution>();
				if (_instance == null)
				{
					if (!singletonErrorWasThrown)
					{
						Debug.LogError("SteamDataSuite ERROR: Please add the SteamDataAttribution prefab to your startup scene");
						singletonErrorWasThrown = true;
					}
					return;
				}
			}
			_instance.SendMilestoneForm(milestone);
		}

		private void Awake()
		{
			if (_instance != null && _instance != this)
			{
				Object.Destroy(base.gameObject);
				return;
			}
			Object.DontDestroyOnLoad(base.gameObject);
			StartCoroutine(PostPendingMilestones());
			if (sendFirstRunEvent)
			{
				SendMilestoneForm("firstrun");
			}
		}

		private IEnumerator PostPendingMilestones()
		{
			string[] knownMilestones = GetKnownMilestones();
			foreach (string milestone in knownMilestones)
			{
				int value = PlayerPrefs.GetInt(milestone, 0);
				if (value > 0)
				{
					IEnumerator iter = HandleFormPost(milestone, false);
					while (iter.MoveNext())
					{
						yield return iter.Current;
					}
				}
			}
			PlayerPrefs.Save();
		}

		private void SendMilestoneForm(string milestone)
		{
			if (!PlayerPrefs.HasKey(milestone))
			{
				PlayerPrefs.SetInt(milestone, 1);
				AddKnownMilestone(milestone);
				StartCoroutine(HandleFormPost(milestone, true));
			}
		}

		private IEnumerator HandleFormPost(string milestone, bool savePlayerPrefs)
		{
			UnityWebRequest www = UnityWebRequest.Post(multipartFormSections: new List<IMultipartFormSection>(), uri: GetBackendUrl(milestone));
			www.SetRequestHeader("cache-control", "no-cache");
			yield return www.Send();
			if (www.responseCode == 400)
			{
				if (debugMode)
				{
					Debug.Log("SteamDataSuite ERROR: Milestone does not exist? milestone: " + milestone);
				}
			}
			else if (www.isNetworkError)
			{
				if (debugMode)
				{
					Debug.Log("SteamDataSuite ERROR: " + www.error);
				}
			}
			else if (www.isDone && www.responseCode == 200)
			{
				PlayerPrefs.SetInt(milestone, 0);
			}
			www.Dispose();
			if (savePlayerPrefs)
			{
				PlayerPrefs.Save();
			}
		}

		private string GetBackendUrl(string milestone)
		{
			return "http://ldns.co/ca/" + licenseKey + "?m=" + milestone;
		}

		private string[] GetKnownMilestones()
		{
			string text = PlayerPrefs.GetString("milestones", string.Empty);
			if (string.IsNullOrEmpty(text))
			{
				return new string[0];
			}
			return text.Split(',');
		}

		private void AddKnownMilestone(string milestone)
		{
			string text = PlayerPrefs.GetString("milestones", string.Empty);
			if (!string.IsNullOrEmpty(text))
			{
				if (!text.Contains(milestone))
				{
					text = text + "," + milestone;
				}
			}
			else
			{
				text = milestone;
			}
			PlayerPrefs.SetString("milestones", text);
		}

		[ContextMenu("Reset Milestones")]
		private void ResetMilestones()
		{
			string[] knownMilestones = GetKnownMilestones();
			foreach (string key in knownMilestones)
			{
				PlayerPrefs.DeleteKey(key);
			}
			PlayerPrefs.Save();
		}

		[ContextMenu("Clear all playerprefs")]
		private void ClearAllPlayerprefs()
		{
			PlayerPrefs.DeleteAll();
		}
	}
}
