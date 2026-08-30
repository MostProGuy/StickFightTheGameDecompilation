using System;
using System.Collections.Generic;
using UnityEngine;

public class SnakeSpawner : MonoBehaviour
{
	[Serializable]
	public struct Spawn
	{
		public GameObject Snake;

		public Vector3 Rot;

		public Vector3 Pos;
	}

	[SerializeField]
	private List<Spawn> Spawns;

	private MultiplayerManager mNetworkManager;

	private void Start()
	{
		mNetworkManager = UnityEngine.Object.FindObjectOfType<MultiplayerManager>();
		for (int i = 0; i < Spawns.Count; i++)
		{
			Spawn spawn = Spawns[i];
			if (MatchmakingHandler.IsNetworkMatch)
			{
				if (MultiplayerManager.IsServer)
				{
					Quaternion quaternion = base.transform.rotation * Quaternion.Euler(spawn.Rot);
					mNetworkManager.SpawnObject(spawn.Snake, base.transform.TransformPoint(spawn.Pos), quaternion.eulerAngles, true);
				}
			}
			else
			{
				Quaternion rotation = base.transform.rotation * Quaternion.Euler(spawn.Rot);
				GameObject gameObject = UnityEngine.Object.Instantiate(spawn.Snake, base.transform.position + spawn.Pos, rotation);
			}
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
