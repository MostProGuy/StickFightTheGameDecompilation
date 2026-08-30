using UnityEngine;

public class CharacterStats : MonoBehaviour
{
	public int wins;

	public int kills;

	public int deaths;

	public int suicides;

	public int falls;

	public int crownSteals;

	public int bulletsHit;

	public int bulletsMissed;

	public int bulletsShot;

	public int blocks;

	public int punchesLanded;

	public int weaponsPickedUp;

	public int weaponsThrown;

	private void Awake()
	{
	}

	private void SetRandomStats()
	{
		wins = Random.Range(0, 10);
		kills = Random.Range(0, 10);
		deaths = Random.Range(0, 10);
		suicides = Random.Range(0, 10);
		crownSteals = Random.Range(0, 10);
	}

	public string GetString()
	{
		string text = "Wins: " + wins;
		text = text + "\nKills: " + kills;
		text = text + "\nDeaths: " + deaths;
		text = text + "\nSuicides: " + suicides;
		text = text + "\nFalls: " + falls;
		text = text + "\nCrownSteals: " + crownSteals;
		text = text + "\nBulletsHit: " + bulletsHit;
		text = text + "\nBulletsMissed: " + bulletsMissed;
		text = text + "\nBulletsShot: " + bulletsShot;
		text = text + "\nBlocks: " + blocks;
		text = text + "\nPunchesLanded: " + punchesLanded;
		text = text + "\nWeaponsPickedUp: " + weaponsPickedUp;
		return text + "\nWeaponsThrown: " + weaponsThrown;
	}
}
