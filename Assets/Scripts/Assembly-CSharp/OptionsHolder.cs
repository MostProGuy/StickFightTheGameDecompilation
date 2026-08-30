using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.PostProcessing;

public class OptionsHolder : MonoBehaviour
{
	public static int masterVolume;

	public static int SFXVolume;

	public static int musicVolume;

	public static int shake;

	public static int chat;

	public static int pickup;

	public static int ping;

	public static int bots;

	public static int maps;

	public static int weaponsSpawn;

	public static int HP;

	public static int regen;

	public static int vSync;

	public static int showWins;

	public static int AA;

	public static int resolutionIndex;

	public static int fullscreen;

	public static int framerate;

	public PostProcessingProfile profile;

	public static int[] AvailableFramerates = new int[6] { 60, 75, 120, 144, 240, 0 };

	private static byte[] LastSentOptionsData = new byte[0];

	private void Start()
	{
		CheckOptions(this);
		GetComponent<AudioManager>().SetMixers();
	}

	public static void NetworkOptionsChanged(byte[] data)
	{
		byte b;
		byte hP;
		byte b2;
		byte b3;
		using (MemoryStream input = new MemoryStream(data))
		{
			using (BinaryReader binaryReader = new BinaryReader(input))
			{
				b = binaryReader.ReadByte();
				hP = binaryReader.ReadByte();
				b2 = binaryReader.ReadByte();
				b3 = binaryReader.ReadByte();
			}
		}
		maps = b;
		HP = hP;
		regen = b2;
		weaponsSpawn = b3;
		switch (HP)
		{
		case 0:
			HP = 100;
			break;
		case 1:
			HP = 200;
			break;
		case 2:
			HP = 300;
			break;
		case 3:
			HP = 1;
			break;
		case 4:
			HP = 25;
			break;
		case 5:
			HP = 50;
			break;
		case 6:
			HP = 75;
			break;
		}
		Debug.Log("Network Options Changed!");
		Debug.Log("HP: " + HP);
		Debug.Log("regen: " + regen);
		Debug.Log("Weapons spawns: " + b3);
	}

	private void SetNonStatic()
	{
		if (vSync == 0)
		{
			QualitySettings.vSyncCount = 0;
		}
		else
		{
			QualitySettings.vSyncCount = 1;
		}
		if (AA == 0)
		{
			profile.antialiasing.enabled = true;
		}
		else
		{
			profile.antialiasing.enabled = false;
		}
		WinCounterUI winCounterUI = Object.FindObjectOfType<WinCounterUI>();
		if (winCounterUI != null)
		{
			winCounterUI.ToggleWinCounterVisibility(showWins == 1);
		}
		if (framerate >= 0 && framerate < AvailableFramerates.Length)
		{
			Application.targetFrameRate = AvailableFramerates[framerate];
		}
		else
		{
			Debug.LogWarning("Invalid framerate set");
		}
		List<Resolution> list = new List<Resolution>(Screen.resolutions).Distinct().ToList();
		list.Sort((Resolution x, Resolution y) => x.width - y.width);
		if (resolutionIndex >= 0 && resolutionIndex < list.Count)
		{
			Screen.SetResolution(list[resolutionIndex].width, list[resolutionIndex].height, fullscreen == 0);
			return;
		}
		PlayerPrefs.SetInt("Resolution", list.Count - 1);
		CheckOptions(this);
	}

