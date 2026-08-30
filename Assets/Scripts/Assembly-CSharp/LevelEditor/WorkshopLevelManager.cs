using System;
using System.Collections.Generic;
using UnityEngine;

namespace LevelEditor
{
	public class WorkshopLevelManager : MonoBehaviour
	{
		private static MapWrapper m_MapWrapper;

		public static int NetworkIDSetterOnLoad;

		public static CustomLevel LastLoadedLevel { get; private set; }

		private static string GetMapData()
		{
			switch ((MapType)m_MapWrapper.MapType)
			{
			case MapType.Landfall:
				return "Hej";
			case MapType.CustomLocal:
				return BitConverter.ToString(m_MapWrapper.MapData);
			case MapType.CustomOnline:
				return BitConverter.ToUInt64(m_MapWrapper.MapData, 0).ToString();
			default:
				return "Invalid MapType";
			}
		}

		public static void SetNewLoadedLevel(CustomLevel newLevel)
		{
			LastLoadedLevel = newLevel;
			ThemeButtonsUI.Instance.SetNewTheme(newLevel.Theme);
			MapWrapper mapWrapper = new MapWrapper();
			mapWrapper.MapType = 0;
			m_MapWrapper = mapWrapper;
		}

		public static void SetNewLoadedLevel(CustomLevel newLevel, MapWrapper wrapper)
		{
			LastLoadedLevel = newLevel;
			m_MapWrapper = wrapper;
		}

		public static bool InitCurrentLoadedLevel(bool playtest = false)
		{
			SaveableLevelObject[] objects = (LastLoadedLevel.PlacedObjects ?? new List<SaveableLevelObject>()).ToArray();
			SaveableWeaponObject[] objects2 = (LastLoadedLevel.PlacedWeapons ?? new List<SaveableWeaponObject>()).ToArray();
			SerializableVector2[] spawnPoints = LastLoadedLevel.SpawnPoints;
			GameObject gameObject = new GameObject("Map_Custom " + GetMapData());
			GameObject gameObject2 = UnityEngine.Object.Instantiate(ResourcesManager.Instance.GetBackground(LastLoadedLevel.Theme), gameObject.transform);
			gameObject2.transform.position = new Vector3(10f, 0f, 0f);
			NetworkIDSetterOnLoad = 0;
			SpawnLevelObjects(gameObject, objects);
			SpawnWeaponObjects(gameObject, objects2);
			SpawnSpawnPoints(gameObject, spawnPoints);
			if (!playtest)
			{
				gameObject.transform.position = new Vector3(0f, 0f, -40f);
			}
			return true;
		}

		private static void SpawnWeaponObjects(GameObject mapParent, SaveableWeaponObject[] objects)
		{
			int num = objects.Length;
			for (int i = 0; i < num; i++)
			{
				SaveableWeaponObject saveableWeaponObject = objects[i];
				Vector3 position = new Vector3(0f, saveableWeaponObject.PositionX, saveableWeaponObject.PositionY);
				GameObject weaponObject = WeaponSelectionHandler.GetSelectableWeaponByIndex(saveableWeaponObject.WeaponIndex).WeaponObject;
				GameObject gameObject = UnityEngine.Object.Instantiate(weaponObject, position, Quaternion.identity, mapParent.transform);
				if (WorkshopStateHandler.IsPlayTestingMode)
				{
					WeaponObject newObject = new WeaponObject(gameObject, saveableWeaponObject.WeaponIndex);
					LevelManager.Instance.AddNewPlacedLevelWeaponObject(newObject);
				}
				if (MatchmakingHandler.IsNetworkMatch && saveableWeaponObject.NetworkID != -1)
				{
					WeaponPickUp component = gameObject.GetComponent<WeaponPickUp>();
					component.flyUpAfter = float.PositiveInfinity;
					component.InitGroundWeapon();
					NetworkSyncableObject networkSyncableObject = gameObject.FetchComponent<NetworkSyncableObject>();
					networkSyncableObject.InitNetworkIndex((ushort)saveableWeaponObject.NetworkID, true);
					networkSyncableObject.enabled = true;
				}
			}
		}

