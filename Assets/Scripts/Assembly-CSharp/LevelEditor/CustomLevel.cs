using System;
using System.Collections.Generic;

namespace LevelEditor
{
	[Serializable]
	public class CustomLevel
	{
		public SerializableVector2[] SpawnPoints = new SerializableVector2[4];

		public List<SaveableLevelObject> PlacedObjects;

		public List<SaveableWeaponObject> PlacedWeapons;

		public float MapSize;

		public int Theme;

		public string Version;
	}
}
