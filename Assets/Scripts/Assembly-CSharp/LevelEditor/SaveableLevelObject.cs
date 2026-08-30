using System;
using System.Xml.Serialization;

namespace LevelEditor
{
	[Serializable]
	public class SaveableLevelObject
	{
		public float PositionX;

		public float PositionY;

		public float RotationX;

		public float RotationY;

		public float ScaleX;

		public float ScaleY;

		public string ObjectID;

		public bool HasMirrorObject;

		public int PropsSeed = int.MinValue;

		public int NetworkID = -1;

		[XmlIgnore]
		public bool HasValidSeed
		{
			get
			{
				return PropsSeed != int.MinValue;
			}
		}
	}
}
