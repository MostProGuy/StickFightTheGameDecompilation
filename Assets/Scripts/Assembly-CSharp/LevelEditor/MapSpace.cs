using UnityEngine;

namespace LevelEditor
{
	public class MapSpace
	{
		private Vector3 m_BotLeft;

		private Vector3 m_TopRight;

		private float m_Middle;

		public Vector3 BotLeft
		{
			get
			{
				return m_BotLeft;
			}
		}

		public MapSpace(Vector3 botLeft, Vector3 topright)
		{
			m_BotLeft = new Vector3(0f, botLeft.y, botLeft.z);
			m_TopRight = new Vector3(0f, topright.y, topright.z);
			m_Middle = (m_BotLeft.z + m_TopRight.z) / 2f;
		}

		public bool IsOnDifferentSides(Vector3 start, Vector3 end)
		{
			return start.z < m_Middle != end.z < m_Middle;
		}

		public Vector3 GetMirroredPosition(Vector3 pos)
		{
			float num = pos.z - m_Middle;
			return new Vector3(pos.x, pos.y, m_Middle - num);
		}

		public float GetDistanceToMiddle(Vector3 start)
		{
			return m_Middle - start.z;
		}

		public float GetLengthInX()
		{
			return m_TopRight.z - m_BotLeft.z;
		}

		public float GetLengthInY()
		{
			return Mathf.Abs(m_TopRight.y - m_BotLeft.y);
		}
	}
}
