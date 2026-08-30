using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoardHandler : MonoBehaviour
{
	[Serializable]
	public class Wave
	{
		public GameObject[] units;
	}

	public Wave[] unitWaves;

	public GameObject character;

	public Material mat;

	private int CurrentWave;

	public bool waves = true;

	public int specificNumber;

	private List<Controller> charactersAlive = new List<Controller>();

	public Vector3 spawn;

	public Vector3 randomSpawn;

	public int numberOfUnitsToSpawnOnCall = 1;

	public int waveUnits = 1;

	private void Start()
	{
		if (waves)
		{
			StartWave();
		}
	}

	private void Update()
	{
		if (waves && charactersAlive.Count == 0)
		{
			StartWave();
		}
		if (charactersAlive.Count < specificNumber && MapInfo.canSpawnBots)
		{
			SpawnAI(character);
		}
	}

	private void StartWave()
	{
		int numberOfEnemies = waveUnits;
		StartCoroutine(SpawmEnemies(numberOfEnemies, character));
		CurrentWave++;
	}

	public void Spawn()
	{
		StartCoroutine(SpawmEnemies(unitWaves[CurrentWave].units));
		if (CurrentWave < unitWaves.Length - 1)
		{
			CurrentWave++;
		}
	}

	private IEnumerator SpawmEnemies(GameObject[] units)
	{
		for (int i = 0; i < units.Length; i++)
		{
			SpawnAI(units[i]);
			yield return new WaitForSeconds(0.1f);
		}
	}

	private IEnumerator SpawmEnemies(int numberOfEnemies, GameObject unit)
	{
		for (int i = 0; i < numberOfEnemies; i++)
		{
			SpawnAI(unit);
			yield return new WaitForSeconds(0.1f);
		}
	}

	private void SpawnAI(GameObject unit)
	{
		Vector3 position = spawn;
		position += new Vector3(UnityEngine.Random.Range(0f - randomSpawn.x, randomSpawn.x), UnityEngine.Random.Range(0f - randomSpawn.y, randomSpawn.y), UnityEngine.Random.Range(0f - randomSpawn.z, randomSpawn.z));
		GameObject gameObject = UnityEngine.Object.Instantiate(unit, position, Quaternion.identity);
		Controller component = gameObject.GetComponent<Controller>();
		gameObject.GetComponent<CharacterInformation>().myMaterial = mat;
		component.isAI = true;
		component.playerID = -1;
		component.SetCollision(true);
		LineRenderer[] componentsInChildren = gameObject.GetComponentsInChildren<LineRenderer>();
		foreach (LineRenderer lineRenderer in componentsInChildren)
		{
			lineRenderer.sharedMaterial = mat;
		}
		SpriteRenderer[] componentsInChildren2 = gameObject.GetComponentsInChildren<SpriteRenderer>();
		foreach (SpriteRenderer spriteRenderer in componentsInChildren2)
		{
			spriteRenderer.color = mat.color;
		}
		MeshRenderer[] componentsInChildren3 = gameObject.GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer meshRenderer in componentsInChildren3)
		{
		}
		charactersAlive.Add(component);
	}

	public void RemoveAI(Controller c)
	{
		charactersAlive.Remove(c);
	}
}
