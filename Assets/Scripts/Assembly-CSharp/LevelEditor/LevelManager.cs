using System;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

namespace LevelEditor
{
	public class LevelManager : MonoBehaviour
	{
		private List<LevelObject> m_PlacedLevelObjects = new List<LevelObject>();

		private List<WeaponObject> m_PlacedWeaponObjects = new List<WeaponObject>();

		private Transform[] m_SpawnPoints;

		private MapSettings m_MapSettings;

		private Action m_OnClearedAction;

		private Action m_OnObjectAddedAction;

		private static LevelManager _instance;

		public MapSettings CurrentMapSettings
		{
			get
			{
				return m_MapSettings;
			}
		}

		public SerializableVector2[] SpawnPoints
		{
			get
			{
				SerializableVector2[] array = new SerializableVector2[4];
				for (int i = 0; i < array.Length; i++)
				{
					Vector3 position = m_SpawnPoints[i].position;
					array[i] = new SerializableVector2
					{
						X = position.z,
						Y = position.y
					};
				}
				return array;
			}
		}

		public List<SaveableLevelObject> GetSaveableLevelObjects
		{
			get
			{
				List<SaveableLevelObject> list = new List<SaveableLevelObject>();
				List<LevelObject> list2 = new List<LevelObject>();
				LevelObject.NetworkIDSetter = 0;
				foreach (LevelObject placedLevelObject in m_PlacedLevelObjects)
				{
					if (!list2.Contains(placedLevelObject))
					{
						list.Add(placedLevelObject.GetSaveableObject());
						if (placedLevelObject.HasMirrorObject())
						{
							list2.Add(placedLevelObject.MirrorObject);
							list.Add(placedLevelObject.MirrorObject.GetSaveableObject());
						}
					}
				}
				return list;
			}
		}

		public List<SaveableWeaponObject> GetSaveableLevelWeaponObjects
		{
			get
			{
				List<SaveableWeaponObject> list = new List<SaveableWeaponObject>();
				List<WeaponObject> list2 = new List<WeaponObject>();
				foreach (WeaponObject placedWeaponObject in m_PlacedWeaponObjects)
				{
					if (!list2.Contains(placedWeaponObject))
					{
						list.Add(placedWeaponObject.GetSaveableObject());
						if (placedWeaponObject.HasMirrorObject())
						{
							list2.Add(placedWeaponObject.MirrorObject);
							list.Add(placedWeaponObject.MirrorObject.GetSaveableObject());
						}
					}
				}
				return list;
			}
		}

		public int NumberOfPlacedObjects
		{
			get
			{
				return m_PlacedLevelObjects.Count;
			}
		}

		public LevelObject[] PlacedLevelObjects
		{
			get
			{
				return m_PlacedLevelObjects.ToArray();
			}
		}

		public static LevelManager Instance
		{
			get
			{
				return _instance;
			}
		}

		private void Awake()
		{
			_instance = this;
			InitMapSettings();
		}

		public void Destruct()
		{
			_instance = null;
		}

		public void AddSpawnPointRefs(Transform[] points)
		{
			m_SpawnPoints = points;
		}

		public void AddOnClearedAction(Action a)
		{
			m_OnClearedAction = (Action)Delegate.Combine(m_OnClearedAction, a);
		}

		public void AddOnObjectAddedAction(Action a)
		{
			m_OnObjectAddedAction = (Action)Delegate.Combine(m_OnObjectAddedAction, a);
		}

		private void InitMapSettings()
		{
			m_MapSettings = new MapSettings
			{
				Theme = 0
			};
		}

		public void AddNewPlacedLevelObject(LevelObject newObject, bool strip = false)
		{
			m_PlacedLevelObjects.Add(newObject);
			if (strip)
			{
				StripObject(newObject.VisibleObject);
			}
			PropSpecialBehaviourBase component = newObject.VisibleObject.GetComponent<PropSpecialBehaviourBase>();
			if ((bool)component)
			{
				component.Exit();
			}
			if (m_OnObjectAddedAction != null)
			{
				m_OnObjectAddedAction();
			}
		}

		public void AddNewPlacedLevelWeaponObject(WeaponObject newObject)
		{
			m_PlacedWeaponObjects.Add(newObject);
			if (m_OnObjectAddedAction != null)
			{
				m_OnObjectAddedAction();
			}
		}

