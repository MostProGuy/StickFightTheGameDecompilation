using UnityEngine;

namespace LevelEditor
{
	public class MapSizeHandler : MonoBehaviour
	{
		public float mapSize = 10f;

		private float defaultMapSize;

		public Transform mapSizeFrame;

		private float currentMapSize = 10f;

		private float mapSizeVelocity;

		private float drag = 0.6f;

		private float spring = 0.3f;

		private bool hard;

		private float extraSize;

		public const float DefaultMapSize = 10f;

		public const float maxMapSize = 15f;

		public const float minMapSize = 5f;

		public static MapSizeHandler Instance { get; private set; }

		public float GetMapScale()
		{
			return mapSize / 15f;
		}

		private void Awake()
		{
			Instance = this;
		}

		private void Start()
		{
			defaultMapSize = mapSize;
		}

		private void FixedUpdate()
		{
			mapSizeVelocity += (mapSize + extraSize - currentMapSize) * spring;
			mapSizeVelocity *= drag;
			currentMapSize += mapSizeVelocity;
			if (hard)
			{
				currentMapSize = mapSize;
			}
			mapSizeFrame.localScale = Vector3.one * currentMapSize;
		}

		private void Update()
		{
			if (WorkshopStateHandler.IsPlayTestingMode)
			{
				extraSize = 0.5f;
				return;
			}
			extraSize = 0f;
			if (Input.GetKeyDown(KeyCode.H))
			{
				hard = !hard;
			}
			float num = mapSize;
			num -= Input.GetAxis("Mouse ScrollWheel") * 10f;
			mapSizeVelocity -= Input.GetAxis("Mouse ScrollWheel") * 1f;
			num = Mathf.Clamp(num, 5f, 15f);
			if (num == mapSize)
			{
				return;
			}
			mapSize = num;
			if (mapSize > defaultMapSize)
			{
				BackGround[] array = Object.FindObjectsOfType<BackGround>();
				BackGround[] array2 = array;
				foreach (BackGround backGround in array2)
				{
					backGround.transform.localScale = backGround.StartScale * mapSize / defaultMapSize;
				}
			}
		}

		public void ScaleMe(BackGround bg)
		{
			bg.transform.localScale = bg.StartScale * mapSize / defaultMapSize;
		}

		public void LoadSize(float size)
		{
			mapSize = size;
			mapSizeFrame.localScale = Vector3.one * mapSize;
			BackGround[] array = Object.FindObjectsOfType<BackGround>();
			BackGround[] array2 = array;
			foreach (BackGround bg in array2)
			{
				ScaleMe(bg);
			}
		}
	}
}
