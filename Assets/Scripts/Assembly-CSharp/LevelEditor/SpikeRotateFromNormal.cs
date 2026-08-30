using UnityEngine;

namespace LevelEditor
{
	public class SpikeRotateFromNormal : PropSpecialBehaviourBase
	{
		public enum Direction
		{
			Right = 0,
			Left = 1,
			Up = 2,
			Down = 3
		}

		private bool m_IsUsing;

		private float m_Fractor = 1f;

		public override void Begin()
		{
			m_IsUsing = true;
		}

		public override void Exit()
		{
			Object.Destroy(this);
		}

		private void Update()
		{
			if (m_IsUsing)
			{
				DoRayCasts();
			}
		}

		private void DoRayCasts()
		{
			Vector3 position = base.transform.position;
			Ray ray = new Ray(position - Vector3.forward * m_Fractor, Vector3.forward);
			Ray ray2 = new Ray(position - Vector3.back * m_Fractor, Vector3.back);
			Ray ray3 = new Ray(position - Vector3.up * m_Fractor, Vector3.up);
			Ray ray4 = new Ray(position - Vector3.down * m_Fractor, Vector3.down);
			RaycastHit hitInfo;
			Physics.Raycast(ray, out hitInfo);
			RaycastHit hitInfo2;
			Physics.Raycast(ray2, out hitInfo2);
			RaycastHit hitInfo3;
			Physics.Raycast(ray3, out hitInfo3);
			RaycastHit hitInfo4;
			Physics.Raycast(ray4, out hitInfo4);
			float num = float.PositiveInfinity;
			Direction directionToTurn = Direction.Right;
			if (hitInfo.collider != null)
			{
				float distance = hitInfo.distance;
				if (distance < num && !hitInfo.collider.transform.root.name.ToLower().Contains("spike"))
				{
					num = distance;
					directionToTurn = Direction.Right;
				}
				Debug.DrawLine(position - Vector3.forward * m_Fractor, hitInfo.point, Color.green);
			}
			if (hitInfo2.collider != null)
			{
				float distance2 = hitInfo2.distance;
				if (distance2 < num && !hitInfo2.collider.transform.root.name.ToLower().Contains("spike"))
				{
					num = distance2;
					directionToTurn = Direction.Left;
				}
				Debug.DrawLine(position - Vector3.back * m_Fractor, hitInfo2.point, Color.red);
			}
			if (hitInfo3.collider != null)
			{
				float distance3 = hitInfo3.distance;
				if (distance3 < num && !hitInfo3.collider.transform.root.name.ToLower().Contains("spike"))
				{
					num = distance3;
					directionToTurn = Direction.Up;
				}
				Debug.DrawLine(position - Vector3.up * m_Fractor, hitInfo3.point, Color.blue);
			}
			if (hitInfo4.collider != null)
			{
				float distance4 = hitInfo4.distance;
				if (distance4 < num && !hitInfo4.collider.transform.root.name.ToLower().Contains("spike"))
				{
					num = distance4;
					directionToTurn = Direction.Down;
				}
				Debug.DrawLine(position - Vector3.down * m_Fractor, hitInfo4.point, Color.magenta);
			}
			float num2 = 2f;
			if (num <= num2)
			{
				RotateTowards(directionToTurn);
				Debug.Log("ClosestDirection: " + directionToTurn.ToString() + " Dis: " + num);
			}
		}

		private void RotateTowards(Direction directionToTurn)
		{
			Vector3 rotationForDirection = GetRotationForDirection(directionToTurn);
			base.transform.rotation = Quaternion.Euler(rotationForDirection);
		}

		private Vector3 GetRotationForDirection(Direction dir)
		{
			switch (dir)
			{
			case Direction.Right:
				return new Vector3(180f, 0f, 0f);
			case Direction.Left:
				return new Vector3(0f, 0f, 0f);
			case Direction.Up:
				return new Vector3(90f, 0f, 0f);
			case Direction.Down:
				return new Vector3(270f, 0f, 0f);
			default:
				return Vector3.zero;
			}
		}
	}
}
