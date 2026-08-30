using UnityEngine;

namespace LevelEditor
{
	public class LevelObject
	{
		public static int NetworkIDSetter;

		public Vector2 LevelObjectOffsetFromPosition = Vector2.zero;

		private bool m_IsNetworkObject;

		private GameObject[] m_SpawnedProps;

		private GameObject m_AttachedGround;

		public bool HasVegetation
		{
			get
			{
				return VegetationSeed != int.MinValue;
			}
		}

		public Vector2 Position { get; set; }

		public Vector3 Rotation { get; set; }

		public Vector2 Scale { get; set; }

		public string Id { get; private set; }

		public int VegetationSeed { get; private set; }

		public int NetworkID { get; private set; }

		public GameObject VisibleObject { get; private set; }

		public LevelObject MirrorObject { get; private set; }

		public LevelObjectProperties ObjectProperties { get; private set; }

		public LevelObject(GameObject obj, string objectID, LevelObject mirrorObj = null, int savedSeed = int.MinValue)
		{
			MirrorObject = mirrorObj;
			if (MirrorObject != null)
			{
				MirrorObject.AddMirrorObject(this);
			}
			InitPosRotScale(obj);
			Id = objectID;
			VegetationSeed = savedSeed;
			NetworkID = -1;
			m_IsNetworkObject = obj.GetComponent<NetworkComponentTAG>();
			ObjectProperties = LevelObjectPropertiesFactory.GetObjectPropertiesFor(objectID);
		}

		public void InitPosRotScale(GameObject obj)
		{
			VisibleObject = obj;
			Vector3 position = VisibleObject.transform.position;
			Position = new Vector2(position.y, position.z);
			Rotation = obj.transform.rotation.eulerAngles;
			if (Rotation.z != 0f && Rotation.z == 180f && Rotation.y == 180f)
			{
				Rotation = new Vector3(-180f, 0f, 0f);
			}
			Vector3 localScale = obj.transform.localScale;
			Scale = new Vector2(localScale.z, localScale.y);
		}

		public void InitGround()
		{
			Vector3 position = VisibleObject.transform.position;
			m_AttachedGround = ResourcesManager.Instance.AddNewGround(VisibleObject);
			LevelObjectOffsetFromPosition = position - VisibleObject.transform.position;
		}

		public bool HasMirrorObject()
		{
			return MirrorObject != null;
		}

		private void AddVegetationSeed(int seed)
		{
			VegetationSeed = seed;
		}

		public void AddVegetationProps(int seed)
		{
			ClearVegetationProps();
			AddVegetationSeed(seed);
			if (HasVegetation)
			{
				m_SpawnedProps = ResourcesManager.Instance.GenerateRandomThemeProps(VisibleObject, seed);
			}
		}

		private void ClearVegetationProps()
		{
			if (m_SpawnedProps != null)
			{
				GameObject[] spawnedProps = m_SpawnedProps;
				foreach (GameObject obj in spawnedProps)
				{
					Object.Destroy(obj);
				}
			}
		}

		private void AddMirrorObject(LevelObject mirrorObj)
		{
			MirrorObject = mirrorObj;
		}

		public SaveableLevelObject GetSaveableObject()
		{
			SaveableLevelObject saveableLevelObject = new SaveableLevelObject();
			saveableLevelObject.PositionX = Position.x;
			saveableLevelObject.PositionY = Position.y;
			saveableLevelObject.RotationX = Rotation.x;
			saveableLevelObject.RotationY = Rotation.y;
			saveableLevelObject.ScaleX = Scale.x;
			saveableLevelObject.ScaleY = Scale.y;
			saveableLevelObject.ObjectID = Id;
			saveableLevelObject.PropsSeed = VegetationSeed;
			if (m_IsNetworkObject)
			{
				NetworkID = NetworkIDSetter++;
				Debug.Log("Setting New NetworkObject With ID: " + NetworkID);
			}
			saveableLevelObject.NetworkID = NetworkID;
			saveableLevelObject.HasMirrorObject = HasMirrorObject();
			Debug.Log("Retuning SaveableObject: " + saveableLevelObject.PositionX + " : " + saveableLevelObject.PositionY + " Rot: " + saveableLevelObject.RotationX + " : " + saveableLevelObject.RotationY);
			return saveableLevelObject;
		}

		public void UpdateGround()
		{
			ResourcesManager.Instance.UpdateGroundMaterial(m_AttachedGround);
		}

		public void ForceUpdateAccordingToObject()
		{
			Vector3 position = VisibleObject.transform.position;
			Position = new Vector2(position.y, position.z);
			Rotation = VisibleObject.transform.rotation.eulerAngles;
			Vector3 localScale = VisibleObject.transform.localScale;
			Scale = new Vector2(localScale.z, localScale.y);
		}
	}
}