		private static void SpawnSpawnPoints(GameObject mapParent, SerializableVector2[] spawnPoints)
		{
			MapInfo mapInfo = mapParent.AddComponent<MapInfo>();
			if (WorkshopStateHandler.IsPlayTestingMode)
			{
				Debug.Log("Spawning Players...");
				int num = 1;
				GameObject characterObject = ResourcesManager.Instance.CharacterObject;
				Vector3 position = new Vector3(0f, spawnPoints[0].Y, spawnPoints[0].X);
				GameObject gameObject = UnityEngine.Object.Instantiate(characterObject, position, Quaternion.identity);
				CharacterActions characterActions = CharacterActions.CreateWithAnyBindings();
				characterActions.Device = null;
				gameObject.GetComponent<Controller>().TakeLocalControl(characterActions);
				Debug.Log("Spawning Player: " + num++);
				LevelObject newObject = new LevelObject(gameObject, gameObject.name);
				LevelManager.Instance.AddNewPlacedLevelObject(newObject);
				return;
			}
			mapInfo.spawnPoints = new Transform[4];
			int num2 = 0;
			if (spawnPoints == null)
			{
				for (int i = 0; i < 4; i++)
				{
					Transform transform = new GameObject("SpawnPoint" + num2).transform;
					transform.position = Vector3.zero;
					mapInfo.spawnPoints[num2] = transform;
					transform.parent = mapParent.transform;
					num2++;
				}
				return;
			}
			for (int j = 0; j < spawnPoints.Length; j++)
			{
				SerializableVector2 serializableVector = spawnPoints[j];
				Transform transform2 = new GameObject("SpawnPoint" + num2).transform;
				transform2.position = new Vector3(0f, serializableVector.Y, serializableVector.X);
				mapInfo.spawnPoints[num2] = transform2;
				transform2.parent = mapParent.transform;
				num2++;
			}
		}

		private static void SpawnLevelObjects(GameObject mapParent, SaveableLevelObject[] objects)
		{
			int num = objects.Length;
			for (int i = 0; i < num; i++)
			{
				SaveableLevelObject saveableLevelObject = objects[i];
				Vector3 vector = new Vector3(0f, saveableLevelObject.PositionX, saveableLevelObject.PositionY);
				Vector3 euler = new Vector3(saveableLevelObject.RotationX, saveableLevelObject.RotationY, 0f);
				Quaternion rotation = Quaternion.Euler(euler);
				Vector3 localScale = new Vector3(1f, saveableLevelObject.ScaleY, saveableLevelObject.ScaleX);
				GameObject objectByName = ResourcesManager.Instance.GetObjectByName(saveableLevelObject.ObjectID);
				if (!objectByName)
				{
					continue;
				}
				GameObject gameObject = UnityEngine.Object.Instantiate(objectByName, vector, rotation, mapParent.transform);
				ConfigurableJoint[] componentsInChildren = gameObject.GetComponentsInChildren<ConfigurableJoint>();
				ConfigurableJoint[] array = componentsInChildren;
				foreach (ConfigurableJoint configurableJoint in array)
				{
					configurableJoint.connectedAnchor = vector + gameObject.transform.TransformDirection(Vector3.Scale(configurableJoint.transform.localScale, configurableJoint.anchor));
					configurableJoint.axis = gameObject.transform.TransformDirection(configurableJoint.axis);
				}
				LevelObjectProperties objectPropertiesFor = LevelObjectPropertiesFactory.GetObjectPropertiesFor(saveableLevelObject.ObjectID);
				if (objectPropertiesFor.HasEditorCollider)
				{
					UnityEngine.Object.Destroy(gameObject.GetComponent<Collider>());
				}
				gameObject.transform.localScale = localScale;
				if (saveableLevelObject.HasValidSeed)
				{
					ResourcesManager.Instance.GenerateRandomThemeProps(gameObject, saveableLevelObject.PropsSeed, LastLoadedLevel.Theme);
					ResourcesManager.Instance.AddNewGround(gameObject);
				}
				if (WorkshopStateHandler.IsPlayTestingMode)
				{
					LevelObject newObject = new LevelObject(gameObject, objectByName.name);
					LevelManager.Instance.AddNewPlacedLevelObject(newObject);
				}
				if (MatchmakingHandler.IsNetworkMatch)
				{
					NetworkSyncableObject[] componentsInChildren2 = gameObject.GetComponentsInChildren<NetworkSyncableObject>();
					NetworkSyncableObject[] array2 = componentsInChildren2;
					foreach (NetworkSyncableObject networkSyncableObject in array2)
					{
						networkSyncableObject.InitNetworkIndex((ushort)NetworkIDSetterOnLoad++, true);
						networkSyncableObject.enabled = true;
					}
				}
			}
		}
	}
}
