using InControl;
using UnityEngine;

public class KeybindingButtonHandler : MonoBehaviour
{
	public bool WaitingForInput;

	public GameObject KeybindHud;

	public GameObject WaitingHud;

	private PlayerActions Action;

	private bool altKey;

	private void Start()
	{
	}

	private void Update()
	{
		if (WaitingForInput)
		{
			GetInput();
		}
	}

	private void GetInput()
	{
		KeyInfo[] keyList = KeyInfo.KeyList;
		for (int i = 0; i < keyList.Length; i++)
		{
			KeyInfo keyInfo = keyList[i];
			if (keyInfo.Key != Key.Escape && keyInfo.Key != Key.Return && keyInfo.IsPressed)
			{
				SetKeybinding(new KeybindingInfo(keyInfo.Key), altKey);
				ExitWaitingState();
				return;
			}
		}
		for (int j = 0; j < 9; j++)
		{
			if (Input.GetMouseButton(j))
			{
				int num = j + 1;
				if (j > 2)
				{
					num += 6;
				}
				SetKeybinding(new KeybindingInfo((Mouse)num), altKey);
				ExitWaitingState();
				break;
			}
		}
	}

	private void SetKeybinding(KeybindingInfo keyInfo, bool altKey)
	{
		ControllerHandler instance = ControllerHandler.Instance;
		foreach (Controller activePlayer in instance.ActivePlayers)
		{
			if (activePlayer.PlayerActions != null && activePlayer.PlayerActions.InputType == InputType.Keyboard)
			{
				activePlayer.PlayerActions.RebindAction(Action, keyInfo, altKey);
				return;
			}
		}
		CharacterActions.SaveKeybinding(Action, keyInfo, altKey);
	}

	public void ExitWaitingState()
	{
		WaitingForInput = false;
		KeybindHud.SetActive(true);
		WaitingHud.SetActive(false);
	}

	private void WaitForInput(PlayerActions action)
	{
		WaitingForInput = true;
		Action = action;
		KeybindHud.SetActive(false);
		WaitingHud.SetActive(true);
	}

	public void SetupAction(int action)
	{
		SetupAction(action, false);
	}

	public void SetupActionAlt(int action)
	{
		SetupAction(action, true);
	}

	public void SetupAction(int action, bool alt)
	{
		if (action < 0 || action >= 8)
		{
			Debug.LogError("Invalid keybinding");
			return;
		}
		altKey = alt;
		WaitForInput((PlayerActions)action);
	}

	public void ResetBindings()
	{
		ControllerHandler instance = ControllerHandler.Instance;
		bool flag = false;
		foreach (Controller activePlayer in instance.ActivePlayers)
		{
			if (activePlayer.PlayerActions != null && activePlayer.PlayerActions.InputType == InputType.Keyboard)
			{
				activePlayer.PlayerActions.ResetKeybindingsAndClear();
				flag = true;
			}
		}
		if (!flag)
		{
			CharacterActions.ResetKeybindings();
		}
		if ((bool)KeybindHud)
		{
			KeybindingText[] componentsInChildren = KeybindHud.GetComponentsInChildren<KeybindingText>();
			foreach (KeybindingText keybindingText in componentsInChildren)
			{
				keybindingText.UpdateText();
			}
		}
	}
}
