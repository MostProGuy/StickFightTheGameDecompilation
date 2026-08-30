using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using InControl;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.EventSystems;
using UnityEngine.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
	public static bool isPaused;

	private Camera cam;

	private PostProcessingBehaviour post;

	public PostProcessingProfile blur;

	public PostProcessingProfile standard;

	public GameObject pauseMenu;

	public GameObject optionsMenu;

	public GameObject gameOptionsMenu;

	public GameObject weaponSelection;

	public GameObject mapSelection;

	public GameObject keybindingMenu;

	private CodeStateAnimation weaponAnim;

	private CodeStateAnimation mapAnim;

	public CodeStateAnimation pauseAnim;

	public CodeStateAnimation optionsAnim;

	public CodeStateAnimation gameSettingsAnim;

	public CodeStateAnimation keybindingAnim;

	public bool isInOptionsMenu;

	public bool isInKeybindingMenu;

	public static bool usedKeyboard;

	public float sinceTransition;

	private OptionsButton[] mOptionButtons;

	private OptionsButton[] mGameOptionButtons;

	private OptionsButton[] mKeybindingButtons;

	public Button HideOnUnpause;

	private Action m_OnOpenMapSelectAction;

	private Action m_OnCloseMapSelectAction;

	private void Start()
	{
		cam = Camera.main;
		post = cam.GetComponent<PostProcessingBehaviour>();
		mOptionButtons = optionsMenu.GetComponentsInChildren<OptionsButton>();
		mGameOptionButtons = gameOptionsMenu.GetComponentsInChildren<OptionsButton>();
		mKeybindingButtons = keybindingMenu.GetComponentsInChildren<OptionsButton>();
		weaponAnim = weaponSelection.GetComponent<CodeStateAnimation>();
		mapAnim = mapSelection.GetComponent<CodeStateAnimation>();
		SetButtonState(weaponAnim.gameObject, false);
		SetButtonState(mapAnim.gameObject, false);
	}

	private void Update()
	{
		sinceTransition += Time.unscaledDeltaTime;
		CheckKeyboardInput();
		CheckControllerInput();
		if (Input.GetKey(KeyCode.Mouse0))
		{
			usedKeyboard = true;
		}
	}

	private void CheckKeyboardInput()
	{
		if (ChatManager.isTyping && !isPaused)
		{
			return;
		}
		if (Input.GetKeyDown(KeyCode.Escape) && !mapAnim.state1)
		{
			usedKeyboard = true;
			CloseMapSelect();
			if (usedKeyboard)
			{
				EventSystem.current.SetSelectedGameObject(null);
			}
		}
		else if (Input.GetKeyDown(KeyCode.Escape) && !weaponAnim.state1)
		{
			usedKeyboard = true;
			CloseWeaponSelect();
			if (usedKeyboard)
			{
				EventSystem.current.SetSelectedGameObject(null);
			}
		}
		else if (Input.GetKeyDown(KeyCode.Escape) && (isInOptionsMenu || isInKeybindingMenu))
		{
			usedKeyboard = true;
			KeybindingButtonHandler component = keybindingMenu.GetComponent<KeybindingButtonHandler>();
			if (component.WaitingForInput)
			{
				component.ExitWaitingState();
				return;
			}
			BackToPause();
			if (usedKeyboard)
			{
				EventSystem.current.SetSelectedGameObject(null);
			}
		}
		else
		{
			if (!Input.GetKeyDown(KeyCode.Escape))
			{
				return;
			}
			usedKeyboard = true;
			if (!isPaused)
			{
				Pause();
				post.profile = blur;
				if (!MatchmakingHandler.IsNetworkMatch)
				{
					TimeHandler.pauseTime = 0f;
				}
			}
			else
			{
				Resume();
			}
		}
	}

	private void CheckControllerInput()
	{
		ReadOnlyCollection<InputDevice> devices = InputManager.Devices;
		foreach (InputDevice item in devices)
		{
			if ((item.CommandWasPressed || item.Action2.WasPressed) && !mapAnim.state1 && isPaused)
			{
				usedKeyboard = false;
				CloseMapSelect();
			}
			else if ((item.CommandWasPressed || item.Action2.WasPressed) && !weaponAnim.state1 && isPaused)
			{
				usedKeyboard = false;
				CloseWeaponSelect();
			}
			else if ((item.CommandWasPressed || item.Action2.WasPressed) && (isInOptionsMenu || isInKeybindingMenu) && isPaused)
			{
				usedKeyboard = false;
				BackToPause();
			}
			else if (item.CommandWasPressed)
			{
				usedKeyboard = false;
				if (!isPaused)
				{
					Pause();
					post.profile = blur;
					if (!MatchmakingHandler.IsNetworkMatch)
					{
						TimeHandler.pauseTime = 0f;
					}
				}
				else
				{
					Resume();
				}
			}
			else if (item.Action2.WasPressed && isPaused)
			{
				usedKeyboard = false;
				Resume();
			}
		}
	}

	public void GoToLevelEditor()
	{
		if (isPaused)
		{
			Time.timeScale = 1f;
			isPaused = false;
			if (MatchmakingHandler.IsNetworkMatch)
			{
				MatchmakingHandler.Instance.Disconnect(false);
			}
			StartCoroutine(LowerSoundThen(() =>
			{
				SceneManager.LoadScene("LevelEditor");
			}));
		}
	}

	private IEnumerator LowerSoundThen(Action a)
	{
		MusicHandler handler = MusicHandler.Instance;
		while (!handler.FadeOutVolume())
		{
			yield return null;
		}
		a();
	}

	public void AddOnMapSelectOpenAction(Action a)
	{
		m_OnOpenMapSelectAction = (Action)Delegate.Combine(m_OnOpenMapSelectAction, a);
	}

	public void AddOnMapSelectCloseAction(Action a)
	{
		m_OnCloseMapSelectAction = (Action)Delegate.Combine(m_OnCloseMapSelectAction, a);
	}

	public void OpenMapSelect()
	{
		if (!MatchmakingHandler.IsNetworkMatch || MultiplayerManager.IsServer)
		{
			FixShit(mapAnim.gameObject);
			mapAnim.state1 = false;
			isInOptionsMenu = false;
			isInKeybindingMenu = false;
			if (m_OnOpenMapSelectAction != null)
			{
				m_OnOpenMapSelectAction();
			}
		}
	}

	public void CloseMapSelect()
	{
		FixShit(optionsMenu);
		mapAnim.state1 = true;
		isInOptionsMenu = true;
		if (m_OnCloseMapSelectAction != null)
		{
			m_OnCloseMapSelectAction();
		}
	}

	public void OpenWeaponSelect()
	{
		FixShit(weaponAnim.gameObject);
		weaponAnim.state1 = false;
		isInOptionsMenu = false;
		isInKeybindingMenu = false;
	}

	public void CloseWeaponSelect()
	{
		FixShit(optionsMenu);
		weaponAnim.state1 = true;
		isInOptionsMenu = true;
	}

	public void Resume()
	{
		if (isPaused && !isInOptionsMenu && !isInKeybindingMenu)
		{
			if ((bool)HideOnUnpause)
			{
				HideOnUnpause.gameObject.SetActive(false);
			}
			pauseAnim.state1 = true;
			optionsAnim.state1 = true;
			keybindingAnim.state1 = true;
			StartCoroutine(EndPause());
			isPaused = false;
		}
	}

	public void Pause()
	{
		FixShit(pauseMenu);
		BaseEventData eventData = new BaseEventData(EventSystem.current);
		Button[] componentsInChildren = pauseMenu.GetComponentsInChildren<Button>();
		foreach (Button button in componentsInChildren)
		{
			button.OnDeselect(eventData);
		}
		pauseAnim.state1 = false;
		optionsAnim.state1 = true;
		keybindingAnim.state1 = true;
		isPaused = true;
	}

	public void Options()
	{
		if (isPaused)
		{
			optionsMenu.SetActive(true);
			FixShit(optionsMenu);
			keybindingMenu.SetActive(false);
			OptionsButton[] array = mOptionButtons;
			foreach (OptionsButton optionsButton in array)
			{
				optionsButton.Init();
			}
			isInOptionsMenu = true;
			pauseAnim.state1 = true;
			optionsAnim.state1 = false;
			keybindingAnim.state1 = true;
		}
	}

	public void GameSettings()
	{
		if (isPaused)
		{
			gameOptionsMenu.SetActive(true);
			FixShit(gameOptionsMenu);
			optionsMenu.SetActive(false);
			OptionsButton[] array = mGameOptionButtons;
			foreach (OptionsButton optionsButton in array)
			{
				optionsButton.Init();
			}
			isInOptionsMenu = true;
			pauseAnim.state1 = true;
			optionsAnim.state1 = true;
			gameSettingsAnim.state1 = false;
		}
	}

	public void Keybindings()
	{
		if (isPaused)
		{
			keybindingMenu.SetActive(true);
			FixShit(keybindingMenu);
			optionsMenu.SetActive(false);
			OptionsButton[] array = mKeybindingButtons;
			foreach (OptionsButton optionsButton in array)
			{
				optionsButton.Init();
			}
			isInKeybindingMenu = true;
			pauseAnim.state1 = true;
			optionsAnim.state1 = true;
			keybindingAnim.state1 = false;
		}
	}

	public void BackToPause()
	{
		if (isPaused)
		{
			FixShit(pauseMenu);
			isInOptionsMenu = false;
			isInKeybindingMenu = false;
			pauseAnim.state1 = false;
			optionsAnim.state1 = true;
			keybindingAnim.state1 = true;
			gameSettingsAnim.state1 = true;
		}
	}

	private void FixShit(GameObject selectObject)
	{
		if (!usedKeyboard)
		{
			SelectMe[] componentsInChildren = selectObject.GetComponentsInChildren<SelectMe>();
			foreach (SelectMe selectMe in componentsInChildren)
			{
				selectMe.Select();
			}
		}
		if (!usedKeyboard)
		{
		}
	}

	public void Restart()
	{
		if (isPaused)
		{
			if (Application.isEditor && MatchmakingHandler.HasTriedJoiningOnline && !MatchmakingHandler.HasSuccededJoining)
			{
				ConnectionErrorType mostFrequentError = LoadingScreenManager.GetMostFrequentError();
				Analytics.CustomEvent(AnalyticsEvents.NEVER_GOT_TO_PLAY_ONLINE, new Dictionary<string, object>
				{
					{ "Success", 0 },
					{
						"Error",
						mostFrequentError.ToString()
					}
				});
			}
			GameManager.Instance.RestartGame();
		}
	}

	public void Quit()
	{
		if (isPaused)
		{
			if (MatchmakingHandler.HasTriedJoiningOnline && !MatchmakingHandler.HasSuccededJoining)
			{
				ConnectionErrorType mostFrequentError = LoadingScreenManager.GetMostFrequentError();
				Analytics.CustomEvent(AnalyticsEvents.NEVER_GOT_TO_PLAY_ONLINE, new Dictionary<string, object>
				{
					{ "Success", 0 },
					{
						"Error",
						mostFrequentError.ToString()
					}
				});
			}
			Application.Quit();
		}
	}

	private IEnumerator StartPause()
	{
		yield return new WaitForSecondsRealtime(0.4f);
	}

	private IEnumerator EndPause()
	{
		yield return new WaitForSecondsRealtime(0.3f);
		if (!isPaused)
		{
			post.profile = standard;
			TimeHandler.pauseTime = 1f;
		}
	}

	private IEnumerator ToOptions()
	{
		yield return new WaitForSecondsRealtime(0.7f);
	}

	private IEnumerator ToPause()
	{
		yield return new WaitForSecondsRealtime(0.7f);
	}

	private void SetButtonState(GameObject obj, bool state)
	{
		Button[] componentsInChildren = obj.GetComponentsInChildren<Button>();
		foreach (Button button in componentsInChildren)
		{
			if (state)
			{
				SelectMe componentInChildren = button.GetComponentInChildren<SelectMe>();
				if ((bool)componentInChildren)
				{
					componentInChildren.Select();
				}
			}
		}
	}
}
