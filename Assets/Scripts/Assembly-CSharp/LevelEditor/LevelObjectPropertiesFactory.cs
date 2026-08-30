using UnityEngine;

namespace LevelEditor
{
	public static class LevelObjectPropertiesFactory
	{
		public static LevelObjectProperties DefaultProperties
		{
			get
			{
				return new LevelObjectProperties(true, false, false);
			}
		}

		public static LevelObjectProperties GetObjectPropertiesFor(string objectName)
		{
			switch (objectName.ToLower())
			{
			case "spike":
				return new LevelObjectProperties(true, true, true);
			case "weapon":
				return new LevelObjectProperties(false, false, false);
			default:
				Debug.Log("Could not find objectProperties For: " + objectName + " Returning Default");
				return DefaultProperties;
			}
		}
	}
}
