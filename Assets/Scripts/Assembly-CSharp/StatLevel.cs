using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StatLevel : MonoBehaviour
{
	private TextMeshProUGUI text;

	private float counter;

	private IList<CharacterStats> playerStats = new List<CharacterStats>();

	private ScreenshakeHandler shake;

	private void Start()
	{
		text = GetComponentInChildren<TextMeshProUGUI>();
		shake = ScreenshakeHandler.Instance;
		StartStatLevel();
	}

	private void Update()
	{
		counter += Time.deltaTime;
	}

	private void StartStatLevel()
	{
		text = GetComponentInChildren<TextMeshProUGUI>();
		playerStats.Clear();
		Controller[] array = Object.FindObjectsOfType<Controller>();
		foreach (Controller controller in array)
		{
			if (!controller.isAI)
			{
				playerStats.Add(controller.GetComponent<CharacterStats>());
			}
		}
		StartCoroutine(PlayStats());
	}

	private IEnumerator PlayStats()
	{
		float timeBetweenMessages = 8f;
		yield return new WaitForSeconds(1f);
		StartCoroutine(PlayText(CheckForBestKiller()));
		yield return new WaitForSeconds(timeBetweenMessages);
		StartCoroutine(PlayText(CheckForWorstKiller()));
		yield return new WaitForSeconds(timeBetweenMessages);
		StartCoroutine(PlayText(CheckForFallOut()));
		yield return new WaitForSeconds(timeBetweenMessages);
		StartCoroutine(PlayText(CheckForBulletHits()));
		yield return new WaitForSeconds(timeBetweenMessages);
		StartCoroutine(PlayText(CheckForPickups()));
		yield return new WaitForSeconds(timeBetweenMessages);
		StartCoroutine(PlayText(CheckForBulletFired()));
		yield return new WaitForSeconds(timeBetweenMessages);
		StartCoroutine(PlayText(CheckForPunches()));
		yield return new WaitForSeconds(timeBetweenMessages);
		StartCoroutine(PlayText(CheckForBlocks()));
		yield return new WaitForSeconds(timeBetweenMessages);
		StartCoroutine(PlayText(CheckForBulletMissed()));
		yield return new WaitForSeconds(timeBetweenMessages);
		StartCoroutine(PlayText(CheckForCrownSteals()));
	}

	private string CheckForBestKiller()
	{
		int id = 0;
		int num = 0;
		for (int i = 0; i < playerStats.Count; i++)
		{
			if (playerStats[i].kills > num)
			{
				num = playerStats[i].kills;
				id = playerStats[i].GetComponent<Controller>().playerID;
			}
		}
		string text = string.Empty;
		switch (Random.Range(0, 5))
		{
		case 0:
		{
			string text2 = text;
			text = text2 + PlayerIDToColor(id) + " IS IN THE LEAD WITH " + num + " KILLS\nWELL DONE DONE " + PlayerIDToColor(id);
			break;
		}
		case 1:
		{
			string text2 = text;
			text = text2 + PlayerIDToColor(id) + " IS DEFINITELY THE BEST PLAYER\n" + num + " STICK LIVES ENDED SO FAR";
			break;
		}
		case 2:
		{
			string text2 = text;
			text = text2 + PlayerIDToColor(id) + " IS DOING GREAT WITH THEIR " + num + " KILLS";
			break;
		}
		case 3:
		{
			string text2 = text;
			text = text2 + "WATCH OUT FOR " + PlayerIDToColor(id) + "\nTHEY ARE AT A WHOOPING " + num + " KILLS";
			break;
		}
		case 4:
		{
			string text2 = text;
			text = text2 + "PEOPLE SHOULD GANG UP ON " + PlayerIDToColor(id) + "\nTHEY HAVE KILLED THE REST OF YOU " + num + " TIMES";
			break;
		}
		}
		return text;
	}

	private string CheckForWorstKiller()
	{
		int id = 0;
		int num = 99;
		for (int i = 0; i < playerStats.Count; i++)
		{
			if (playerStats[i].kills < num)
			{
				num = playerStats[i].kills;
				id = playerStats[i].GetComponent<Controller>().playerID;
			}
		}
		string text = string.Empty;
		switch (Random.Range(0, 5))
		{
		case 0:
		{
			string text2 = text;
			text = text2 + PlayerIDToColor(id) + " IS FAILING MISERABLY\n" + num + " KILLS\nSAD";
			break;
		}
		case 1:
		{
			string text2 = text;
			text = text2 + PlayerIDToColor(id) + " IS NOT DOING GREAT\nONLY " + num + " KILLS";
			break;
		}
		case 2:
		{
			string text2 = text;
			text = text2 + " GO EASY ON " + PlayerIDToColor(id) + "\nTHEY ONLY HAVE " + num + " KILLS";
			break;
		}
		case 3:
		{
			string text2 = text;
			text = text2 + "COME ON " + PlayerIDToColor(id) + "\nYOU CAN DO BETTER THAN " + num + " KILLS";
			break;
		}
		case 4:
		{
			string text2 = text;
			text = text2 + PlayerIDToColor(id) + " HAS " + num + " KILLS\nNO ONE ELSE HAS THAT FEW";
			break;
		}
		}
		return text;
	}

	private string CheckForFallOut()
	{
		int id = 0;
		int num = 0;
		for (int i = 0; i < playerStats.Count; i++)
		{
			if (playerStats[i].falls > num)
			{
				num = playerStats[i].falls;
				id = playerStats[i].GetComponent<Controller>().playerID;
			}
		}
		string text = string.Empty;
		switch (Random.Range(0, 5))
		{
		case 0:
		{
			string text2 = text;
			text = text2 + PlayerIDToColor(id) + " KEEPS FALLING OUT OF THE LEVEL\n" + num + " TIMES SO FAR";
			break;
		}
		case 1:
		{
			string text2 = text;
			text = text2 + PlayerIDToColor(id) + " HAS FALLEN OUT OF THE LEVEL " + num + " TIMES";
			break;
		}
		case 2:
		{
			string text2 = text;
			text = text2 + "SOMEONE NEEDS TO TEACH " + PlayerIDToColor(id) + " HOW TO WALK\nTHEY HAVE FALLEN OUT " + num + " TIMES";
			break;
		}
		case 3:
		{
			string text2 = text;
			text = text2 + "WTF " + PlayerIDToColor(id) + "\nHOW HAVE YOU FALLEN OUT\n" + num + " TIMES";
			break;
		}
		case 4:
		{
			string text2 = text;
			text = text2 + PlayerIDToColor(id) + " HAS LITERALLY FALLEN OUT " + num + " TIMES";
			break;
		}
		}
		return text;
	}

	private string CheckForCrownSteals()
	{
		int id = 0;
		int num = 0;
		for (int i = 0; i < playerStats.Count; i++)
		{
			if (playerStats[i].crownSteals > num)
			{
				num = playerStats[i].crownSteals;
				id = playerStats[i].GetComponent<Controller>().playerID;
			}
		}
		string text = string.Empty;
		switch (Random.Range(0, 3))
		{
		case 0:
		{
			string text2 = text;
			text = text2 + PlayerIDToColor(id) + " HAS HAD THE CROWN " + num + " TIMES\nWELL DONE";
			break;
		}
		case 1:
		{
			string text2 = text;
			text = text2 + PlayerIDToColor(id) + " HAS TAKEN THE CROWN " + num + " TIMES\nTHATS PRETTY COOL";
			break;
		}
		case 2:
		{
			string text2 = text;
			text = text2 + "BOW DOWN TO " + PlayerIDToColor(id) + "\nTHEY HAVE HAD THE CROWN " + num + " TIMES";
			break;
		}
		}
		return text;
	}

	private string CheckForBulletHits()
	{
		int id = 0;
		int num = 0;
		for (int i = 0; i < playerStats.Count; i++)
		{
			if (playerStats[i].bulletsHit > num)
			{
				num = playerStats[i].bulletsHit;
				id = playerStats[i].GetComponent<Controller>().playerID;
			}
		}
		string text = string.Empty;
		switch (Random.Range(0, 3))
		{
		case 0:
		{
			string text2 = text;
			text = text2 + PlayerIDToColor(id) + " IS THE BEST MARKSMAN IN TOWN\n" + num + " BULLETS HIT SO FAR";
			break;
		}
		case 1:
		{
			string text2 = text;
			text = text2 + PlayerIDToColor(id) + " HAS SUCCESFULLY HIT " + num + " SHOTS SO FAR";
			break;
		}
		case 2:
		{
			string text2 = text;
			text = text2 + "DONT GET INTO A FIRE FIGHT WITH " + PlayerIDToColor(id) + "\nTHEY HAVE HIT " + num + " SHOTS";
			break;
		}
		}
		return text;
	}

	private string CheckForBulletMissed()
	{
		int id = 0;
		int num = 0;
		for (int i = 0; i < playerStats.Count; i++)
		{
			if (playerStats[i].bulletsMissed > num)
			{
				num = playerStats[i].bulletsMissed;
				id = playerStats[i].GetComponent<Controller>().playerID;
			}
		}
		string text = string.Empty;
		switch (Random.Range(0, 2))
		{
		case 0:
		{
			string text2 = text;
			text = text2 + PlayerIDToColor(id) + " KEEPS MISSING\nTHEY HAVE MISSED " + num + " SHOTS ALREADY";
			break;
		}
		case 1:
		{
			string text2 = text;
			text = text2 + PlayerIDToColor(id) + " CANT HIT ANYTHING\n" + num + " SHOTS MISSED";
			break;
		}
		}
		return text;
	}

	private string CheckForBulletFired()
	{
		int id = 0;
		int num = 0;
		for (int i = 0; i < playerStats.Count; i++)
		{
			if (playerStats[i].bulletsShot > num)
			{
				num = playerStats[i].bulletsShot;
				id = playerStats[i].GetComponent<Controller>().playerID;
			}
		}
		string text = string.Empty;
		switch (Random.Range(0, 2))
		{
		case 0:
		{
			string text2 = text;
			text = text2 + PlayerIDToColor(id) + " IS A GUN TOTING MANIAC\nTHEY HAVE FIRED " + num + " BULLETS";
			break;
		}
		case 1:
		{
			string text2 = text;
			text = text2 + PlayerIDToColor(id) + " IS SHOOTING LIKE CRAZY " + num + " SHOTS FIRED";
			break;
		}
		}
		return text;
	}

	private string CheckForBlocks()
	{
		int id = 0;
		int num = 0;
		for (int i = 0; i < playerStats.Count; i++)
		{
			if (playerStats[i].blocks > num)
			{
				num = playerStats[i].blocks;
				id = playerStats[i].GetComponent<Controller>().playerID;
			}
		}
		string text = string.Empty;
		switch (Random.Range(0, 2))
		{
		case 0:
		{
			string text2 = text;
			text = text2 + PlayerIDToColor(id) + " HAS BLOCKED " + num + " PUNCHES\nWOW";
			break;
		}
		case 1:
		{
			string text2 = text;
			text = text2 + PlayerIDToColor(id) + " IS A BRICK WALL\n" + num + " SWINGS BLOCKED ALREADY";
			break;
		}
		}
		return text;
	}

	private string CheckForPunches()
	{
		int id = 0;
		int num = 0;
		for (int i = 0; i < playerStats.Count; i++)
		{
			if (playerStats[i].punchesLanded > num)
			{
				num = playerStats[i].punchesLanded;
				id = playerStats[i].GetComponent<Controller>().playerID;
			}
		}
		string text = string.Empty;
		switch (Random.Range(0, 2))
		{
		case 0:
		{
			string text2 = text;
			text = text2 + PlayerIDToColor(id) + " HAS ALREADY LANDED " + num + " PUNCHES\nYOU ARE GREAT " + PlayerIDToColor(id);
			break;
		}
		case 1:
		{
			string text2 = text;
			text = text2 + PlayerIDToColor(id) + " IS LANDING SO MANY SWINGS\n" + num + " TO BE EXACT";
			break;
		}
		}
		return text;
	}

	private string CheckForPickups()
	{
		int id = 0;
		int num = 0;
		for (int i = 0; i < playerStats.Count; i++)
		{
			if (playerStats[i].weaponsPickedUp > num)
			{
				num = playerStats[i].weaponsPickedUp;
				id = playerStats[i].GetComponent<Controller>().playerID;
			}
		}
		string text = string.Empty;
		switch (Random.Range(0, 2))
		{
		case 0:
		{
			string text2 = text;
			text = text2 + PlayerIDToColor(id) + " HAS PICKED UP " + num + " WEAPONS\nYOU ARE SUCH A HOARDER " + PlayerIDToColor(id);
			break;
		}
		case 1:
		{
			string text2 = text;
			text = text2 + PlayerIDToColor(id) + " IS TAKING ALL THE GUNS\nTHEY HAVE STOLEN " + num + " SO FAR";
			break;
		}
		}
		return text;
	}

	private string CheckForThrows()
	{
		int id = 0;
		int num = 0;
		for (int i = 0; i < playerStats.Count; i++)
		{
			if (playerStats[i].weaponsThrown > num)
			{
				num = playerStats[i].weaponsThrown;
				id = playerStats[i].GetComponent<Controller>().playerID;
			}
		}
		string text = string.Empty;
		switch (Random.Range(0, 2))
		{
		case 0:
		{
			string text2 = text;
			text = text2 + PlayerIDToColor(id) + " HAS HIT " + num + " WEAPON THROWS\nPRETTY GOOD ACCURACY";
			break;
		}
		case 1:
		{
			string text2 = text;
			text = text2 + PlayerIDToColor(id) + " HAS THROWN " + num + " WEAPONS AT PEOPLE SO FAR\nTHATS COOL AND ALL BUT YOU KNOW THEY SHOOT TOO\nRIGHT?";
			break;
		}
		}
		return text;
	}

	private string PlayerIDToColor(int id)
	{
		switch (id)
		{
		case 0:
			return "YELLOW";
		case 1:
			return "BLUE";
		case 2:
			return "RED";
		default:
			return "GREEN";
		}
	}

	private IEnumerator PlayText(string textToDisplay)
	{
		float t = 0f;
		int i = 0;
		string displayedText = string.Empty;
		while (i < textToDisplay.Length)
		{
			t += Time.deltaTime;
			if (t > 0.05f)
			{
				t = 0f;
				displayedText += textToDisplay[i];
				i++;
				text.text = displayedText;
			}
			yield return null;
		}
		yield return new WaitForSeconds(2f);
		while (displayedText.Length > 0)
		{
			t += Time.deltaTime;
			if (t > 0.025f)
			{
				t = 0f;
				displayedText = displayedText.Remove(displayedText.Length - 1);
				i++;
				text.text = displayedText;
			}
			yield return null;
		}
	}
}
