using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsButton : MonoBehaviour
{
	public string[] values;

	private int currentIndex;

	private TextMeshProUGUI text;

	private OptionsHolder optionsHolder;

	private bool mLocked;

	public bool Locked
	{
		get
		{
			return mLocked;
		}
	}

	private void Start()
	{
		optionsHolder = base.transform.root.GetComponent<OptionsHolder>();
		text = GetComponentInChildren<TextMeshProUGUI>();
		OptionsHolder.CheckOptions(optionsHolder);
		if (PlayerPrefs.HasKey(base.transform.parent.name))
		{
			currentIndex = PlayerPrefs.GetInt(base.transform.parent.name);
		}
		currentIndex = Mathf.Clamp(currentIndex, 0, values.Length - 1);
		text.text = values[currentIndex];
		base.transform.root.GetComponent<AudioManager>().SetMixers();
	}

	public void Init()
	{
		if (MatchmakingHandler.IsNetworkMatch)
		{
			if (!MultiplayerManager.IsAllowedToChangeOptions)
			{
				LockButton();
			}
			else
			{
				UnlockButton();
			}
		}
		else
		{
			UnlockButton();
		}
	}

	private void LockButton()
	{
		Image image = null;
		GameObject gameObject = FindObject("Lock");
		if ((bool)gameObject)
		{
			image = gameObject.GetComponent<Image>();
		}
		if ((bool)image)
		{
			text.gameObject.SetActive(false);
			image.gameObject.SetActive(true);
			mLocked = true;
		}
	}

	private void UnlockButton()
	{
		Image image = null;
		GameObject gameObject = FindObject("Lock");
		if ((bool)gameObject)
		{
			image = gameObject.GetComponent<Image>();
		}
		if ((bool)image)
		{
			image.gameObject.SetActive(false);
		}
		text.gameObject.SetActive(true);
		mLocked = false;
	}

	private GameObject FindObject(string name)
	{
		Transform[] componentsInChildren = base.gameObject.GetComponentsInChildren<Transform>(true);
		Transform[] array = componentsInChildren;
		foreach (Transform transform in array)
		{
			if (transform.name.ToLower() == name.ToLower())
			{
				return transform.gameObject;
			}
		}
		return null;
	}

	public void MoveIndexRight()
	{
		currentIndex++;
		if (currentIndex >= values.Length)
		{
			currentIndex = 0;
		}
		text.text = values[currentIndex];
		PlayerPrefs.SetInt(base.transform.parent.name, currentIndex);
		OptionsHolder.CheckOptions(optionsHolder);
		base.transform.root.GetComponent<AudioManager>().SetMixers();
	}

	public void MoveIndexLeft()
	{
		currentIndex--;
		if (currentIndex < 0)
		{
			currentIndex = values.Length - 1;
		}
		text.text = values[currentIndex];
		PlayerPrefs.SetInt(base.transform.parent.name, currentIndex);
		OptionsHolder.CheckOptions(optionsHolder);
		base.transform.root.GetComponent<AudioManager>().SetMixers();
	}

	public void OpenWeaponScreen()
	{
		if (!mLocked)
		{
			Object.FindObjectOfType<PauseManager>().OpenWeaponSelect();
		}
	}
}
