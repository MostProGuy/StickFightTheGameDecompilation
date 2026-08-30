using System.Collections.Generic;
using InControl;
using UnityEngine;

public class CharacterActions : PlayerActionSet
{
	private PlayerAction Left;

	private PlayerAction Right;

	private PlayerAction Up;

	private PlayerAction Down;

	private PlayerAction AimLeft;

	private PlayerAction AimRight;

	private PlayerAction AimUp;

	private PlayerAction AimDown;

	private PlayerAction Jump;

	private PlayerAction Jump2;

	public PlayerAction PunchOrFire;

	public PlayerAction PunchOrFireWithKeyBoard;

	public PlayerAction Block;

	public PlayerAction Throw;

	public TwoAxisInputControl Movement;

	public TwoAxisInputControl Aiming;

	private const int MouseKeyOffset = 1000;

	public const string Alt_Mod = "_Alt";

	public const string Up_Key = "Key:Up";

	public const string Down_Key = "Key:Down";

	public const string Left_Key = "Key:Left";

	public const string Right_Key = "Key:Right";

	public const string Throw_Key = "Key:Throw";

	public const string Punch_Key = "Key:PunchOrFire";

	public const string Block_Key = "Key:Block";

	public const string Jump_Key = "Key:Jump";

	public static string[] ActionPrefs = new string[8] { "Key:Up", "Key:Down", "Key:Left", "Key:Right", "Key:Throw", "Key:PunchOrFire", "Key:Block", "Key:Jump" };

	private static readonly Dictionary<string, KeybindingInfo> DefaultKeybindings = new Dictionary<string, KeybindingInfo>
	{
		{
			"Key:Up",
			new KeybindingInfo(Key.W)
		},
		{
			"Key:Down",
			new KeybindingInfo(Key.S)
		},
		{
			"Key:Left",
			new KeybindingInfo(Key.A)
		},
		{
			"Key:Right",
			new KeybindingInfo(Key.D)
		},
		{
			"Key:Throw",
			new KeybindingInfo(Key.F)
		},
		{
			"Key:PunchOrFire",
			new KeybindingInfo(Mouse.LeftButton)
		},
		{
			"Key:Block",
			new KeybindingInfo(Mouse.RightButton)
		},
		{
			"Key:Jump",
			new KeybindingInfo(Key.W)
		},
		{
			"Key:Up_Alt",
			new KeybindingInfo(Key.UpArrow)
		},
		{
			"Key:Down_Alt",
			new KeybindingInfo(Key.DownArrow)
		},
		{
			"Key:Left_Alt",
			new KeybindingInfo(Key.LeftArrow)
		},
		{
			"Key:Right_Alt",
			new KeybindingInfo(Key.RightArrow)
		},
		{
			"Key:Throw_Alt",
			new KeybindingInfo(Key.B)
		},
		{
			"Key:PunchOrFire_Alt",
			new KeybindingInfo(Key.C)
		},
		{
			"Key:Block_Alt",
			new KeybindingInfo(Key.V)
		},
		{
			"Key:Jump_Alt",
			new KeybindingInfo(Key.UpArrow)
		}
	};

	private InputType mInputType;

	public bool JumpWasPressed
	{
		get
		{
			if (Jump2 == null)
			{
				return Jump.WasPressed;
			}
			return Jump.WasPressed || Jump2.WasPressed;
		}
	}

	public InputType InputType
	{
		get
		{
			if (mInputType == InputType.Any)
			{
				switch (LastInputType)
				{
				case BindingSourceType.None:
					return InputType.Keyboard;
				case BindingSourceType.DeviceBindingSource:
					return InputType.Controller;
				case BindingSourceType.KeyBindingSource:
					return InputType.Keyboard;
				case BindingSourceType.MouseBindingSource:
					return InputType.Keyboard;
				case BindingSourceType.UnknownDeviceBindingSource:
					return InputType.Controller;
				}
			}
			return mInputType;
		}
	}

