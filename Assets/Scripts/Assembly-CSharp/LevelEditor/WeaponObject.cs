using UnityEngine;

namespace LevelEditor
{
	public class WeaponObject
	{
		public Vector2 Position { get; private set; }

		public string WeaponName { get; private set; }

		public GameObject VisibleObject { get; private set; }

		public WeaponObject MirrorObject { get; private set; }

		public int Index { get; private set; }

		public LevelObjectProperties ObjectProperties { get; private set; }

		public WeaponObject(GameObject gun, int index, WeaponObject mirror = null)
		{
			VisibleObject = gun;
			InitPos(gun);
			ObjectProperties = LevelObjectPropertiesFactory.DefaultProperties;
			Index = index;
			MirrorObject = mirror;
			if (MirrorObject != null)
			{
				MirrorObject.AddMirrorObject(this);
			}
		}

		public bool HasMirrorObject()
		{
			return MirrorObject != null;
		}

		public void InitPos(GameObject gun)
		{
			Vector3 position = gun.transform.position;
			Position = new Vector2(position.y, position.z);
		}

		private void AddMirrorObject(WeaponObject mirrorObj)
		{
			MirrorObject = mirrorObj;
		}

		public SaveableWeaponObject GetSaveableObject()
		{
			SaveableWeaponObject saveableWeaponObject = new SaveableWeaponObject();
			saveableWeaponObject.PositionX = Position.x;
			saveableWeaponObject.PositionY = Position.y;
			saveableWeaponObject.WeaponIndex = Index;
			saveableWeaponObject.NetworkID = LevelObject.NetworkIDSetter++;
			saveableWeaponObject.HasMirrorObject = HasMirrorObject();
			return saveableWeaponObject;
		}
	}
}
