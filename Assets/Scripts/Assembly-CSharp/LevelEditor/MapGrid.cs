using System;
using UnityEngine;

namespace LevelEditor
{
	public class MapGrid
	{
		private GameObject m_LineCube;

		private GameObject m_Parent;

		private MapSpace m_Space;

		private int m_SizeX;

		private int m_SizeY;

		private Action m_OnChangedAction;

		public bool UsingGrid
		{
			get
			{
				return m_Parent.activeInHierarchy;
			}
		}

		public int SnapY { get; private set; }

		public int SnapX { get; private set; }

		public Vector2[] GridPositions { get; private set; }

		public MapGrid(MapSpace space, int sizeX)
		{
			m_LineCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
			m_LineCube.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
			m_LineCube.SetActive(false);
			m_LineCube.name = "GridLine";
			m_Space = space;
			m_SizeX = sizeX;
			float f = m_Space.GetLengthInY() / m_Space.GetLengthInX();
			m_SizeY = (int)((float)sizeX * Mathf.Abs(f));
			Populate();
			Hide();
		}

		private void Populate()
		{
			m_Parent = new GameObject("GridParent");
			GridPositions = new Vector2[(m_SizeX + 1) * (m_SizeY + 1)];
			Vector3 botLeft = m_Space.BotLeft;
			float num = m_Space.GetLengthInX() / (float)m_SizeX;
			float num2 = m_Space.GetLengthInY() / (float)m_SizeY;
			for (int i = 0; i < m_SizeX + 1; i++)
			{
				botLeft = m_Space.BotLeft;
				botLeft = new Vector2(botLeft.z + (float)i * num, botLeft.y);
				int num3 = i * (m_SizeY + 1);
				GridPositions[num3] = botLeft;
				for (int j = 1; j < m_SizeY + 1; j++)
				{
					botLeft = m_Space.BotLeft;
					botLeft = new Vector2(botLeft.z + (float)i * num, botLeft.y + (float)j * num2);
					num3 = i * (m_SizeY + 1) + j;
					GridPositions[num3] = botLeft;
				}
			}
			Vector2[] gridPositions = GridPositions;
			for (int k = 0; k < gridPositions.Length; k++)
			{
				Vector2 vector = gridPositions[k];
				GameObject gameObject = UnityEngine.Object.Instantiate(m_LineCube, new Vector3(0f, vector.y, vector.x), Quaternion.identity);
				gameObject.transform.SetParent(m_Parent.transform);
				gameObject.SetActive(true);
			}
		}

		public void Show(bool show)
		{
			if (show)
			{
				Show();
			}
			else
			{
				Hide();
			}
		}

		public void Show()
		{
			if (!m_Parent.activeInHierarchy)
			{
				m_Parent.SetActive(true);
				if (m_OnChangedAction != null)
				{
					m_OnChangedAction();
				}
			}
		}

		public void Hide()
		{
			if (m_Parent.activeInHierarchy)
			{
				m_Parent.SetActive(false);
				if (m_OnChangedAction != null)
				{
					m_OnChangedAction();
				}
			}
		}

		public void AddOnSnapChangedAction(Action a)
		{
			m_OnChangedAction = (Action)Delegate.Combine(m_OnChangedAction, a);
		}
	}
}
