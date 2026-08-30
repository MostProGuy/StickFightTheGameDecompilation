using System;

namespace LevelEditor
{
	[Serializable]
	public class SaveableWeaponObject
	{
		public float PositionX;

		public float PositionY;

		public int WeaponIndex;

		public bool HasMirrorObject;

		public int NetworkID = -1;
	}
}
