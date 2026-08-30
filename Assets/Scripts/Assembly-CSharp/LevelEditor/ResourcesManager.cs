using System;
using System.Collections.Generic;
using UnityEngine;

namespace LevelEditor
{
	public class ResourcesManager : MonoBehaviour
	{
		[SerializeField]
		private GameObject m_CharacterObject;

		[SerializeField]
		private GameObject[] m_PlaceableObjects;

		[SerializeField]
		private ThemeProps[] m_ThemeProps;

		[SerializeField]
		private GameObject[] m_NetworkSpawnableObjects;

		private static ResourcesManager _instance;

		private bool m_Tile;

		public GameObject[] PlaceableObjects
		{
			get
			{
				return m_PlaceableObjects;
			}
		}

		public GameObject CharacterObject
		{
			get
			{
				return m_CharacterObject;
			}
		}

		public GameObject[] NetworkSpawnableObjects
		{
			get
			{
				return m_NetworkSpawnableObjects;
			}
		}

		public static ResourcesManager Instance
		{
			get
			{
				return _instance;
			}
		}

		private void Awake()
		{
			_instance = this;
		}

		public void Destruct()
		{
			_instance = null;
		}

		public int GetNumberOfThemes()
		{
			return m_ThemeProps.Length;
		}

		public string GetThemeName(int index)
		{
			return m_ThemeProps[index].ThemeName;
		}

		public Material GetThemeMaterial(int overrideTheme = -1)
		{
			int num = ((overrideTheme != -1) ? overrideTheme : LevelManager.Instance.CurrentMapSettings.Theme);
			return m_ThemeProps[num].ThemeMaterial;
		}

		public GameObject AddNewGround(GameObject newBlock)
		{
			Vector3 position = newBlock.transform.position;
			Vector3 lossyScale = newBlock.transform.lossyScale;
			float num = Mathf.Abs(lossyScale.y) / 2f;
			Vector3 vector = new Vector3(0f, position.y + num, position.z);
			GameObject groundObject = GetGroundObject();
			if (!groundObject)
			{
				return groundObject;
			}
			Vector3 lossyScale2 = groundObject.transform.lossyScale;
			groundObject.transform.localScale = new Vector3(1f, lossyScale2.y, lossyScale.z);
			float num2 = groundObject.transform.lossyScale.y / 2f;
			vector = new Vector3(0.2f, vector.y - num2, vector.z);
			groundObject.transform.position = vector;
			newBlock.transform.position = new Vector3(position.x, position.y - num2, position.z);
			newBlock.transform.localScale = new Vector3(lossyScale.x, Mathf.Abs(lossyScale.y) - num2 * 2f, lossyScale.z);
			groundObject.transform.SetParent(newBlock.transform);
			BoxCollider component = newBlock.GetComponent<BoxCollider>();
			Vector3 center = component.center;
			center.y += groundObject.transform.lossyScale.y / 2f / newBlock.transform.lossyScale.y;
			component.center = center;
			Vector3 vector2 = component.size;
			vector2 = (component.size = new Vector3(vector2.x, vector2.y += groundObject.transform.lossyScale.y / Mathf.Abs(newBlock.transform.lossyScale.y), vector2.z));
			Debug.Log(string.Concat("Ground Col After: ", component.center, " : ", component.size));
			return groundObject;
		}

		public void UpdateGroundMaterial(GameObject ground)
		{
			int num = ((!LevelManager.Instance) ? WorkshopLevelManager.LastLoadedLevel.Theme : LevelManager.Instance.CurrentMapSettings.Theme);
			m_Tile = m_ThemeProps[num].ShallTile;
			Material groundMaterial = m_ThemeProps[num].GroundMaterial;
			if (groundMaterial != null)
			{
				ground.GetComponent<MeshRenderer>().material = groundMaterial;
			}
		}

		public void UpdateThemeMaterial(GameObject ground)
		{
			int num = ((!LevelManager.Instance) ? WorkshopLevelManager.LastLoadedLevel.Theme : LevelManager.Instance.CurrentMapSettings.Theme);
			m_Tile = m_ThemeProps[num].ShallTile;
			Material themeMaterial = m_ThemeProps[num].ThemeMaterial;
			if (themeMaterial != null)
			{
				ground.GetComponent<MeshRenderer>().material = themeMaterial;
			}
		}

		public GameObject[] GetRandomVegetationProps(int overrideTheme = -1)
		{
			int num = ((overrideTheme != -1) ? overrideTheme : LevelManager.Instance.CurrentMapSettings.Theme);
			GameObject[] vegetationProps = m_ThemeProps[num].VegetationProps;
			if (vegetationProps.Length <= 0)
			{
				return new GameObject[0];
			}
			List<GameObject> list = new List<GameObject>();
			int num2 = UnityEngine.Random.Range(1, 4);
			for (int i = 0; i < num2; i++)
			{
				int num3 = UnityEngine.Random.Range(0, vegetationProps.Length);
				list.Add(vegetationProps[num3]);
			}
			return list.ToArray();
		}

		public GameObject GetGroundObject()
		{
			int num = ((!LevelManager.Instance) ? WorkshopLevelManager.LastLoadedLevel.Theme : LevelManager.Instance.CurrentMapSettings.Theme);
			m_Tile = m_ThemeProps[num].ShallTile;
			Material groundMaterial = m_ThemeProps[num].GroundMaterial;
			GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
			if (groundMaterial != null)
			{
				gameObject.GetComponent<MeshRenderer>().material = groundMaterial;
			}
			UnityEngine.Object.Destroy(gameObject.GetComponent<Collider>());
			gameObject.transform.localScale = new Vector3(1f, 0.35f, 1f);
			return gameObject;
		}

		public GameObject GetBackground(int index)
		{
			return m_ThemeProps[index].BackGroundObject;
		}

		public GameObject GetFirstObject()
		{
			return m_PlaceableObjects[0];
		}

		public GameObject GetObjectByName(string objectName, bool caseSensetive = false)
		{
			string value = ((!caseSensetive) ? objectName.ToLower() : objectName);
			GameObject[] placeableObjects = m_PlaceableObjects;
			foreach (GameObject gameObject in placeableObjects)
			{
				string text = ((!caseSensetive) ? gameObject.name.ToLower() : gameObject.name);
				if (text.Equals(value))
				{
					return gameObject;
				}
			}
			Debug.LogWarning("No object with name: " + objectName + " Could be found in the database");
			return null;
		}

		public GameObject[] GenerateRandomThemeProps(GameObject newBlock, int seed, int overrideTheme = -1)
		{
			if (seed == int.MinValue)
			{
				Debug.Log("Invalid Seed");
				return null;
			}
			UnityEngine.Random.InitState(seed);
			Material themeMaterial = GetThemeMaterial(overrideTheme);
			newBlock.GetComponent<MeshRenderer>().material = themeMaterial;
			GameObject[] randomVegetationProps = GetRandomVegetationProps(overrideTheme);
			Vector3 position = new Vector3(1f, newBlock.transform.position.y, newBlock.transform.position.z);
			List<GameObject> list = new List<GameObject>();
			GameObject[] array = randomVegetationProps;
			foreach (GameObject gameObject in array)
			{
				GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject, position, gameObject.transform.rotation);
				gameObject2.SetActive(false);
				gameObject2.transform.SetParent(newBlock.transform);
				list.Add(gameObject2);
			}
			UnityEngine.Random.InitState((int)DateTime.Now.Ticks);
			return list.ToArray();
		}
	}
}
