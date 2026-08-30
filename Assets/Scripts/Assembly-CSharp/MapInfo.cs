using System.Collections.Generic;
using UnityEngine;

public class MapInfo : MonoBehaviour
{
	public Transform[] spawnPoints;

	public bool dontFollowTheSwoosher = true;

	public bool dontSpawnItems;

	public bool dontSpawnBots;

	public static bool canSpawnBots;

	public float extraWeaponSpawnTime;

	public AudioClip mapMusic;

	public List<string> metaData = new List<string>();

	private void Start()
	{
		canSpawnBots = !dontSpawnBots;
		if ((bool)MusicHandler.Instance)
		{
			if ((bool)mapMusic)
			{
				MusicHandler.Instance.PlaySpecialSong(mapMusic);
			}
			else
			{
				MusicHandler.Instance.StopPlayingSpecialSong();
			}
		}
	}

	private void FixedUpdate()
	{
		if (!dontFollowTheSwoosher)
		{
			base.transform.position = Maps.myGlobalPosition;
		}
	}
}