	public static void CheckOptions(OptionsHolder optionsHolder)
	{
		switch (PlayerPrefs.GetInt("Master"))
		{
		case 0:
			masterVolume = 100;
			break;
		case 1:
			masterVolume = 0;
			break;
		case 2:
			masterVolume = 10;
			break;
		case 3:
			masterVolume = 20;
			break;
		case 4:
			masterVolume = 30;
			break;
		case 5:
			masterVolume = 40;
			break;
		case 6:
			masterVolume = 50;
			break;
		case 7:
			masterVolume = 60;
			break;
		case 8:
			masterVolume = 70;
			break;
		case 9:
			masterVolume = 80;
			break;
		case 10:
			masterVolume = 90;
			break;
		}
		switch (PlayerPrefs.GetInt("SFX"))
		{
		case 0:
			SFXVolume = 100;
			break;
		case 1:
			SFXVolume = 0;
			break;
		case 2:
			SFXVolume = 10;
			break;
		case 3:
			SFXVolume = 20;
			break;
		case 4:
			SFXVolume = 30;
			break;
		case 5:
			SFXVolume = 40;
			break;
		case 6:
			SFXVolume = 50;
			break;
		case 7:
			SFXVolume = 60;
			break;
		case 8:
			SFXVolume = 70;
			break;
		case 9:
			SFXVolume = 80;
			break;
		case 10:
			SFXVolume = 90;
			break;
		}
		switch (PlayerPrefs.GetInt("Shake"))
		{
		case 0:
			shake = 100;
			break;
		case 1:
			shake = 0;
			break;
		case 2:
			shake = 10;
			break;
		case 3:
			shake = 20;
			break;
		case 4:
			shake = 30;
			break;
		case 5:
			shake = 40;
			break;
		case 6:
			shake = 50;
			break;
		case 7:
			shake = 60;
			break;
		case 8:
			shake = 70;
			break;
		case 9:
			shake = 80;
			break;
		case 10:
			shake = 90;
			break;
		}
		switch (PlayerPrefs.GetInt("Music"))
		{
		case 0:
			musicVolume = 100;
			break;
		case 1:
			musicVolume = 0;
			break;
		case 2:
			musicVolume = 10;
			break;
		case 3:
			musicVolume = 20;
			break;
		case 4:
			musicVolume = 30;
			break;
		case 5:
			musicVolume = 40;
			break;
		case 6:
			musicVolume = 50;
			break;
		case 7:
			musicVolume = 60;
			break;
		case 8:
			musicVolume = 70;
			break;
		case 9:
			musicVolume = 80;
			break;
		case 10:
			musicVolume = 90;
			break;
		}
		switch (PlayerPrefs.GetInt("Bots"))
		{
		case 0:
			bots = 2;
			break;
		case 1:
			bots = 3;
			break;
		case 2:
			bots = 4;
			break;
		case 3:
			bots = 5;
			break;
		case 4:
			bots = 6;
			break;
		case 5:
			bots = 7;
			break;
		case 6:
			bots = 8;
			break;
		case 7:
			bots = 9;
			break;
		case 8:
			bots = 10;
			break;
		case 9:
			bots = 0;
			break;
		case 10:
			bots = 1;
			break;
		}
		pickup = PlayerPrefs.GetInt("Auto Pickup", 0);
		chat = PlayerPrefs.GetInt("Chat", 0);
		ping = PlayerPrefs.GetInt("Ping", 0);
		maps = PlayerPrefs.GetInt("Maps");
		if (maps != 0)
		{
			PlayerPrefs.SetInt("HasChangedLevelPref", 1);
		}
		weaponsSpawn = PlayerPrefs.GetInt("weaponSpawns");
		AA = PlayerPrefs.GetInt("AA");
		vSync = PlayerPrefs.GetInt("Vsync");
		showWins = PlayerPrefs.GetInt("Show Wins");
		if (PlayerPrefs.HasKey("Resolution"))
		{
			resolutionIndex = PlayerPrefs.GetInt("Resolution");
		}
		else
		{
			resolutionIndex = -1;
		}
		fullscreen = PlayerPrefs.GetInt("Fullscreen");
		framerate = PlayerPrefs.GetInt("Framerate", 60);
		optionsHolder.SetNonStatic();
		Debug.Log(maps);
		switch (PlayerPrefs.GetInt("HP"))
		{
		case 0:
			HP = 100;
			break;
		case 1:
			HP = 200;
			break;
		case 2:
			HP = 300;
			break;
		case 3:
			HP = 1;
			break;
		case 4:
			HP = 25;
			break;
		case 5:
			HP = 50;
			break;
		case 6:
			HP = 75;
			break;
		}
		regen = PlayerPrefs.GetInt("Regen");
		if (MatchmakingHandler.IsNetworkMatch)
		{
			SendOptionsOverNetwork();
		}
	}

	private static void SendOptionsOverNetwork()
	{
		if (MultiplayerManager.IsServer)
		{
			byte[] optionsData = GetOptionsData();
			if (optionsData != LastSentOptionsData)
			{
				Object.FindObjectOfType<MultiplayerManager>().OptionsChanged(optionsData);
				LastSentOptionsData = optionsData;
			}
		}
	}

	public static byte[] GetOptionsData()
	{
		byte[] array = new byte[4];
		using (MemoryStream output = new MemoryStream(array))
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(output))
			{
				binaryWriter.Write((byte)maps);
				binaryWriter.Write((byte)PlayerPrefs.GetInt("HP"));
				binaryWriter.Write((byte)regen);
				binaryWriter.Write((byte)weaponsSpawn);
				return array;
			}
		}
	}

	public static void DefaultSettings()
	{
		HP = 100;
		regen = 0;
		weaponsSpawn = 0;
	}

	public static void NetworkMapChanged(byte v)
	{
		maps = v;
	}
}
