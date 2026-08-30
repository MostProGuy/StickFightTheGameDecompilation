using System.Collections;
using TMPro;
using UnityEngine;

public class OnlinePlayerUI : MonoBehaviour
{
	[SerializeField]
	private Color[] mPlayerColors;

	[SerializeField]
	private TextMeshProUGUI[] mPlayerTexts;

	private ConnectedClientData[] mClients;

	private bool mIsStaying;

	private void Awake()
	{
		for (int i = 0; i < mPlayerTexts.Length; i++)
		{
			mPlayerTexts[i].color = mPlayerColors[i];
		}
	}

	private void Update()
	{
		if (!mIsStaying)
		{
			return;
		}
		int num = mClients.Length;
		for (int i = 0; i < num; i++)
		{
			ConnectedClientData connectedClientData = mClients[i];
			if (connectedClientData == null || !connectedClientData.ClientID.IsValid() || connectedClientData.PlayerObject == null)
			{
				mPlayerTexts[i].text = string.Empty;
				continue;
			}
			mPlayerTexts[i].text = connectedClientData.PlayerName;
			CodeStateAnimation component = mPlayerTexts[i].GetComponent<CodeStateAnimation>();
			GameObject gameObject = connectedClientData.PlayerObject.GetComponentInChildren<Torso>().gameObject;
			component.state1 = true;
			if (gameObject == null)
			{
				break;
			}
			component.transform.position = gameObject.transform.position + Vector3.up * 1.5f;
			if (gameObject == null)
			{
				component.state1 = false;
			}
		}
	}

	public void OnUpdated()
	{
		Populate();
	}

	public void Init()
	{
		if (!MatchmakingHandler.IsNetworkMatch)
		{
			Debug.LogError("Trying to init onlineUI when playing locally!");
			Hide();
		}
		mClients = Object.FindObjectOfType<MultiplayerManager>().ConnectedClients;
		Populate();
	}

	public void TextStay()
	{
		if (mClients != null)
		{
			mIsStaying = true;
		}
	}

	private void Populate()
	{
		if (mClients == null)
		{
			return;
		}
		mIsStaying = false;
		int num = mClients.Length;
		for (int i = 0; i < num; i++)
		{
			ConnectedClientData connectedClientData = mClients[i];
			if (connectedClientData == null || !connectedClientData.ClientID.IsValid() || connectedClientData.PlayerObject == null)
			{
				mPlayerTexts[i].text = string.Empty;
				continue;
			}
			mPlayerTexts[i].text = connectedClientData.PlayerName + connectedClientData.Ping;
			StartCoroutine(ShowText(mPlayerTexts[i].GetComponent<CodeStateAnimation>(), connectedClientData.PlayerObject.GetComponentInChildren<Torso>().gameObject));
		}
	}

	private IEnumerator ShowText(CodeStateAnimation anim, GameObject target)
	{
		yield return new WaitForSecondsRealtime(0.5f);
		float f = 0f;
		anim.state1 = true;
		while (f < 2.5f)
		{
			f += Time.unscaledDeltaTime;
			if (target == null)
			{
				break;
			}
			anim.transform.position = target.transform.position + Vector3.up * 1.5f;
			yield return null;
		}
		f = 0f;
		anim.state1 = false;
		while (f < 0.5f)
		{
			f += Time.unscaledDeltaTime;
			if (target == null)
			{
				break;
			}
			anim.transform.position = target.transform.position + Vector3.up * 1.5f;
			yield return null;
		}
		if (target == null)
		{
			anim.state1 = false;
		}
	}

	private void Hide()
	{
		TextMeshProUGUI[] array = mPlayerTexts;
		foreach (TextMeshProUGUI textMeshProUGUI in array)
		{
			textMeshProUGUI.text = string.Empty;
		}
	}
}
