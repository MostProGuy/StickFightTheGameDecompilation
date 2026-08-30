using System.Collections.Generic;
using System.Collections.ObjectModel;
using InControl;
using UnityEngine;

public class ControllerHandler : MonoBehaviour
{
	private static ControllerHandler _instance;

	public GameObject playerPrefab;

	private GameManager gameManager;

	public const int maxPlayers = 4;

	public Material[] colors;

	private List<KeyCode> IgnoredAnyKeys = new List<KeyCode>();

	private List<Controller> mPlayers = new List<Controller>(4);

	private MultiplayerManager mNetworkManager;

	public static ControllerHandler Instance
	{
		get
		{
			return _instance;
		}
	}

	public List<Controller> ActivePlayers
	{
		get
		{
			List<Controller> list = new List<Controller>(players);
			list.RemoveAll((Controller x) => x == null);
			return list;
		}
	}

	public List<Controller> players
	{
		get
		{
			if (MatchmakingHandler.IsNetworkMatch)
			{
				return mNetworkManager.PlayerControllers;
			}
			return mPlayers;
		}
		set
		{
			mPlayers = value;
		}
	}

	private void Awake()
	{
		if (_instance != null && _instance != this)
		{
			Object.Destroy(base.gameObject);
		}
		else
		{
			_instance = this;
		}
	}

	private void Start()
	{
		mNetworkManager = Object.FindObjectOfType<MultiplayerManager>();
		InputManager.OnDeviceDetached += OnDeviceDetached;
		gameManager = GetComponent<GameManager>();
		IgnoredAnyKeys.Add(KeyCode.LeftShift);
		IgnoredAnyKeys.Add(KeyCode.RightShift);
		IgnoredAnyKeys.Add(KeyCode.Tab);
	}

	private void Update()
	{
		if (MatchmakingHandler.IsNetworkMatch)
		{
			return;
		}
		if (IsPressingKeyBoardButton() && ThereIsNoPlayerOnKeyBoard())
		{
			CreatePlayer(null, true);
			return;
		}
		ReadOnlyCollection<InputDevice> devices = InputManager.Devices;
		foreach (InputDevice item in devices)
		{
			if (JoinButtonWasPressedOnDevice(item) && ThereIsNoPlayerUsingDevice(item))
			{
				CreatePlayer(item);
			}
		}
	}

	private bool IsPressingKeyBoardButton()
	{
		return CharacterActions.IsAnyKeybindPressed(IgnoredAnyKeys) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.V) || Input.GetKeyDown(KeyCode.B);
	}

	private bool JoinButtonWasPressedOnDevice(InputDevice inputDevice)
	{
		return inputDevice.AnyButtonIsPressed;
	}

	private Controller FindPlayerUsingDevice(InputDevice inputDevice)
	{
		int count = players.Count;
		for (int i = 0; i < count; i++)
		{
			Controller controller = players[i];
			if (controller.PlayerActions.Device == inputDevice)
			{
				return controller;
			}
		}
		return null;
	}

	private bool ThereIsNoPlayerOnKeyBoard()
	{
		foreach (Controller player in players)
		{
			if (player.PlayerActions.InputType == InputType.Keyboard)
			{
				return false;
			}
		}
		return true;
	}

	private bool ThereIsNoPlayerUsingDevice(InputDevice inputDevice)
	{
		return FindPlayerUsingDevice(inputDevice) == null;
	}

	private void OnDeviceDetached(InputDevice inputDevice)
	{
		if (!MatchmakingHandler.IsNetworkMatch)
		{
			Controller controller = FindPlayerUsingDevice(inputDevice);
			if (controller != null)
			{
				RemovePlayer(controller);
			}
		}
	}

	private Controller CreatePlayer(InputDevice inputDevice, bool keyBoard = false)
	{
		if (players.Count < 4)
		{
			GameObject gameObject = Object.Instantiate(playerPrefab, Vector3.up * 8f, Quaternion.identity);
			Controller component = gameObject.GetComponent<Controller>();
			component.AssignNewDevice(inputDevice, keyBoard);
			int num = 0;
			foreach (Controller player in players)
			{
				if (player.playerID == num)
				{
					num++;
				}
			}
			component.playerID = num;
			component.SetCollision(true);
			LineRenderer[] componentsInChildren = gameObject.GetComponentsInChildren<LineRenderer>();
			foreach (LineRenderer lineRenderer in componentsInChildren)
			{
				lineRenderer.sharedMaterial = colors[num];
			}
			SpriteRenderer[] componentsInChildren2 = gameObject.GetComponentsInChildren<SpriteRenderer>();
			foreach (SpriteRenderer spriteRenderer in componentsInChildren2)
			{
				if (spriteRenderer.transform.tag != "DontChangeColor")
				{
					spriteRenderer.color = colors[num].color;
				}
			}
			MeshRenderer[] componentsInChildren3 = gameObject.GetComponentsInChildren<MeshRenderer>();
			foreach (MeshRenderer meshRenderer in componentsInChildren3)
			{
			}
			component.GetComponent<CharacterInformation>().myMaterial = colors[num];
			players.Add(component);
			gameManager.RevivePlayer(component);
			return component;
		}
		return null;
	}

	private void RemovePlayer(Controller player)
	{
		gameManager.RemovePlayer(player);
		players.Remove(player);
		player.AssignNewDevice(null);
		Object.Destroy(player.gameObject);
		WinCounterUI winCounterUI = Object.FindObjectOfType<WinCounterUI>();
		if (winCounterUI != null)
		{
			winCounterUI.RefreshWinTexts();
		}
	}
}