		public void StripObject(GameObject obj)
		{
			MonoBehaviour[] componentsInChildren = obj.GetComponentsInChildren<MonoBehaviour>();
			MonoBehaviour[] array = componentsInChildren;
			foreach (MonoBehaviour monoBehaviour in array)
			{
				if (!(monoBehaviour is PropSpecialBehaviourBase))
				{
					UnityEngine.Object.Destroy(monoBehaviour);
				}
			}
			Joint[] componentsInChildren2 = obj.GetComponentsInChildren<Joint>();
			Joint[] array2 = componentsInChildren2;
			foreach (Joint obj2 in array2)
			{
				UnityEngine.Object.Destroy(obj2);
			}
			ConstantForce[] componentsInChildren3 = obj.GetComponentsInChildren<ConstantForce>();
			ConstantForce[] array3 = componentsInChildren3;
			foreach (ConstantForce obj3 in array3)
			{
				UnityEngine.Object.Destroy(obj3);
			}
			Rigidbody[] componentsInChildren4 = obj.GetComponentsInChildren<Rigidbody>();
			Rigidbody[] array4 = componentsInChildren4;
			foreach (Rigidbody obj4 in array4)
			{
				UnityEngine.Object.Destroy(obj4);
			}
		}

		public bool ContainsObject(GameObject objectToSearch)
		{
			string text = "ground";
			LevelObject levelObjectFromGameObject = GetLevelObjectFromGameObject(objectToSearch);
			if (levelObjectFromGameObject != null)
			{
				if (levelObjectFromGameObject.Id.ToLower() == text.ToLower())
				{
					return false;
				}
				if (m_PlacedLevelObjects.Contains(levelObjectFromGameObject))
				{
					return true;
				}
			}
			WeaponObject levelWeaponObjectFromGameObject = GetLevelWeaponObjectFromGameObject(objectToSearch);
			if (levelWeaponObjectFromGameObject != null && m_PlacedWeaponObjects.Contains(levelWeaponObjectFromGameObject))
			{
				return true;
			}
			return false;
		}

		public bool UpdatePlacedObject(GameObject objectToEdit)
		{
			LevelObject levelObjectFromGameObject = GetLevelObjectFromGameObject(objectToEdit);
			if (levelObjectFromGameObject != null)
			{
				if (m_PlacedLevelObjects.Contains(levelObjectFromGameObject))
				{
					Vector3 position = levelObjectFromGameObject.VisibleObject.transform.position;
					Vector2 vector = new Vector2(levelObjectFromGameObject.LevelObjectOffsetFromPosition.y, levelObjectFromGameObject.LevelObjectOffsetFromPosition.x);
					levelObjectFromGameObject.Position = new Vector2(position.y, position.z) + vector;
					levelObjectFromGameObject.Rotation = levelObjectFromGameObject.VisibleObject.transform.rotation.eulerAngles;
					if (levelObjectFromGameObject.Rotation.z != 0f && levelObjectFromGameObject.Rotation.z == 180f && levelObjectFromGameObject.Rotation.y == 180f)
					{
						levelObjectFromGameObject.Rotation = new Vector3(-180f, 0f, 0f);
					}
				}
				return true;
			}
			WeaponObject levelWeaponObjectFromGameObject = GetLevelWeaponObjectFromGameObject(objectToEdit);
			if (levelWeaponObjectFromGameObject != null)
			{
				if (m_PlacedWeaponObjects.Contains(levelWeaponObjectFromGameObject))
				{
					levelWeaponObjectFromGameObject.InitPos(objectToEdit);
				}
				return true;
			}
			return false;
		}

		public bool RemovePlacedLevelObject(GameObject objectToRemove)
		{
			LevelObject levelObjectFromGameObject = GetLevelObjectFromGameObject(objectToRemove);
			if (levelObjectFromGameObject != null)
			{
				if (m_PlacedLevelObjects.Contains(levelObjectFromGameObject))
				{
					if (levelObjectFromGameObject.HasMirrorObject())
					{
						GameObject visibleObject = levelObjectFromGameObject.MirrorObject.VisibleObject;
						m_PlacedLevelObjects.Remove(levelObjectFromGameObject.MirrorObject);
						UnityEngine.Object.Destroy(visibleObject);
					}
					m_PlacedLevelObjects.Remove(levelObjectFromGameObject);
					UnityEngine.Object.Destroy(objectToRemove);
				}
				return true;
			}
			WeaponObject levelWeaponObjectFromGameObject = GetLevelWeaponObjectFromGameObject(objectToRemove);
			if (levelWeaponObjectFromGameObject != null)
			{
				if (m_PlacedWeaponObjects.Contains(levelWeaponObjectFromGameObject))
				{
					if (levelWeaponObjectFromGameObject.HasMirrorObject())
					{
						GameObject visibleObject2 = levelWeaponObjectFromGameObject.MirrorObject.VisibleObject;
						m_PlacedWeaponObjects.Remove(levelWeaponObjectFromGameObject.MirrorObject);
						UnityEngine.Object.Destroy(visibleObject2);
					}
					m_PlacedWeaponObjects.Remove(levelWeaponObjectFromGameObject);
					UnityEngine.Object.Destroy(objectToRemove);
				}
				return true;
			}
			if (objectToRemove.name != "Barriers")
			{
				Debug.LogError("Could not find object: " + objectToRemove.name);
			}
			return false;
		}

