using System;
using UnityEngine;

namespace LevelEditor
{
	[Serializable]
	public struct SerializableVector2
	{
		public float X;

		public float Y;

		public static implicit operator Vector2(SerializableVector2 v)
		{
			return new Vector2
			{
				x = v.X,
				y = v.Y
			};
		}
	}
}
