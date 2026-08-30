using TMPro;
using UnityEngine;

public class WinCounterUI : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI[] mPlayerWinTexts;

	[SerializeField]
	private Color[] mPlayerColors;

	private bool isEnabled;

	private void Start()
	{
		int num = PlayerPrefs.GetInt("Show Wins");
		ToggleWinCounterVisibility(num == 1);
		for (int i = 0; i < mPlayerWinTexts.Length; i++)
		{
			mPlayerWinTexts[i].color = mPlayerColors[i];
		}
	}

	public void IncrementWinCounter(Controller winner)
	{
		RefreshWinTexts();
	}

	public void RefreshWinTexts()
	{
		if (ControllerHandler.Instance == null || !isEnabled)
		{
			return;
		}
		for (int i = 0; i < mPlayerWinTexts.Length; i++)
		{
			if (ControllerHandler.Instance.players.Count <= i || ControllerHandler.Instance.players[i] == null)
			{
				mPlayerWinTexts[i].gameObject.SetActive(false);
				continue;
			}
			mPlayerWinTexts[i].gameObject.SetActive(true);
			mPlayerWinTexts[i].text = ControllerHandler.Instance.players[i].GetComponent<CharacterStats>().wins.ToString();
		}
	}

	public void ToggleWinCounterVisibility(bool isVisible)
	{
		isEnabled = isVisible;
		if (isVisible)
		{
			RefreshWinTexts();
			return;
		}
		for (int i = 0; i < mPlayerWinTexts.Length; i++)
		{
			mPlayerWinTexts[i].gameObject.SetActive(false);
		}
	}
}