	public CharacterActions()
	{
		Left = CreatePlayerAction("Move Left");
		Right = CreatePlayerAction("Move Right");
		Up = CreatePlayerAction("Move Up");
		Down = CreatePlayerAction("Move Down");
		Jump = CreatePlayerAction("Jump");
		Jump2 = CreatePlayerAction("Jump2");
		PunchOrFireWithKeyBoard = CreatePlayerAction("PunchWithKeyBoard");
		PunchOrFire = CreatePlayerAction("Fire");
		Block = CreatePlayerAction("Block");
		Throw = CreatePlayerAction("Throw");
		Movement = CreateTwoAxisPlayerAction(Left, Right, Down, Up);
		AimLeft = CreatePlayerAction("AimLeft");
		AimRight = CreatePlayerAction("AimRight");
		AimUp = CreatePlayerAction("AimUp");
		AimDown = CreatePlayerAction("AimDown");
		Aiming = CreateTwoAxisPlayerAction(AimLeft, AimRight, AimDown, AimUp);
	}

	private static bool IsPrefAnyOfKeys(List<KeyCode> KeyCodes, string currPref)
	{
		KeybindingInfo keybinding = GetKeybinding(currPref);
		if (keybinding.key == -1)
		{
			return false;
		}
		KeyInfo[] keyList = KeyInfo.KeyList;
		for (int i = 0; i < keyList.Length; i++)
		{
			KeyInfo keyInfo = keyList[i];
			if (keyInfo.Key != (Key)keybinding.key)
			{
				continue;
			}
			KeyCode[] keyCodes = keyInfo.GetKeyCodes();
			foreach (KeyCode item in keyCodes)
			{
				if (KeyCodes.Contains(item))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool IsAnyKeybindPressed(List<KeyCode> IgnoreKeys = null)
	{
		string[] actionPrefs = ActionPrefs;
		foreach (string text in actionPrefs)
		{
			if (IgnoreKeys == null || !IsPrefAnyOfKeys(IgnoreKeys, text))
			{
				if (IsKeybindPressed(text))
				{
					return true;
				}
				if (IsKeybindPressed(text + "_Alt"))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool IsKeybindPressed(string pref)
	{
		KeybindingInfo keybinding = GetKeybinding(pref);
		if (keybinding.key == -1)
		{
			return false;
		}
		KeyInfo[] keyList = KeyInfo.KeyList;
		for (int i = 0; i < keyList.Length; i++)
		{
			KeyInfo keyInfo = keyList[i];
			if (keyInfo.Key == (Key)keybinding.key)
			{
				return keyInfo.IsPressed;
			}
		}
		return false;
	}

	public static string GetKeybindingString(string pref)
	{
		int num = PlayerPrefs.GetInt(pref, -1);
		if (num != -1)
		{
			if (num > 1000)
			{
				num -= 1000;
				Mouse mouse = (Mouse)num;
				return mouse.ToString();
			}
			KeyInfo[] keyList = KeyInfo.KeyList;
			for (int i = 0; i < keyList.Length; i++)
			{
				KeyInfo keyInfo = keyList[i];
				if (keyInfo.Key == (Key)num)
				{
					return keyInfo.Key.ToString();
				}
			}
		}
		return GetDefaultKeybindingString(pref);
	}

	public static string GetDefaultKeybindingString(string pref)
	{
		if (DefaultKeybindings.ContainsKey(pref))
		{
			KeybindingInfo keybindingInfo = DefaultKeybindings[pref];
			if (keybindingInfo.mouse)
			{
				return ((Mouse)keybindingInfo.key/*cast due to constrained. prefix*/).ToString();
			}
			return (!keybindingInfo.mouse) ? ((Key)keybindingInfo.key/*cast due to constrained. prefix*/).ToString() : ((Mouse)keybindingInfo.key/*cast due to constrained. prefix*/).ToString();
		}
		return string.Empty;
	}

	public static KeybindingInfo GetKeybinding(string pref)
	{
		int num = PlayerPrefs.GetInt(pref, -1);
		if (num != -1)
		{
			bool mouse = num > 1000;
			if (num > 1000)
			{
				num -= 1000;
			}
			KeyInfo[] keyList = KeyInfo.KeyList;
			foreach (KeyInfo keyInfo in keyList)
			{
				if (keyInfo.Key == (Key)num)
				{
					return new KeybindingInfo(num, mouse);
				}
			}
		}
		if (DefaultKeybindings.ContainsKey(pref))
		{
			return DefaultKeybindings[pref];
		}
		Debug.LogError("Key does not exist");
		return new KeybindingInfo(-1);
	}

	public static CharacterActions CreateWithControllerBindings()
	{
		CharacterActions characterActions = new CharacterActions();
		AddControllerBindings(characterActions);
		characterActions.SetInputType(InputType.Controller);
		return characterActions;
	}

	private static void SetupBind(string PrefKeyID, PlayerAction action)
	{
		SetupBind(PrefKeyID, action, DefaultKeybindings[PrefKeyID]);
	}

	private static void SetupBind(string PrefKeyID, PlayerAction action, KeybindingInfo key)
	{
		int num = PlayerPrefs.GetInt(PrefKeyID, -1);
		bool flag = num > -1 && num < 1000;
		bool flag2 = num > 1000;
		if (!flag && (key.mouse || flag2))
		{
			if (num > -1)
			{
				if (num > 1000)
				{
					num -= 1000;
				}
				action.AddDefaultBinding((Mouse)num);
			}
			else
			{
				action.AddDefaultBinding((Mouse)key.key);
			}
		}
		else if (num > -1)
		{
			action.AddDefaultBinding((Key)num);
		}
		else
		{
			action.AddDefaultBinding((Key)key.key);
		}
	}

	public static CharacterActions CreateWithKeyBoardMouseBindings()
	{
		SystemLanguage systemLanguage = Application.systemLanguage;
		CharacterActions characterActions = new CharacterActions();
		SetupBind("Key:Left", characterActions.Left);
		SetupBind("Key:Left_Alt", characterActions.Left);
		SetupBind("Key:Right", characterActions.Right);
		SetupBind("Key:Right_Alt", characterActions.Right);
		SetupBind("Key:Up", characterActions.Up);
		SetupBind("Key:Up_Alt", characterActions.Up);
		SetupBind("Key:Down", characterActions.Down);
		SetupBind("Key:Down_Alt", characterActions.Down);
		SetupBind("Key:Jump", characterActions.Jump);
		SetupBind("Key:Jump_Alt", characterActions.Jump);
		SetupBind("Key:Block", characterActions.Block);
		SetupBind("Key:Block_Alt", characterActions.Block);
		SetupBind("Key:Throw", characterActions.Throw);
		SetupBind("Key:Throw_Alt", characterActions.Throw);
		SetupBind("Key:PunchOrFire", characterActions.PunchOrFire);
		SetupBind("Key:PunchOrFire_Alt", characterActions.PunchOrFire);
		characterActions.AimDown.AddDefaultBinding(Mouse.NegativeY);
		characterActions.AimDown.AddDefaultBinding(Key.S);
		characterActions.AimLeft.AddDefaultBinding(Mouse.NegativeX);
		characterActions.AimLeft.AddDefaultBinding(Key.A);
		characterActions.AimRight.AddDefaultBinding(Mouse.PositiveX);
		characterActions.AimRight.AddDefaultBinding(Key.D);
		characterActions.AimUp.AddDefaultBinding(Mouse.PositiveY);
		characterActions.AimUp.AddDefaultBinding(Key.W);
		characterActions.SetInputType(InputType.Keyboard);
		return characterActions;
	}

	public static void AddControllerBindings(CharacterActions playerActions)
	{
		playerActions.Left.AddDefaultBinding(InputControlType.LeftStickLeft);
		playerActions.Left.AddDefaultBinding(InputControlType.DPadLeft);
		playerActions.Right.AddDefaultBinding(InputControlType.LeftStickRight);
		playerActions.Right.AddDefaultBinding(InputControlType.DPadRight);
		playerActions.Up.AddDefaultBinding(InputControlType.LeftStickUp);
		playerActions.Up.AddDefaultBinding(InputControlType.DPadUp);
		playerActions.Down.AddDefaultBinding(InputControlType.LeftStickDown);
		playerActions.Down.AddDefaultBinding(InputControlType.DPadDown);
		playerActions.Jump.AddDefaultBinding(InputControlType.Action1);
		playerActions.Jump.AddDefaultBinding(InputControlType.LeftBumper);
		playerActions.Block.AddDefaultBinding(InputControlType.LeftTrigger);
		playerActions.Throw.AddDefaultBinding(InputControlType.Action4);
		playerActions.PunchOrFire.AddDefaultBinding(InputControlType.RightTrigger);
		playerActions.PunchOrFire.AddDefaultBinding(InputControlType.Action3);
		playerActions.AimDown.AddDefaultBinding(InputControlType.RightStickDown);
		playerActions.AimLeft.AddDefaultBinding(InputControlType.RightStickLeft);
		playerActions.AimRight.AddDefaultBinding(InputControlType.RightStickRight);
		playerActions.AimUp.AddDefaultBinding(InputControlType.RightStickUp);
	}

	public static CharacterActions CreateWithAnyBindings()
	{
		CharacterActions characterActions = new CharacterActions();
		AddControllerBindings(characterActions);
		SetupBind("Key:Left", characterActions.Left);
		SetupBind("Key:Left_Alt", characterActions.Left);
		SetupBind("Key:Right", characterActions.Right);
		SetupBind("Key:Right_Alt", characterActions.Right);
		SetupBind("Key:Up", characterActions.Up);
		SetupBind("Key:Up_Alt", characterActions.Up);
		SetupBind("Key:Down", characterActions.Down);
		SetupBind("Key:Down_Alt", characterActions.Down);
		SetupBind("Key:Jump", characterActions.Jump);
		SetupBind("Key:Jump_Alt", characterActions.Jump);
		SetupBind("Key:Block", characterActions.Block);
		SetupBind("Key:Block_Alt", characterActions.Block);
		SetupBind("Key:Throw", characterActions.Throw);
		SetupBind("Key:Throw_Alt", characterActions.Throw);
		SetupBind("Key:PunchOrFire", characterActions.PunchOrFire);
		SetupBind("Key:PunchOrFire_Alt", characterActions.PunchOrFire);
		characterActions.PunchOrFire.AddDefaultBinding(Mouse.LeftButton);
		characterActions.PunchOrFire.AddDefaultBinding(Key.C);
		characterActions.PunchOrFireWithKeyBoard.AddDefaultBinding(Key.C);
		characterActions.AimDown.AddDefaultBinding(Mouse.NegativeY);
		characterActions.AimDown.AddDefaultBinding(Key.S);
		characterActions.AimLeft.AddDefaultBinding(Mouse.NegativeX);
		characterActions.AimLeft.AddDefaultBinding(Key.A);
		characterActions.AimRight.AddDefaultBinding(Mouse.PositiveX);
		characterActions.AimRight.AddDefaultBinding(Key.D);
		characterActions.AimUp.AddDefaultBinding(Mouse.PositiveY);
		characterActions.AimUp.AddDefaultBinding(Key.W);
		characterActions.SetInputType(InputType.Any);
		return characterActions;
	}

	private void SetInputType(InputType type)
	{
		mInputType = type;
	}

	private static void UpdateKeyInPref(string key, KeybindingInfo keyInfo, bool altKey)
	{
		if (altKey)
		{
			key += "_Alt";
		}
		int num = keyInfo.key;
		if (keyInfo.mouse)
		{
			num += 1000;
		}
		PlayerPrefs.SetInt(key, num);
	}

	private void SetupBinds(string key, PlayerAction action)
	{
		SetupBind(key, action, DefaultKeybindings[key]);
		SetupBind(key + "_Alt", action, DefaultKeybindings[key + "_Alt"]);
	}

	public void RebindAction(PlayerActions action, KeybindingInfo key, bool altKey = false)
	{
		switch (action)
		{
		case PlayerActions.Up:
		case PlayerActions.Jump:
			Up.ClearBindings();
			UpdateKeyInPref("Key:Up", key, altKey);
			SetupBinds("Key:Up", Up);
			Movement = CreateTwoAxisPlayerAction(Left, Right, Down, Up);
			Up.AddDefaultBinding(InputControlType.LeftStickUp);
			Up.AddDefaultBinding(InputControlType.DPadUp);
			Jump.ClearBindings();
			UpdateKeyInPref("Key:Jump", key, altKey);
			SetupBinds("Key:Jump", Jump);
			Jump.AddDefaultBinding(InputControlType.Action1);
			Jump.AddDefaultBinding(InputControlType.LeftBumper);
			break;
		case PlayerActions.Down:
			Down.ClearBindings();
			UpdateKeyInPref("Key:Down", key, altKey);
			SetupBinds("Key:Down", Down);
			Movement = CreateTwoAxisPlayerAction(Left, Right, Down, Up);
			Down.AddDefaultBinding(InputControlType.LeftStickDown);
			Down.AddDefaultBinding(InputControlType.DPadDown);
			break;
		case PlayerActions.Left:
			Left.ClearBindings();
			UpdateKeyInPref("Key:Left", key, altKey);
			SetupBinds("Key:Left", Left);
			Movement = CreateTwoAxisPlayerAction(Left, Right, Down, Up);
			Left.AddDefaultBinding(InputControlType.LeftStickLeft);
			Left.AddDefaultBinding(InputControlType.DPadLeft);
			break;
		case PlayerActions.Right:
			Right.ClearBindings();
			UpdateKeyInPref("Key:Right", key, altKey);
			SetupBinds("Key:Right", Right);
			Movement = CreateTwoAxisPlayerAction(Left, Right, Down, Up);
			Right.AddDefaultBinding(InputControlType.LeftStickRight);
			Right.AddDefaultBinding(InputControlType.DPadRight);
			break;
		case PlayerActions.Throw:
			Throw.ClearBindings();
			UpdateKeyInPref("Key:Throw", key, altKey);
			SetupBinds("Key:Throw", Throw);
			Throw.AddDefaultBinding(InputControlType.Action4);
			break;
		case PlayerActions.Attack:
			PunchOrFire.ClearBindings();
			UpdateKeyInPref("Key:PunchOrFire", key, altKey);
			SetupBinds("Key:PunchOrFire", PunchOrFire);
			PunchOrFire.AddDefaultBinding(InputControlType.RightTrigger);
			PunchOrFire.AddDefaultBinding(InputControlType.Action3);
			break;
		case PlayerActions.Block:
			Block.ClearBindings();
			UpdateKeyInPref("Key:Block", key, altKey);
			SetupBinds("Key:Block", Block);
			Block.AddDefaultBinding(InputControlType.LeftTrigger);
			break;
		default:
			Debug.LogError("Invalid player action");
			break;
		}
	}

	public static void SaveKeybinding(PlayerActions action, KeybindingInfo key, bool altKey = false)
	{
		switch (action)
		{
		case PlayerActions.Up:
			UpdateKeyInPref("Key:Up", key, altKey);
			break;
		case PlayerActions.Down:
			UpdateKeyInPref("Key:Down", key, altKey);
			break;
		case PlayerActions.Left:
			UpdateKeyInPref("Key:Left", key, altKey);
			break;
		case PlayerActions.Right:
			UpdateKeyInPref("Key:Right", key, altKey);
			break;
		case PlayerActions.Throw:
			UpdateKeyInPref("Key:Throw", key, altKey);
			break;
		case PlayerActions.Attack:
			UpdateKeyInPref("Key:PunchOrFire", key, altKey);
			break;
		case PlayerActions.Block:
			UpdateKeyInPref("Key:Block", key, altKey);
			break;
		case PlayerActions.Jump:
			UpdateKeyInPref("Key:Jump", key, altKey);
			break;
		default:
			Debug.LogError("Invalid player action");
			break;
		}
	}

	public static void ResetKeybindings()
	{
		string[] actionPrefs = ActionPrefs;
		foreach (string text in actionPrefs)
		{
			PlayerPrefs.DeleteKey(text);
			PlayerPrefs.DeleteKey(text + "_Alt");
		}
	}

	public void ResetKeybindingsAndClear()
	{
		ResetKeybindings();
		Left.ClearBindings();
		SetupBind("Key:Left", Left);
		SetupBind("Key:Left_Alt", Left);
		Right.ClearBindings();
		SetupBind("Key:Right", Right);
		SetupBind("Key:Right_Alt", Right);
		Up.ClearBindings();
		SetupBind("Key:Up", Up);
		SetupBind("Key:Up_Alt", Up);
		Down.ClearBindings();
		SetupBind("Key:Down", Down);
		SetupBind("Key:Down_Alt", Down);
		Jump.ClearBindings();
		SetupBind("Key:Jump", Jump);
		SetupBind("Key:Jump_Alt", Jump);
		Block.ClearBindings();
		SetupBind("Key:Block", Block);
		SetupBind("Key:Block_Alt", Block);
		Throw.ClearBindings();
		SetupBind("Key:Throw", Throw);
		SetupBind("Key:Throw_Alt", Throw);
		PunchOrFire.ClearBindings();
		SetupBind("Key:PunchOrFire", PunchOrFire);
		SetupBind("Key:PunchOrFire_Alt", PunchOrFire);
		if (mInputType == InputType.Any || mInputType == InputType.Controller)
		{
			AddControllerBindings(this);
		}
	}
}