		private WeaponObject GetLevelWeaponObjectFromGameObject(GameObject obj)
		{
			foreach (WeaponObject placedWeaponObject in m_PlacedWeaponObjects)
			{
				if (placedWeaponObject.VisibleObject == obj)
				{
					return placedWeaponObject;
				}
			}
			return null;
		}

		private LevelObject GetLevelObjectFromGameObject(GameObject obj)
		{
			foreach (LevelObject placedLevelObject in m_PlacedLevelObjects)
			{
				if (placedLevelObject.VisibleObject == obj)
				{
					return placedLevelObject;
				}
			}
			return null;
		}

		public void ClearLevel(bool newMap = false)
		{
			foreach (LevelObject placedLevelObject in m_PlacedLevelObjects)
			{
				UnityEngine.Object.Destroy(placedLevelObject.VisibleObject);
			}
			m_PlacedLevelObjects.Clear();
			foreach (WeaponObject placedWeaponObject in m_PlacedWeaponObjects)
			{
				UnityEngine.Object.Destroy(placedWeaponObject.VisibleObject);
			}
			m_PlacedWeaponObjects.Clear();
			WeaponPickUp[] array = UnityEngine.Object.FindObjectsOfType<WeaponPickUp>();
			WeaponPickUp[] array2 = array;
			foreach (WeaponPickUp weaponPickUp in array2)
			{
				UnityEngine.Object.Destroy(weaponPickUp.gameObject);
			}
			RemoveOnLevelChange[] array3 = UnityEngine.Object.FindObjectsOfType<RemoveOnLevelChange>();
			RemoveOnLevelChange[] array4 = array3;
			foreach (RemoveOnLevelChange removeOnLevelChange in array4)
			{
				UnityEngine.Object.Destroy(removeOnLevelChange.gameObject);
			}
			MapInfo[] array5 = UnityEngine.Object.FindObjectsOfType<MapInfo>();
			foreach (MapInfo mapInfo in array5)
			{
				UnityEngine.Object.Destroy(mapInfo.gameObject);
			}
			if (m_OnClearedAction != null)
			{
				m_OnClearedAction();
			}
			if (newMap)
			{
				WorkshopDataHolder.Instance.workshopData.levelName = string.Empty;
				WorkshopDataHolder.Instance.workshopData.description = "Woop woop";
				WorkshopDataHolder.Instance.workshopData.path = string.Empty;
				WorkshopDataHolder.Instance.workshopData.directoryPath = string.Empty;
				WorkshopDataHolder.Instance.workshopData.publishedFileID = new PublishedFileId_t(0uL);
				WorkshopDataHolder.Instance.workshopData.isNew = true;
			}
		}

		public void SetNewMapTheme(int theme)
		{
			m_MapSettings.Theme = theme;
			Debug.Log("New Theme Is Set! " + theme);
		}

		public void GenerateNewVegetation()
		{
			foreach (LevelObject placedLevelObject in m_PlacedLevelObjects)
			{
				if (placedLevelObject.HasVegetation)
				{
					int num = LevelCreator.GenerateNewSeed();
					Debug.Log("New Seed: " + num);
					placedLevelObject.AddVegetationProps(num);
					placedLevelObject.UpdateGround();
				}
			}
		}

		public GameObject GetPlacedGameObjet()
		{
			return new GameObject();
		}

		public void PopulateLevel()
		{
			CustomLevel lastLoadedLevel = WorkshopLevelManager.LastLoadedLevel;
			PopulateLoadedMap(lastLoadedLevel);
		}

