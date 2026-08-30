using System;
using UnityEngine;

namespace LevelEditor
{
	[Serializable]
	public struct ThemeProps
	{
		public string ThemeName;

		public bool ShallTile;

		public GameObject BackGroundObject;

		public Material GroundMaterial;

		public Material ThemeMaterial;

		public GameObject[] VegetationProps;
	}
}