		private WeaponObject GetWeaponObject(SaveableWeaponObject obj, WeaponObject mirror = null)
		{
			Vector3 position = new Vector3(0f, obj.PositionX, obj.PositionY);
			GameObject weaponObject = WeaponSelectionHandler.GetSelectableWeaponByIndex(obj.WeaponIndex).WeaponObject;
			GameObject gameObject = UnityEngine.Object.Instantiate(weaponObject, position, weaponObject.transform.rotation);
			StripObject(gameObject);
			return new WeaponObject(gameObject, obj.WeaponIndex);
		}

		private LevelObject GetLevelObject(SaveableLevelObject obj, LevelObject mirror = null)
		{
			Vector3 vector = new Vector3(0f, obj.PositionX, obj.PositionY);
			Vector3 vector2 = new Vector3(obj.RotationX, obj.RotationY, 0f);
			Quaternion rotation = Quaternion.Euler(vector2);
			Vector3 vector3 = new Vector3(1f, obj.ScaleY, obj.ScaleX);
			int propsSeed = obj.PropsSeed;
			GameObject objectByName = ResourcesManager.Instance.GetObjectByName(obj.ObjectID);
			if (!objectByName)
			{
				return null;
			}
			Debug.Log(string.Concat("Instatiating new object: ", vector, " : ", vector2, " Scale: ", vector3));
			GameObject gameObject = UnityEngine.Object.Instantiate(objectByName, vector, rotation);
			gameObject.transform.localScale = vector3;
			LevelObject levelObject = new LevelObject(gameObject, objectByName.name, mirror, propsSeed);
			if (levelObject.HasVegetation)
			{
				levelObject.AddVegetationProps(propsSeed);
				levelObject.InitGround();
			}
			return levelObject;
		}

		private void PopulateLoadedMap(CustomLevel recentlyLoadedLevel)
		{
			Debug.Log("Loading Level..");
			SaveableLevelObject[] objects = (recentlyLoadedLevel.PlacedObjects ?? new List<SaveableLevelObject>()).ToArray();
			SaveableWeaponObject[] objects2 = (recentlyLoadedLevel.PlacedWeapons ?? new List<SaveableWeaponObject>()).ToArray();
			SerializableVector2[] spawnPoints = recentlyLoadedLevel.SpawnPoints;
			PopulateLevelObjects(objects);
			PopulateSpawnPoints(spawnPoints);
			PopulateWeapons(objects2);
			Debug.Log("Successfully loaded level!");
		}

		private void PopulateWeapons(SaveableWeaponObject[] objects)
		{
			int num = objects.Length;
			for (int i = 0; i < num; i++)
			{
				SaveableWeaponObject saveableWeaponObject = objects[i];
				WeaponObject weaponObject = GetWeaponObject(saveableWeaponObject);
				AddNewPlacedLevelWeaponObject(weaponObject);
				Debug.Log("Spawning gun at: " + weaponObject.Position);
				if (saveableWeaponObject.HasMirrorObject)
				{
					SaveableWeaponObject obj = objects[i + 1];
					WeaponObject weaponObject2 = GetWeaponObject(obj, weaponObject);
					AddNewPlacedLevelWeaponObject(weaponObject2);
					i++;
				}
			}
		}

		private void PopulateSpawnPoints(SerializableVector2[] spawnPoints)
		{
			int num = spawnPoints.Length;
			for (int i = 0; i < num; i++)
			{
				Vector3 position = new Vector3(-1f, spawnPoints[i].Y, spawnPoints[i].X);
				m_SpawnPoints[i].position = position;
			}
		}

		private void PopulateLevelObjects(SaveableLevelObject[] objects)
		{
			int num = objects.Length;
			for (int i = 0; i < num; i++)
			{
				SaveableLevelObject saveableLevelObject = objects[i];
				LevelObject levelObject = GetLevelObject(saveableLevelObject);
				if (levelObject == null)
				{
					continue;
				}
				LevelObjectProperties objectProperties = levelObject.ObjectProperties;
				EnableAfterFrame component = levelObject.VisibleObject.GetComponent<EnableAfterFrame>();
				if ((bool)component)
				{
					component.obj.SetActive(true);
				}
				AddNewPlacedLevelObject(levelObject, true);
				if (saveableLevelObject.HasMirrorObject)
				{
					SaveableLevelObject obj = objects[i + 1];
					LevelObject levelObject2 = GetLevelObject(obj, levelObject);
					if ((bool)component)
					{
						component = levelObject2.VisibleObject.GetComponent<EnableAfterFrame>();
						component.obj.SetActive(true);
					}
					AddNewPlacedLevelObject(levelObject2, true);
					i++;
				}
			}
		}
	}
}
