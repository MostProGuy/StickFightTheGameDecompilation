using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace LevelEditor
{
	public class LevelCreator : MonoBehaviour
	{
		public enum SnapType
		{
			Corner = 0,
			Face = 1
		}

		public enum SnapAxis
		{
			Both = 0,
			XAxis = 1,
			YAxis = 2
		}

		public enum SnapMask
		{
			None = 0,
			Drag = 1,
			Corner = 2
		}

		public struct Vector2Pair
		{
			public Vector2 Vec_A;

			public Vector2 Vec_B;
		}

		public struct SnapPosition
		{
			private Vector3 CornerPoint;

			private Vector2Pair Face;

			private Vector3 Difference;

			public Vector3 Point
			{
				get
				{
					return CornerPoint;
				}
			}

			public SnapAxis Axis { get; private set; }

			public SnapType Type { get; private set; }

			public SnapPosition(Vector3 p)
			{
				CornerPoint = p;
				Axis = SnapAxis.Both;
				Type = SnapType.Corner;
				Face = default(Vector2Pair);
				Difference = Vector3.zero;
			}

			public SnapPosition(Vector3 p, SnapAxis a)
			{
				CornerPoint = p;
				Axis = a;
				Type = SnapType.Corner;
				Face = default(Vector2Pair);
				Difference = Vector3.zero;
			}

			public SnapPosition(Vector2Pair p, Vector3 point, SnapAxis a)
			{
				Face = p;
				Axis = a;
				Type = SnapType.Face;
				CornerPoint = point;
				Difference = Vector3.zero;
			}

			public SnapPosition(SnapPosition p)
			{
				Face = p.Face;
				Axis = p.Axis;
				Type = p.Type;
				Difference = p.Difference;
				CornerPoint = p.Point;
			}

			public void SetNewDiff(Vector3 diff)
			{
				Vector3 zero = Vector3.zero;
				if (Axis == SnapAxis.XAxis)
				{
					zero.y = diff.y;
				}
				else if (Axis == SnapAxis.YAxis)
				{
					zero.z = diff.z;
				}
				Difference = zero;
			}

			public void SetNewPoint(Vector3 point)
			{
				CornerPoint = point;
			}

			public bool HasReachedThreshold(Vector3 mousePos, float threshold)
			{
				Vector3 vector = Point;
				if (Type == SnapType.Face)
				{
					vector = GetNewfaceSnapPoint(mousePos);
				}
				CornerPoint = vector;
				Vector3 vector2 = mousePos - vector;
				if (Type == SnapType.Face)
				{
					return vector2.magnitude > threshold * 2f;
				}
				switch (Axis)
				{
				case SnapAxis.Both:
					return vector2.magnitude > threshold;
				case SnapAxis.XAxis:
					return Mathf.Abs(vector2.z) > threshold;
				case SnapAxis.YAxis:
					return Mathf.Abs(vector2.y) > threshold;
				default:
					throw new Exception(string.Concat("No SNap Axis: ", Axis, " Is Handled!"));
				}
			}

			private Vector3 GetNewfaceSnapPoint(Vector3 mousePos)
			{
				Vector3 zero = Vector3.zero;
				if (Axis == SnapAxis.XAxis)
				{
					float z = mousePos.z;
					zero.z = Mathf.Clamp(z, Mathf.Min(Face.Vec_A.x, Face.Vec_B.x), Mathf.Max(Face.Vec_A.x, Face.Vec_B.x));
					zero.y = Face.Vec_A.y;
				}
				else if (Axis == SnapAxis.YAxis)
				{
					float y = mousePos.y;
					float y2 = Mathf.Clamp(y, Mathf.Min(Face.Vec_A.y, Face.Vec_B.y), Mathf.Max(Face.Vec_A.y, Face.Vec_B.y));
					zero.y = y2;
					zero.z = Face.Vec_A.x;
				}
				if (Axis == SnapAxis.Both)
				{
					throw new Exception("Cannot have both as snapAxis On Face!");
				}
				Debug.DrawLine(mousePos, zero, Color.blue, 1f, false);
				return zero + Difference;
			}
		}

		[StructLayout(LayoutKind.Sequential, Size = 1)]
		public struct SnapCorner
		{
			public float x
			{
				get
				{
					return Point.x;
				}
			}

			public float y
			{
				get
				{
					return Point.y;
				}
			}

			public Vector2 Point { get; private set; }

			public SnapMask Mask { get; private set; }

			public SnapCorner(Vector2 p, SnapMask m = SnapMask.None)
			{
				Point = p;
				Mask = m;
			}
		}

		[StructLayout(LayoutKind.Sequential, Size = 1)]
		public struct SnapFace
		{
			public Vector2 Vec_A
			{
				get
				{
					return Face.Vec_A;
				}
			}

			public Vector2 Vec_B
			{
				get
				{
					return Face.Vec_B;
				}
			}

			public Vector2Pair Face { get; private set; }

			public SnapMask Mask { get; private set; }

			public SnapFace(Vector2Pair f, SnapMask m = SnapMask.None)
			{
				Face = f;
				Mask = m;
			}
		}

		[Header("GRID")]
		[SerializeField]
		private int m_SizeX;

		[SerializeField]
		private Transform m_DragWindow;

		private Transform m_DragWindowMirror;

		private Transform[] m_SpawnPoints;

		private GameObject m_DragGroundObject;

		private GameObject m_DragGroundObjectMirror;

		private GameObject m_SelectedGameObject;

		private GameObject m_BrushObject;

		private GameObject m_MirrorBrushObject;

		private Vector3 m_BrushOffset;

		private bool m_ShowGrid;

		private bool m_AllowDragPlacedObjects = true;

		private MapGrid m_MapGrid;

		private MapSpace m_MapSpace;

		private Vector3 m_MousePosition;

		private Camera m_MainCamera;

		private bool m_Inited;

		private static ResourcesManager m_ResourcesManager;

		private static LevelManager m_LevelManager;

		private static LevelEditorInputManager m_EditorInputManager;

		private static LevelToolsHandler m_ToolsHandler;

		private static InterfaceManager m_InterfaceManager;

		private Action m_OnDragAction;

		private Action m_OnPlaceAction;

		private Action m_OnRemoveAction;

		private Action m_OnToggleChangedAction;

		private static LevelCreator _instance;

		private Vector3 m_MoveObjectOffset;

		private Vector3 m_BeginDragPosition;

		private Vector3 m_EndDragPosition;

		private bool m_IsDragging;

		private bool m_OrigoInMiddle;

		private SnapFace[] m_SnapFacesArray;

		private SnapCorner[] m_SnapCornersArray;

		private float m_SnapConnectThreshold = 0.5f;

		private float m_SnapBreakThreshold = 1f;

		private float m_SnapBreakThresholdSNAP = 1f;

		private float m_SnapBreakThresholdGRID = 0.1f;

		private SnapPosition m_CurrentSnapPosition;

		private GameObject m_SnapPartnerObject;

		private bool m_IsSnapping;

		public static LevelCreator Instance
		{
			get
			{
				return _instance;
			}
		}

		private void Awake()
		{
			_instance = this;
			m_MainCamera = Camera.main;
			m_DragWindowMirror = UnityEngine.Object.Instantiate(m_DragWindow);
			m_DragWindowMirror.name = "DragWindowMirror";
		}

		private void Start()
		{
			InitEditorSettings();
			InitReferences();
			InitActions();
			InitSpawnPoints();
			m_SelectedGameObject = m_ResourcesManager.GetFirstObject();
			MakeNewBrush();
			GenerateSnapCornersFaces();
		}

		public bool GetGridVisibility()
		{
			return m_ShowGrid;
		}

		public void ShowGrid(bool b)
		{
			m_ShowGrid = b;
			m_MapGrid.Show(m_ShowGrid);
		}

		public void Init()
		{
			m_Inited = true;
		}

		public void AddOnDragAction(Action a)
		{
			m_OnDragAction = (Action)Delegate.Combine(m_OnDragAction, a);
		}

		public void AddOnPlaceAction(Action a)
		{
			m_OnPlaceAction = (Action)Delegate.Combine(m_OnPlaceAction, a);
		}

		public void AddOnRemoveAction(Action a)
		{
			m_OnRemoveAction = (Action)Delegate.Combine(m_OnRemoveAction, a);
		}

		public void AddOnToggleChangedAction(Action a)
		{
			m_OnToggleChangedAction = (Action)Delegate.Combine(m_OnToggleChangedAction, a);
		}

		public void OnWeaponBrushChanged()
		{
			if (m_ToolsHandler.CurrentToolState == LevelToolsHandler.ToolState.WeaponPlacing)
			{
				m_SelectedGameObject = WeaponSelectionHandler.GetSelectableWeaponByIndex(EditorWeaponUI.CurrentSelectedWeapon).WeaponObject;
				MakeNewBrush();
			}
		}

		private void InitEditorSettings()
		{
			Vector3 position = new Vector3(0f, 0f, 0f);
			Vector3 position2 = new Vector3(1f, 1f, 0f);
			Vector3 botLeft = m_MainCamera.ViewportToWorldPoint(position);
			Vector3 topright = m_MainCamera.ViewportToWorldPoint(position2);
			m_MapSpace = new MapSpace(botLeft, topright);
			m_MapGrid = new MapGrid(m_MapSpace, m_SizeX);
		}

		private void InitReferences()
		{
			m_ResourcesManager = ResourcesManager.Instance;
			m_LevelManager = LevelManager.Instance;
			m_EditorInputManager = LevelEditorInputManager.Instance;
			m_ToolsHandler = LevelToolsHandler.Instance;
			m_InterfaceManager = InterfaceManager.Instance;
			m_SnapCornersArray = new SnapCorner[0];
			m_SnapFacesArray = new SnapFace[0];
		}

		private void InitSpawnPoints()
		{
			Transform transform = GameObject.Find("Map").transform;
			m_SpawnPoints = new Transform[4];
			m_SpawnPoints[0] = transform.Find("1");
			m_SpawnPoints[1] = transform.Find("2");
			m_SpawnPoints[2] = transform.Find("3");
			m_SpawnPoints[3] = transform.Find("4");
			m_LevelManager.AddSpawnPointRefs(m_SpawnPoints);
		}

		private void InitActions()
		{
			m_ToolsHandler.ClearActions();
			m_ToolsHandler.AddOnMirrorAction(OnMirrorStateChanged);
			m_ToolsHandler.AddOnSnapAction(OnSnapStateChanged);
			m_ToolsHandler.AddOnToolAction(OnToolChanged);
			m_LevelManager.AddOnClearedAction(GenerateSnapCornersFaces);
			m_MapGrid.AddOnSnapChangedAction(OnMapGridSnapChanged);
			LevelEditorInputManager.AddOnInputStateChangedAction(OnInputStateChanged);
		}

		public void Destruct()
		{
			m_ToolsHandler.Destruct();
			m_ResourcesManager.Destruct();
			m_LevelManager.Destruct();
			m_ToolsHandler.Destruct();
			m_EditorInputManager.Destruct();
			m_InterfaceManager.Destruct();
			_instance = null;
			m_ResourcesManager = null;
			m_LevelManager = null;
			m_EditorInputManager = null;
			m_ToolsHandler = null;
			m_InterfaceManager = null;
		}

		private void RotateObject(GameObject go, Vector3 rotate)
		{
			go.transform.Rotate(rotate);
		}

		private void Update()
		{
			if (!m_Inited)
			{
				return;
			}
			if (LevelEditorInputManager.CanUseMouse && !DialougePanelUI.IsOpen && m_ToolsHandler != null)
			{
				UpdateMousePosition();
				if (m_ToolsHandler.CurrentToolState == LevelToolsHandler.ToolState.Placing)
				{
					if (DidPressRotate())
					{
						if (m_ToolsHandler.CurrentToolState == LevelToolsHandler.ToolState.Dragging || m_ToolsHandler.CurrentToolState == LevelToolsHandler.ToolState.Placing)
						{
							RotateObject(m_BrushObject, new Vector3(90f, 0f, 0f));
							return;
						}
					}
					else if (DidPressFlip() && (m_ToolsHandler.CurrentToolState == LevelToolsHandler.ToolState.Dragging || m_ToolsHandler.CurrentToolState == LevelToolsHandler.ToolState.Placing))
					{
						if ((bool)m_BrushObject.GetComponent<ProprFlipAroundYIndeadOfX>())
						{
							RotateObject(m_BrushObject, new Vector3(0f, 180f, 0f));
						}
						else
						{
							RotateObject(m_BrushObject, new Vector3(180f, 0f, 0f));
						}
						return;
					}
				}
				if (DidClickPlace() && (DidhitSpawnPoint() || DidHitPlaceable()))
				{
					return;
				}
				DeleteObject();
				switch (m_ToolsHandler.CurrentToolState)
				{
				case LevelToolsHandler.ToolState.Placing:
					PlaceObject();
					break;
				case LevelToolsHandler.ToolState.Dragging:
					CheckForDragInput();
					break;
				case LevelToolsHandler.ToolState.WeaponPlacing:
					PlaceWeapon();
					break;
				}
			}
			if (LevelEditorInputManager.CanUseKeyBoard)
			{
				CheckForToolSwitch();
			}
		}

		private void OnInputStateChanged(bool useMouse, bool useKeyboard)
		{
			Debug.Log("Input state changed!");
			if (!useKeyboard)
			{
				m_MapGrid.Hide();
			}
		}

		private void CheckForToolSwitch()
		{
			if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKeyDown(KeyCode.M))
			{
				EditorToolsUI.Instance.ToggleMirrorRotation();
			}
			else if (Input.GetKeyDown(KeyCode.M))
			{
				EditorToolsUI.Instance.ToggleMirror();
			}
			else if (Input.GetKeyDown(KeyCode.S))
			{
				EditorToolsUI.Instance.ToggleSnap();
			}
			else if (Input.GetKeyDown(KeyCode.Escape))
			{
				if ((bool)m_BrushObject)
				{
					UnityEngine.Object.Destroy(m_BrushObject);
				}
				if ((bool)m_MirrorBrushObject)
				{
					UnityEngine.Object.Destroy(m_MirrorBrushObject);
				}
			}
			if (Input.GetKeyDown(KeyCode.I))
			{
				PipetteOnCursor();
			}
			if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.X))
			{
				m_ShowGrid = !m_ShowGrid;
			}
			if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.D))
			{
				m_AllowDragPlacedObjects = !m_AllowDragPlacedObjects;
			}
			if (Input.GetKey(KeyCode.X))
			{
				m_MapGrid.Show(!m_ShowGrid);
			}
			else
			{
				m_MapGrid.Show(m_ShowGrid);
			}
		}

		private void CheckForDragInput()
		{
			if (IsHoldingPlace())
			{
				if (DidClickPlace())
				{
					if (!m_InterfaceManager.IsOutsideOfEditorArea())
					{
						BeginDrag();
					}
				}
				else
				{
					UpdateDrag();
				}
			}
			if (DidReleasedMouse())
			{
				EndDrag();
			}
		}

		private void UpdateDrag()
		{
			if (!m_IsDragging)
			{
				return;
			}
			if (m_IsSnapping && !m_MapGrid.UsingGrid)
			{
				if (m_CurrentSnapPosition.Axis == SnapAxis.XAxis)
				{
					m_CurrentSnapPosition.SetNewPoint(new Vector3(2f, m_MousePosition.y, m_CurrentSnapPosition.Point.z));
				}
				else if (m_CurrentSnapPosition.Axis == SnapAxis.YAxis)
				{
					m_CurrentSnapPosition.SetNewPoint(new Vector3(2f, m_CurrentSnapPosition.Point.y, m_MousePosition.z));
				}
			}
			m_EndDragPosition = ((!m_IsSnapping) ? new Vector3(2f, m_MousePosition.y, m_MousePosition.z) : m_CurrentSnapPosition.Point);
			Rect rect = new Rect(m_BeginDragPosition.z, m_BeginDragPosition.y, m_BeginDragPosition.z - m_EndDragPosition.z, m_BeginDragPosition.y - m_EndDragPosition.y);
			Vector3 localScale = new Vector3(1f, rect.height, rect.width);
			if (!m_OrigoInMiddle)
			{
				Vector3 vector = new Vector3(0f, localScale.y / 2f, localScale.z / 2f);
				Vector3 vector2 = m_BeginDragPosition - vector;
				m_DragWindow.position = vector2;
				if (m_ToolsHandler.IsMirroring)
				{
					Vector3 mirroredPosition = GetMirroredPosition(vector2);
					m_DragWindowMirror.position = mirroredPosition;
					m_DragWindowMirror.localScale = localScale;
				}
			}
			m_DragWindow.localScale = localScale;
			UpdateTempGround();
		}

		private void UpdateTempGround()
		{
			Vector3 position = m_DragWindow.position;
			Vector3 lossyScale = m_DragWindow.lossyScale;
			float num = Mathf.Abs(lossyScale.y) / 2f;
			Vector3 vector = new Vector3(1f, position.y + num, position.z);
			Vector3 lossyScale2 = m_DragGroundObject.transform.lossyScale;
			m_DragGroundObject.transform.localScale = new Vector3(1f, lossyScale2.y, lossyScale.z);
			float num2 = m_DragGroundObject.transform.lossyScale.y / 2f;
			vector = new Vector3(vector.x, vector.y - num2, vector.z);
			m_DragGroundObject.transform.position = vector;
			if (m_ToolsHandler.IsMirroring)
			{
				position = m_DragWindowMirror.position;
				lossyScale = m_DragWindowMirror.lossyScale;
				num = Mathf.Abs(lossyScale.y) / 2f;
				vector = new Vector3(1f, position.y + num, position.z);
				lossyScale2 = m_DragGroundObjectMirror.transform.lossyScale;
				m_DragGroundObjectMirror.transform.localScale = new Vector3(1f, lossyScale2.y, lossyScale.z);
				num2 = m_DragGroundObjectMirror.transform.lossyScale.y / 2f;
				vector = new Vector3(vector.x, vector.y - num2, vector.z);
				m_DragGroundObjectMirror.transform.position = vector;
				Debug.Log(string.Concat("DragObj Mirrror: ", vector, " Scale: ", lossyScale2));
			}
		}

		private void EndDrag()
		{
			if (!m_IsDragging)
			{
				return;
			}
			m_IsDragging = false;
			Debug.Log("End Drag! " + m_EndDragPosition);
			m_DragWindow.gameObject.SetActive(false);
			m_DragWindowMirror.gameObject.SetActive(false);
			Vector3 start = new Vector3(0f, m_BeginDragPosition.y, m_BeginDragPosition.z);
			Vector3 vector = new Vector3(0f, m_EndDragPosition.y, m_EndDragPosition.z);
			bool flag = m_MapSpace.IsOnDifferentSides(start, vector);
			if (flag && m_ToolsHandler.IsMirroring)
			{
				float distanceToMiddle = m_MapSpace.GetDistanceToMiddle(start);
				float num = 0f - m_MapSpace.GetDistanceToMiddle(vector);
				float num2 = distanceToMiddle;
				if (Mathf.Abs(distanceToMiddle) < Mathf.Abs(num))
				{
					Debug.Log("Extended to right side!");
					float num3 = num - distanceToMiddle;
					start.z -= num3;
					num2 = num;
				}
				vector.z = start.z + num2 * 2f;
			}
			PlaceDraggedObject(start, vector, m_ToolsHandler.IsMirroring && !flag);
			if (m_DragGroundObject != null)
			{
				UnityEngine.Object.Destroy(m_DragGroundObject);
			}
			if (m_DragGroundObjectMirror != null)
			{
				UnityEngine.Object.Destroy(m_DragGroundObjectMirror);
			}
		}

		private void BeginDrag()
		{
			if (m_DragGroundObject == null)
			{
				m_DragGroundObject = m_ResourcesManager.GetGroundObject();
				m_DragGroundObject.transform.position = new Vector3(0f, -100f, 0f);
				m_DragGroundObject.name = "DragGround";
				m_DragGroundObject.SetActive(true);
			}
			if (m_DragGroundObjectMirror == null)
			{
				m_DragGroundObjectMirror = m_ResourcesManager.GetGroundObject();
				m_DragGroundObjectMirror.transform.position = new Vector3(0f, -100f, 0f);
				m_DragGroundObjectMirror.name = "DragGroundMirror";
				m_DragGroundObjectMirror.SetActive(m_ToolsHandler.IsMirroring);
			}
			m_BeginDragPosition = new Vector3(2f, m_MousePosition.y, m_MousePosition.z);
			if (m_IsSnapping)
			{
				Vector2 position = new Vector2(m_MousePosition.z, m_MousePosition.y);
				SnapPosition cornerIndex;
				Vector2 argumentIndex;
				if (SearchForSnappableObjects(position, out cornerIndex, out argumentIndex))
				{
					Vector2 vector = cornerIndex.Point;
					m_BeginDragPosition = new Vector3(2f, vector.y, vector.x);
					Debug.Log("Begin Snap!");
				}
			}
			m_EndDragPosition = m_BeginDragPosition;
			m_IsDragging = true;
			m_DragWindow.position = m_BeginDragPosition;
			m_DragWindow.localScale = Vector3.zero;
			m_DragWindow.gameObject.SetActive(true);
			if (m_ToolsHandler.IsMirroring)
			{
				m_DragWindowMirror.localScale = Vector3.zero;
				m_DragWindowMirror.gameObject.SetActive(true);
			}
			UpdateDragMaterial();
			Debug.Log("Begin Drag");
		}

		private void UpdateDragMaterial()
		{
			ResourcesManager.Instance.UpdateThemeMaterial(m_DragWindow.gameObject);
			ResourcesManager.Instance.UpdateThemeMaterial(m_DragWindowMirror.gameObject);
		}

		private bool HasReachedSnapThreshold()
		{
			return m_CurrentSnapPosition.HasReachedThreshold(m_MousePosition, m_SnapBreakThreshold);
		}

		private void OnMapGridSnapChanged()
		{
			if (!m_MapGrid.UsingGrid)
			{
				EndSnap();
				m_SnapBreakThreshold = m_SnapBreakThresholdSNAP;
			}
			else
			{
				m_SnapBreakThreshold = m_SnapBreakThresholdGRID;
			}
		}

		private bool SearchForSnappableObjects(Vector2 position, out SnapPosition cornerIndex, out Vector2 argumentIndex)
		{
			SnapPosition snapPos;
			Vector2 argumentIndex2;
			bool result = SearchForSnappableObjects(new Vector2[1] { position }, out snapPos, out argumentIndex2);
			cornerIndex = snapPos;
			argumentIndex = argumentIndex2;
			return result;
		}

		private bool SearchForSnappableObjects(Vector2[] positions, out SnapPosition snapPos, out Vector2 argumentIndex)
		{
			Vector2 vector = Vector2.zero;
			Vector2 vector2 = Vector2.zero;
			float num = float.PositiveInfinity;
			float num2 = float.PositiveInfinity;
			Vector2[] array = ((!m_MapGrid.UsingGrid) ? GetValidSnapCorners() : m_MapGrid.GridPositions);
			int num3 = array.Length;
			for (int i = 0; i < num3; i++)
			{
				Vector2 vector3 = array[i];
				for (int j = 0; j < positions.Length; j++)
				{
					float magnitude = (positions[j] - vector3).magnitude;
					if (magnitude < num)
					{
						num = magnitude;
						vector2 = vector3;
						vector = positions[j];
					}
				}
			}
			Vector2 vector4 = Vector2.zero;
			Vector2 vector5 = Vector2.zero;
			Vector2Pair p = default(Vector2Pair);
			SnapAxis a = SnapAxis.Both;
			Vector2Pair[] validSnapFaces = GetValidSnapFaces();
			int num4 = validSnapFaces.Length;
			for (int k = 0; k < num4; k++)
			{
				Vector2Pair vector2Pair = validSnapFaces[k];
				SnapAxis snapAxis = SnapAxis.Both;
				for (int l = 0; l < positions.Length; l++)
				{
					Vector2 vector6 = vector2Pair.Vec_A - vector2Pair.Vec_B;
					if (vector6.x == 0f)
					{
						snapAxis = SnapAxis.YAxis;
					}
					else if (vector6.y == 0f)
					{
						snapAxis = SnapAxis.XAxis;
					}
					Vector2 zero = Vector2.zero;
					switch (snapAxis)
					{
					case SnapAxis.XAxis:
					{
						float x = positions[l].x;
						zero.x = Mathf.Clamp(x, vector2Pair.Vec_A.x, vector2Pair.Vec_B.x);
						zero.y = vector2Pair.Vec_A.y;
						Debug.DrawLine(new Vector3(0f, vector2Pair.Vec_B.y, vector2Pair.Vec_B.x), new Vector3(0f, vector2Pair.Vec_A.y, vector2Pair.Vec_A.x), Color.red);
						break;
					}
					case SnapAxis.YAxis:
					{
						float y = positions[l].y;
						float y2 = Mathf.Clamp(y, Mathf.Min(vector2Pair.Vec_A.y, vector2Pair.Vec_B.y), Mathf.Max(vector2Pair.Vec_A.y, vector2Pair.Vec_B.y));
						zero.y = y2;
						zero.x = vector2Pair.Vec_A.x;
						Debug.DrawLine(new Vector3(0f, vector2Pair.Vec_B.y, vector2Pair.Vec_B.x), new Vector3(0f, vector2Pair.Vec_A.y, vector2Pair.Vec_A.x), Color.yellow);
						Debug.DrawLine(new Vector3(0f, zero.y, zero.x), new Vector3(0f, positions[l].y, positions[l].x), Color.yellow);
						break;
					}
					}
					float magnitude2 = (positions[l] - zero).magnitude;
					if (magnitude2 < num2)
					{
						num2 = magnitude2;
						p = vector2Pair;
						vector5 = zero;
						vector4 = positions[l];
						a = snapAxis;
					}
				}
			}
			float num5 = float.PositiveInfinity;
			if (num < num2)
			{
				SnapPosition snapPosition = new SnapPosition(vector2);
				snapPos = snapPosition;
				argumentIndex = vector;
				num5 = num;
			}
			else
			{
				float num6 = 0.5f;
				float num7 = num - num2;
				if (num7 > num6)
				{
					SnapPosition snapPosition2 = new SnapPosition(p, vector5, a);
					snapPos = snapPosition2;
					argumentIndex = vector4;
					num5 = num2;
				}
				else
				{
					SnapPosition snapPosition3 = new SnapPosition(vector2);
					snapPos = snapPosition3;
					argumentIndex = vector;
					num5 = num;
				}
			}
			if (num5 <= m_SnapConnectThreshold)
			{
				return true;
			}
			return false;
		}

		private Vector2Pair[] GetValidSnapFaces()
		{
			List<Vector2Pair> list = new List<Vector2Pair>();
			SnapMask snapMask = ((m_ToolsHandler.CurrentToolState == LevelToolsHandler.ToolState.Dragging) ? SnapMask.Drag : SnapMask.Corner);
			int num = m_SnapFacesArray.Length;
			for (int i = 0; i < num; i++)
			{
				SnapFace snapFace = m_SnapFacesArray[i];
				if (snapFace.Mask == snapMask || snapFace.Mask == SnapMask.None)
				{
					list.Add(snapFace.Face);
				}
			}
			return list.ToArray();
		}

		private Vector2[] GetValidSnapCorners()
		{
			List<Vector2> list = new List<Vector2>();
			SnapMask snapMask = ((m_ToolsHandler.CurrentToolState == LevelToolsHandler.ToolState.Dragging) ? SnapMask.Drag : SnapMask.Corner);
			int num = m_SnapCornersArray.Length;
			for (int i = 0; i < num; i++)
			{
				SnapCorner snapCorner = m_SnapCornersArray[i];
				if (snapCorner.Mask == snapMask || snapCorner.Mask == SnapMask.None)
				{
					list.Add(snapCorner.Point);
				}
			}
			return list.ToArray();
		}

		private void BeginSnap(SnapPosition p)
		{
			m_IsSnapping = true;
			m_CurrentSnapPosition = new SnapPosition(p);
			if ((bool)m_BrushObject)
			{
				m_BrushObject.transform.position = m_CurrentSnapPosition.Point;
			}
			Debug.Log("BeginSnap: " + m_CurrentSnapPosition.Point);
		}

		private void BeginSnap(Vector3 p, SnapAxis a)
		{
			m_IsSnapping = true;
			m_CurrentSnapPosition = new SnapPosition(p, a);
			if ((bool)m_BrushObject)
			{
				m_BrushObject.transform.position = m_CurrentSnapPosition.Point;
			}
			if (m_IsDragging)
			{
				m_EndDragPosition = m_CurrentSnapPosition.Point;
				UpdateDrag();
			}
			Debug.Log("BeginSnap: " + m_CurrentSnapPosition.Point);
		}

		private void EndSnap()
		{
			m_SnapPartnerObject = null;
			m_IsSnapping = false;
			UpdateDrag();
		}

		public void GenerateSnapCornersFaces()
		{
			int numberOfPlacedObjects = m_LevelManager.NumberOfPlacedObjects;
			int num = m_MapGrid.GridPositions.Length;
			List<SnapFace> list = new List<SnapFace>();
			List<SnapCorner> list2 = new List<SnapCorner>();
			LevelObject[] placedLevelObjects = m_LevelManager.PlacedLevelObjects;
			for (int i = 0; i < numberOfPlacedObjects; i++)
			{
				if (placedLevelObjects[i].ObjectProperties.GenerateSnapFaces)
				{
					Transform transform = placedLevelObjects[i].VisibleObject.transform;
					Collider collider = transform.GetComponent<Collider>();
					if (collider == null)
					{
						collider = transform.gameObject.AddComponent<BoxCollider>();
					}
					Vector3 size = collider.bounds.size;
					Vector3 center = collider.bounds.center;
					Vector2 vector = new Vector2(center.z - size.z / 2f, center.y + size.y / 2f);
					Vector2 vector2 = new Vector2(center.z + size.z / 2f, center.y + size.y / 2f);
					Vector2 vector3 = new Vector2(center.z - size.z / 2f, center.y - size.y / 2f);
					Vector2 vector4 = new Vector2(center.z + size.z / 2f, center.y - size.y / 2f);
					Vector2Pair f = new Vector2Pair
					{
						Vec_A = vector,
						Vec_B = vector3
					};
					Vector2Pair f2 = new Vector2Pair
					{
						Vec_A = vector2,
						Vec_B = vector4
					};
					Vector2Pair f3 = new Vector2Pair
					{
						Vec_A = vector,
						Vec_B = vector2
					};
					Vector2Pair f4 = new Vector2Pair
					{
						Vec_A = vector3,
						Vec_B = vector4
					};
					int num2 = i * 4;
					SnapMask m = SnapMask.None;
					if (placedLevelObjects[i].HasVegetation)
					{
						m = SnapMask.Corner;
					}
					list2.Add(new SnapCorner(vector, m));
					list2.Add(new SnapCorner(vector2, m));
					list.Add(new SnapFace(f3, m));
					m = SnapMask.None;
					list2.Add(new SnapCorner(vector3));
					list2.Add(new SnapCorner(vector4));
					list.Add(new SnapFace(f));
					list.Add(new SnapFace(f2));
					list.Add(new SnapFace(f4));
					if (placedLevelObjects[i].HasVegetation)
					{
						Debug.Log("Doing Corners For Drag...");
						size = transform.lossyScale;
						center = transform.position;
						vector = new Vector2(center.z - size.z / 2f, center.y + size.y / 2f);
						vector2 = new Vector2(center.z + size.z / 2f, center.y + size.y / 2f);
						f3 = new Vector2Pair
						{
							Vec_A = vector,
							Vec_B = vector2
						};
						list2.Add(new SnapCorner(vector, SnapMask.Drag));
						list2.Add(new SnapCorner(vector2, SnapMask.Drag));
						list.Add(new SnapFace(f3, SnapMask.Drag));
					}
				}
			}
			m_SnapCornersArray = list2.ToArray();
			m_SnapFacesArray = list.ToArray();
		}

		private void OnDrawGizmos()
		{
			if (!Application.isPlaying)
			{
				return;
			}
			if (m_SnapCornersArray != null)
			{
				SnapCorner[] snapCornersArray = m_SnapCornersArray;
				for (int i = 0; i < snapCornersArray.Length; i++)
				{
					SnapCorner snapCorner = snapCornersArray[i];
					if (snapCorner.Mask == SnapMask.None)
					{
						Gizmos.color = Color.red;
					}
					else if (snapCorner.Mask == SnapMask.Drag)
					{
						Gizmos.color = Color.blue;
					}
					else if (snapCorner.Mask == SnapMask.Corner)
					{
						Gizmos.color = Color.green;
					}
					Gizmos.DrawSphere(new Vector3(0f, snapCorner.y, snapCorner.x), 0.5f);
				}
			}
			Gizmos.color = Color.blue;
			if (m_SnapFacesArray != null)
			{
				SnapFace[] snapFacesArray = m_SnapFacesArray;
				for (int j = 0; j < snapFacesArray.Length; j++)
				{
					SnapFace snapFace = snapFacesArray[j];
					Gizmos.DrawLine(new Vector3(0f, snapFace.Vec_A.y, snapFace.Vec_A.x), new Vector3(0f, snapFace.Vec_B.y, snapFace.Vec_B.x));
				}
			}
			if (m_IsSnapping)
			{
				Gizmos.color = Color.green;
				Gizmos.DrawSphere(m_CurrentSnapPosition.Point, 1f);
			}
		}

		private void PlaceObject()
		{
			if (!IsHoldingPlace() || m_InterfaceManager.IsOutsideOfEditorArea() || !m_BrushObject || ((!m_ToolsHandler.IsMirroring) ? IsGameobjectOverlapping(m_BrushObject) : (IsGameobjectOverlapping(m_BrushObject) || IsGameobjectOverlapping(m_MirrorBrushObject))))
			{
				return;
			}
			Vector3 position = m_BrushObject.transform.position;
			GameObject gameObject = UnityEngine.Object.Instantiate(m_SelectedGameObject, position, m_BrushObject.transform.rotation);
			LevelObject levelObject = new LevelObject(gameObject, m_SelectedGameObject.name);
			EnableAfterFrame component = gameObject.GetComponent<EnableAfterFrame>();
			if ((bool)component)
			{
				component.obj.SetActive(true);
			}
			m_LevelManager.AddNewPlacedLevelObject(levelObject, true);
			if (m_ToolsHandler.IsMirroring)
			{
				Vector3 mirroredPosition = GetMirroredPosition(position);
				if (m_MirrorBrushObject.activeInHierarchy)
				{
					gameObject = UnityEngine.Object.Instantiate(m_SelectedGameObject, mirroredPosition, m_MirrorBrushObject.transform.rotation);
					if ((bool)component)
					{
						component = gameObject.GetComponent<EnableAfterFrame>();
						component.obj.SetActive(true);
					}
					LevelObject newObject = new LevelObject(gameObject, m_SelectedGameObject.name, levelObject);
					m_LevelManager.AddNewPlacedLevelObject(newObject, true);
				}
			}
			GenerateSnapCornersFaces();
			if (m_OnPlaceAction != null)
			{
				m_OnPlaceAction();
			}
		}

		private bool CheckRayForOverlapper(Vector3 dir, GameObject ignoreThis)
		{
			RaycastHit[] hits;
			CastRaycastFromScreenPos(dir, out hits);
			if (hits.Length > 0)
			{
				RaycastHit[] array = hits;
				foreach (RaycastHit raycastHit in array)
				{
					GameObject gameObject = raycastHit.collider.transform.root.gameObject;
					if (!(gameObject == ignoreThis) && m_LevelManager.ContainsObject(gameObject))
					{
						Debug.Log("Overlaping with: ", gameObject);
						return true;
					}
				}
			}
			return false;
		}

		private bool CheckBoxRayForOverlapper(Vector3 brushPosition, Vector3 scale, GameObject ignoreThis)
		{
			Ray ray = m_MainCamera.ScreenPointToRay(brushPosition);
			RaycastHit[] rayHit;
			if (CastBoxCast(brushPosition, scale, out rayHit))
			{
				RaycastHit[] array = rayHit;
				for (int i = 0; i < array.Length; i++)
				{
					RaycastHit raycastHit = array[i];
					GameObject gameObject = raycastHit.collider.transform.root.gameObject;
					if (!(gameObject == ignoreThis))
					{
						if (m_LevelManager.ContainsObject(gameObject))
						{
							Debug.Log("Overlaping with: ", gameObject);
							return true;
						}
						Debug.Log("HItting with box cast: " + raycastHit.collider.name, raycastHit.collider);
					}
				}
				brushPosition = new Vector3(-15f, brushPosition.y, brushPosition.z);
				DebugExtensions.DrawBox(brushPosition, scale, Quaternion.identity, Color.yellow);
			}
			return false;
		}

		private bool IsGameobjectOverlapping(GameObject gameObject, GameObject ignoreThis = null)
		{
			Vector3 position = gameObject.transform.position;
			Vector3 lossyScale = gameObject.transform.lossyScale;
			Vector3 position2 = new Vector3(0f, position.y, position.z);
			Vector3 position3 = new Vector3(0f, position.y + lossyScale.y / 2.1f, position.z - lossyScale.z / 2.1f);
			Vector3 position4 = new Vector3(0f, position.y + lossyScale.y / 2.1f, position.z + lossyScale.z / 2.1f);
			Vector3 position5 = new Vector3(0f, position.y - lossyScale.y / 2.1f, position.z - lossyScale.z / 2.1f);
			Vector3 position6 = new Vector3(0f, position.y - lossyScale.y / 2.1f, position.z + lossyScale.z / 2.1f);
			if (CheckRayForOverlapper(m_MainCamera.WorldToScreenPoint(position2), ignoreThis))
			{
				return true;
			}
			if (CheckRayForOverlapper(m_MainCamera.WorldToScreenPoint(position3), ignoreThis))
			{
				return true;
			}
			if (CheckRayForOverlapper(m_MainCamera.WorldToScreenPoint(position4), ignoreThis))
			{
				return true;
			}
			if (CheckRayForOverlapper(m_MainCamera.WorldToScreenPoint(position5), ignoreThis))
			{
				return true;
			}
			if (CheckRayForOverlapper(m_MainCamera.WorldToScreenPoint(position6), ignoreThis))
			{
				return true;
			}
			if (CheckBoxRayForOverlapper(position, lossyScale * 0.95f, ignoreThis))
			{
				return true;
			}
			return false;
		}

		private void PlaceWeapon()
		{
			if (DidClickPlace() && !m_InterfaceManager.IsOutsideOfEditorArea() && (bool)m_BrushObject)
			{
				Vector3 position = m_BrushObject.transform.position;
				int currentSelectedWeapon = EditorWeaponUI.CurrentSelectedWeapon;
				GameObject weaponObject = WeaponSelectionHandler.GetSelectableWeaponByIndex(currentSelectedWeapon).WeaponObject;
				GameObject gameObject = UnityEngine.Object.Instantiate(weaponObject, position, m_BrushObject.transform.rotation);
				m_LevelManager.StripObject(gameObject);
				WeaponObject weaponObject2 = new WeaponObject(gameObject, currentSelectedWeapon);
				m_LevelManager.AddNewPlacedLevelWeaponObject(weaponObject2);
				if (m_ToolsHandler.IsMirroring)
				{
					Vector3 mirroredPosition = GetMirroredPosition(position);
					gameObject = UnityEngine.Object.Instantiate(weaponObject, mirroredPosition, m_MirrorBrushObject.transform.rotation);
					m_LevelManager.StripObject(gameObject);
					WeaponObject newObject = new WeaponObject(gameObject, currentSelectedWeapon, weaponObject2);
					m_LevelManager.AddNewPlacedLevelWeaponObject(newObject);
				}
			}
		}

		private Vector3 GetMirroredPosition(Vector3 pos)
		{
			Vector3 mirroredPosition = m_MapSpace.GetMirroredPosition(pos);
			if (Mathf.Abs(m_MapSpace.GetDistanceToMiddle(mirroredPosition)) < 0.5f)
			{
				if (m_MirrorBrushObject != null)
				{
					m_MirrorBrushObject.SetActive(false);
				}
			}
			else if (m_MirrorBrushObject != null)
			{
				m_MirrorBrushObject.SetActive(true);
			}
			return mirroredPosition;
		}

		private Quaternion GetMirroredRotation(bool doMirrorRot)
		{
			int num = 0;
			if (doMirrorRot)
			{
				num = 180;
			}
			Quaternion result = Quaternion.identity;
			if ((bool)m_BrushObject)
			{
				if ((bool)m_BrushObject.GetComponent<ProprFlipAroundYIndeadOfX>())
				{
					Vector3 eulerAngles = m_BrushObject.transform.rotation.eulerAngles;
					eulerAngles = new Vector3(eulerAngles.x, eulerAngles.y + (float)num, eulerAngles.z);
					result = Quaternion.Euler(eulerAngles);
				}
				else
				{
					Vector3 eulerAngles2 = m_BrushObject.transform.rotation.eulerAngles;
					eulerAngles2 = new Vector3(eulerAngles2.x + (float)num, eulerAngles2.y, eulerAngles2.z);
					result = Quaternion.Euler(eulerAngles2);
				}
			}
			else if ((bool)m_BrushObject)
			{
				result = m_BrushObject.transform.rotation;
			}
			return result;
		}

		private void PlaceDraggedObject(Vector3 start, Vector3 end, bool mirror)
		{
			Rect rect = new Rect(start.z, start.y, start.z - end.z, start.y - end.y);
			Vector3 localScale = new Vector3(1f, rect.height, rect.width);
			if (!(Mathf.Abs(localScale.y) < 0.5f) && !(Mathf.Abs(localScale.z) < 0.5f))
			{
				Vector3 vector = new Vector3(0f, localScale.y / 2f, localScale.z / 2f);
				Vector3 vector2 = start - vector;
				GameObject gameObject = UnityEngine.Object.Instantiate(m_SelectedGameObject, vector2, Quaternion.identity);
				gameObject.transform.localScale = localScale;
				LevelObject levelObject = new LevelObject(gameObject, m_SelectedGameObject.name);
				levelObject.InitGround();
				m_LevelManager.AddNewPlacedLevelObject(levelObject, true);
				Debug.DrawLine(start, end, Color.red, 5f, false);
				int seed = GenerateNewSeed();
				levelObject.AddVegetationProps(seed);
				if (mirror)
				{
					seed = GenerateNewSeed();
					Vector3 mirroredPosition = GetMirroredPosition(vector2);
					gameObject = UnityEngine.Object.Instantiate(m_SelectedGameObject, mirroredPosition, Quaternion.identity);
					gameObject.transform.localScale = localScale;
					LevelObject levelObject2 = new LevelObject(gameObject, m_SelectedGameObject.name, levelObject);
					levelObject2.InitGround();
					m_LevelManager.AddNewPlacedLevelObject(levelObject2, true);
					levelObject2.AddVegetationProps(seed);
				}
				GenerateSnapCornersFaces();
				if (m_OnDragAction != null)
				{
					m_OnDragAction();
				}
			}
		}

		private bool IsMouseOverSpawnPoint(out Transform hitObject)
		{
			RaycastHit rayHit;
			if (CastRaycastFromMouse(out rayHit))
			{
				Transform transform = rayHit.collider.transform;
				Transform[] spawnPoints = m_SpawnPoints;
				foreach (Transform transform2 in spawnPoints)
				{
					if (transform2 == transform)
					{
						hitObject = transform;
						return true;
					}
				}
			}
			hitObject = null;
			return false;
		}

		private bool DidhitSpawnPoint()
		{
			Transform hitObject;
			if (IsMouseOverSpawnPoint(out hitObject))
			{
				StartCoroutine(BeginMoveTransform(hitObject));
				return true;
			}
			return false;
		}

		private bool DidHitPlaceable()
		{
			if (!m_AllowDragPlacedObjects)
			{
				return false;
			}
			RaycastHit rayHit;
			if (CastRaycastFromMouse(out rayHit))
			{
				GameObject gameObject = rayHit.collider.gameObject.transform.root.gameObject;
				string text = gameObject.name.Replace("(Clone)", string.Empty);
				bool flag = m_ResourcesManager.GetObjectByName(text) != null;
				if (text.Contains("Gun") || flag)
				{
					bool flag2 = text.Contains("GROUND");
					bool canFlipOrRotate = flag && !flag2;
					StartCoroutine(BeginMoveTransform(gameObject.transform, canFlipOrRotate, true, !flag2));
					return true;
				}
			}
			return false;
		}

		private IEnumerator BeginMoveTransform(Transform moveTransform, bool canFlipOrRotate = false, bool isPlacedObject = false, bool preventBlockablePositions = false)
		{
			if ((bool)m_BrushObject)
			{
				UnityEngine.Object.Destroy(m_BrushObject);
			}
			if ((bool)m_MirrorBrushObject)
			{
				UnityEngine.Object.Destroy(m_MirrorBrushObject);
			}
			m_MoveObjectOffset = moveTransform.position - m_MousePosition;
			m_MoveObjectOffset.x = 0f;
			Vector3 newPos = new Vector3(-1f, m_MousePosition.y, m_MousePosition.z);
			do
			{
				if (canFlipOrRotate)
				{
					HandleFlipOrRotateDraggedObject(moveTransform);
				}
				moveTransform.position = newPos + m_MoveObjectOffset;
				if (!m_InterfaceManager.IsOutsideOfEditorArea())
				{
					newPos = new Vector3(-1f, m_MousePosition.y, m_MousePosition.z);
				}
				yield return null;
			}
			while (IsHoldingPlace() || (preventBlockablePositions && IsGameobjectOverlapping(moveTransform.gameObject, moveTransform.gameObject)));
			if (isPlacedObject)
			{
				m_LevelManager.UpdatePlacedObject(moveTransform.gameObject);
			}
		}

		private void HandleFlipOrRotateDraggedObject(Transform transform)
		{
			if (transform.gameObject.name.ToLower().Contains("gun"))
			{
				return;
			}
			if (DidPressRotate())
			{
				m_MoveObjectOffset = Vector3.zero;
				RotateObject(transform.gameObject, new Vector3(90f, 0f, 0f));
			}
			else if (DidPressFlip())
			{
				m_MoveObjectOffset = Vector3.zero;
				if ((bool)transform.GetComponent<ProprFlipAroundYIndeadOfX>())
				{
					RotateObject(transform.gameObject, new Vector3(0f, 180f, 0f));
				}
				else
				{
					RotateObject(transform.gameObject, new Vector3(180f, 0f, 0f));
				}
			}
		}

		public static int GenerateNewSeed()
		{
			return UnityEngine.Random.Range(-2147483647, int.MaxValue);
		}

		private void DeleteObject()
		{
			RaycastHit rayHit;
			if (IsHoldingDelete() && CastRaycastFromMouse(out rayHit))
			{
				GameObject objectToRemove = rayHit.collider.transform.root.gameObject;
				bool flag = m_LevelManager.RemovePlacedLevelObject(objectToRemove);
				GenerateSnapCornersFaces();
				if (flag && m_OnRemoveAction != null)
				{
					m_OnRemoveAction();
				}
			}
		}

		private bool CastRaycastFromMouse(out RaycastHit rayHit)
		{
			Ray ray = m_MainCamera.ScreenPointToRay(Input.mousePosition);
			RaycastHit hitInfo;
			bool result = Physics.Raycast(ray, out hitInfo, float.PositiveInfinity);
			rayHit = hitInfo;
			return result;
		}

		private bool CastRaycastFromScreenPos(Vector3 screenPos, out RaycastHit rayHit)
		{
			Ray ray = m_MainCamera.ScreenPointToRay(screenPos);
			RaycastHit hitInfo;
			bool result = Physics.Raycast(ray, out hitInfo, float.PositiveInfinity);
			rayHit = hitInfo;
			Debug.DrawRay(ray.origin, ray.direction, Color.red, 5f);
			return result;
		}

		private void CastRaycastFromScreenPos(Vector3 screenPos, out RaycastHit[] hits)
		{
			Ray ray = m_MainCamera.ScreenPointToRay(screenPos);
			hits = Physics.RaycastAll(ray, float.PositiveInfinity);
			Debug.DrawRay(ray.origin, ray.direction, Color.red, 5f);
		}

		private bool CastBoxCast(Vector3 middle, Vector3 scale, out RaycastHit[] rayHit)
		{
			Ray ray = m_MainCamera.ScreenPointToRay(m_MainCamera.WorldToScreenPoint(middle));
			middle = new Vector3(-15f, middle.y, middle.z);
			return (rayHit = Physics.BoxCastAll(middle, scale / 2f, ray.direction)).Length > 0;
		}

		public static Vector3 CalculateCenter(GameObject obj)
		{
			Bounds bounds = default(Bounds);
			Transform[] componentsInChildren = obj.GetComponentsInChildren<Transform>();
			Transform[] array = componentsInChildren;
			foreach (Transform transform in array)
			{
				Renderer[] componentsInChildren2 = transform.GetComponentsInChildren<Renderer>();
				Renderer[] array2 = componentsInChildren2;
				foreach (Renderer renderer in array2)
				{
					if (bounds.size == Vector3.zero)
					{
						bounds = renderer.bounds;
					}
					else
					{
						bounds.Encapsulate(renderer.bounds);
					}
				}
				Collider[] componentsInChildren3 = transform.GetComponentsInChildren<Collider>();
				Collider[] array3 = componentsInChildren3;
				foreach (Collider collider in array3)
				{
					if (bounds.size == Vector3.zero)
					{
						bounds = collider.bounds;
					}
					else
					{
						bounds.Encapsulate(collider.bounds);
					}
				}
			}
			return -bounds.center;
		}

		private void UpdateMousePosition()
		{
			RaycastHit rayHit;
			if (m_ToolsHandler != null && CastRaycastFromMouse(out rayHit))
			{
				Vector3 point = rayHit.point;
				m_MousePosition = new Vector3(0f, point.y, point.z);
				if ((m_ToolsHandler.IsSnapping || m_MapGrid.UsingGrid) && HandleSnapping())
				{
					return;
				}
				if ((bool)m_BrushObject)
				{
					m_BrushObject.transform.position = m_MousePosition + m_BrushOffset;
				}
				if (m_ToolsHandler.IsMirroring)
				{
					Vector3 mirroredPosition = GetMirroredPosition(m_MousePosition + m_BrushOffset);
					Quaternion mirroredRotation = GetMirroredRotation(m_ToolsHandler.IsMirroringRotation);
					if ((bool)m_MirrorBrushObject)
					{
						m_MirrorBrushObject.transform.SetPositionAndRotation(mirroredPosition, mirroredRotation);
					}
				}
			}
			else
			{
				if ((bool)m_BrushObject)
				{
					m_BrushObject.transform.position = new Vector3(0f, -50f, 0f);
				}
				if ((bool)m_MirrorBrushObject)
				{
					m_MirrorBrushObject.transform.position = new Vector3(0f, -50f, 0f);
				}
			}
			Transform hitObject;
			if (IsMouseOverSpawnPoint(out hitObject))
			{
				CodeAnimation component = hitObject.GetComponent<CodeAnimation>();
				if (!component.IsPlaying)
				{
					component.Play();
					component.looping = true;
				}
				return;
			}
			Transform[] spawnPoints = m_SpawnPoints;
			foreach (Transform transform in spawnPoints)
			{
				CodeAnimation component2 = transform.GetComponent<CodeAnimation>();
				component2.looping = false;
			}
		}

		private void GetCornersOfBrushObject(out Vector2[] positions)
		{
			Vector3 mousePosition = m_MousePosition;
			Vector2[] array = new Vector2[0];
			if (m_ToolsHandler.CurrentToolState == LevelToolsHandler.ToolState.Placing)
			{
				if (m_BrushObject != null)
				{
					Vector3 lossyScale = m_BrushObject.transform.lossyScale;
					Vector2 vector = new Vector2(mousePosition.z - lossyScale.z / 2f, mousePosition.y + lossyScale.y / 2f);
					Vector2 vector2 = new Vector2(mousePosition.z + lossyScale.z / 2f, mousePosition.y + lossyScale.y / 2f);
					Vector2 vector3 = new Vector2(mousePosition.z - lossyScale.z / 2f, mousePosition.y - lossyScale.y / 2f);
					Vector2 vector4 = new Vector2(mousePosition.z + lossyScale.z / 2f, mousePosition.y - lossyScale.y / 2f);
					array = new Vector2[4] { vector, vector2, vector3, vector4 };
				}
			}
			else if (m_ToolsHandler.CurrentToolState == LevelToolsHandler.ToolState.Dragging)
			{
				Vector3 lossyScale2 = m_DragWindow.lossyScale;
				Vector2 vector5 = new Vector2(m_MousePosition.z, m_MousePosition.y);
				Vector2 vector6 = new Vector2(m_BeginDragPosition.z - lossyScale2.z, m_BeginDragPosition.y);
				Vector2 vector7 = new Vector2(m_BeginDragPosition.z, m_BeginDragPosition.y - lossyScale2.y);
				if (m_IsDragging)
				{
					if (m_MapGrid.UsingGrid)
					{
						array = new Vector2[1] { vector5 };
						Debug.DrawLine(new Vector3(0f, vector7.y, vector7.x), new Vector3(0f, vector5.y, vector5.x), Color.yellow);
					}
					else
					{
						array = new Vector2[3] { vector5, vector6, vector7 };
						Debug.DrawLine(m_BeginDragPosition, new Vector3(0f, vector6.y, vector6.x), Color.yellow, 2f);
						Debug.DrawLine(m_BeginDragPosition, new Vector3(0f, vector7.y, vector7.x), Color.yellow, 2f);
						Debug.DrawLine(new Vector3(0f, vector6.y, vector6.x), new Vector3(0f, vector5.y, vector5.x), Color.yellow);
						Debug.DrawLine(new Vector3(0f, vector7.y, vector7.x), new Vector3(0f, vector5.y, vector5.x), Color.yellow);
					}
				}
				else
				{
					array = new Vector2[1] { vector5 };
				}
			}
			positions = array;
		}

		private bool HandleSnapping()
		{
			if (m_IsSnapping && !m_MapGrid.UsingGrid)
			{
				if (!HasReachedSnapThreshold())
				{
					bool flag = false;
					if ((bool)m_BrushObject)
					{
						m_BrushObject.transform.position = m_CurrentSnapPosition.Point;
						Vector2[] positions;
						GetCornersOfBrushObject(out positions);
						SnapPosition snapPos;
						Vector2 argumentIndex;
						if (SearchForSnappableObjects(positions, out snapPos, out argumentIndex) && snapPos.Type == SnapType.Corner && m_CurrentSnapPosition.Type == SnapType.Face)
						{
							EndSnap();
							flag = true;
						}
					}
					if (m_ToolsHandler.IsMirroring && (bool)m_MirrorBrushObject)
					{
						Vector3 mirroredPosition = GetMirroredPosition(m_CurrentSnapPosition.Point);
						Quaternion mirroredRotation = GetMirroredRotation(m_ToolsHandler.IsMirroringRotation);
						m_MirrorBrushObject.transform.SetPositionAndRotation(mirroredPosition, mirroredRotation);
					}
					if (!flag)
					{
						return true;
					}
				}
				else
				{
					EndSnap();
				}
			}
			if (m_ToolsHandler.IsMirroring && (bool)m_MirrorBrushObject)
			{
				Vector3 mirroredPosition2 = GetMirroredPosition(m_CurrentSnapPosition.Point);
				m_MirrorBrushObject.transform.position = mirroredPosition2;
			}
			Vector3 mousePosition = m_MousePosition;
			Vector2[] positions2 = new Vector2[0];
			GetCornersOfBrushObject(out positions2);
			SnapPosition snapPos2;
			Vector2 argumentIndex2;
			if (SearchForSnappableObjects(positions2, out snapPos2, out argumentIndex2))
			{
				SnapAxis a = SnapAxis.Both;
				Vector2 vector = snapPos2.Point;
				Vector2 vector2 = argumentIndex2;
				if (m_IsDragging)
				{
					Vector2 vector3 = positions2[0];
					bool flag2 = vector2 != vector3;
					Vector3 vector4 = new Vector3(0f, vector.y, vector.x);
					if (flag2)
					{
						Vector2 vector5 = vector3 - vector;
						if (!m_MapGrid.UsingGrid)
						{
							if (Mathf.Abs(vector5.x) <= m_SnapConnectThreshold)
							{
								vector4 = new Vector3(0f, vector3.y, vector.x);
								a = SnapAxis.XAxis;
							}
							else
							{
								vector4 = new Vector3(0f, vector.y, vector3.x);
								a = SnapAxis.YAxis;
							}
						}
						else
						{
							a = SnapAxis.Both;
						}
					}
					snapPos2.SetNewPoint(vector4);
					BeginSnap(vector4, a);
				}
				else
				{
					Vector3 vector6 = new Vector3(0f, vector2.y, vector2.x);
					Vector3 vector7 = mousePosition - vector6;
					Vector3 newPoint = new Vector3(0f, vector.y, vector.x);
					newPoint += vector7;
					snapPos2.SetNewPoint(newPoint);
					snapPos2.SetNewDiff(vector7);
					BeginSnap(snapPos2);
				}
				return true;
			}
			return false;
		}

		public void SetSelectedGameObjectFromUI(string objectName)
		{
			Debug.Log("SetSelectedGameMode Called From Ui: " + objectName);
			SwitchSelectedGameObject(objectName);
		}

		private void SwitchSelectedGameObject(string objectName)
		{
			GameObject objectByName = m_ResourcesManager.GetObjectByName(objectName);
			if (objectByName == null)
			{
				Debug.LogError("Could Not Find Object with name: " + objectName);
				return;
			}
			Debug.Log("Switching selected gameobject to: " + objectName);
			string text = objectName.ToLower();
			LevelToolsHandler.ToolState toolState = LevelToolsHandler.ToolState.Placing;
			switch (text)
			{
			case "ground":
				toolState = LevelToolsHandler.ToolState.Dragging;
				break;
			case "weapon":
				toolState = LevelToolsHandler.ToolState.WeaponPlacing;
				break;
			default:
				toolState = LevelToolsHandler.ToolState.Placing;
				break;
			}
			LevelToolsHandler.SetNewToolState(toolState);
			m_SelectedGameObject = objectByName;
			MakeNewBrush();
		}

		private void DeleteBrush()
		{
			if (m_BrushObject != null)
			{
				UnityEngine.Object.Destroy(m_BrushObject);
			}
			if (m_MirrorBrushObject != null)
			{
				UnityEngine.Object.Destroy(m_MirrorBrushObject);
			}
		}

		private void PipetteOnCursor()
		{
			RaycastHit[] array = Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition));
			for (int i = 0; i < array.Length; i++)
			{
				GameObject gameObject = array[i].collider.gameObject.transform.root.gameObject;
				if (gameObject == m_BrushObject || gameObject == m_DragGroundObject)
				{
					continue;
				}
				string text = gameObject.name.Replace("(Clone)", string.Empty);
				if (text.Contains("Gun"))
				{
					GameObject gameObject2 = gameObject;
					if (!(gameObject2 == null))
					{
						string s = gameObject.name.Split('(')[0].Replace("Gun", string.Empty);
						int result;
						if (int.TryParse(s, out result))
						{
							LevelToolsHandler.SetNewToolState(LevelToolsHandler.ToolState.WeaponPlacing);
							EditorWeaponUI.CurrentSelectedWeapon = result;
							m_SelectedGameObject = WeaponSelectionHandler.GetSelectableWeaponByIndex(EditorWeaponUI.CurrentSelectedWeapon).WeaponObject;
							MakeNewBrush();
						}
						break;
					}
				}
				else
				{
					GameObject objectByName = m_ResourcesManager.GetObjectByName(text);
					if (!(objectByName == null))
					{
						SwitchSelectedGameObject(text);
						break;
					}
				}
			}
		}

		private void MakeNewBrush()
		{
			m_BrushOffset = Vector3.zero;
			if (m_BrushObject != null)
			{
				UnityEngine.Object.Destroy(m_BrushObject);
			}
			if (m_MirrorBrushObject != null)
			{
				UnityEngine.Object.Destroy(m_MirrorBrushObject);
			}
			m_BrushObject = UnityEngine.Object.Instantiate(m_SelectedGameObject, Vector3.zero, m_SelectedGameObject.transform.rotation);
			m_BrushOffset = CalculateCenter(m_BrushObject);
			Collider[] componentsInChildren = m_BrushObject.GetComponentsInChildren<Collider>();
			Collider[] array = componentsInChildren;
			foreach (Collider obj in array)
			{
				UnityEngine.Object.Destroy(obj);
			}
			m_MirrorBrushObject = UnityEngine.Object.Instantiate(m_BrushObject);
			componentsInChildren = m_MirrorBrushObject.GetComponentsInChildren<Collider>();
			Collider[] array2 = componentsInChildren;
			foreach (Collider obj2 in array2)
			{
				UnityEngine.Object.Destroy(obj2);
			}
			if (!m_ToolsHandler.IsMirroring)
			{
				m_MirrorBrushObject.SetActive(false);
			}
			EnableAfterFrame component = m_BrushObject.GetComponent<EnableAfterFrame>();
			if ((bool)component)
			{
				component.obj.SetActive(true);
				component = m_MirrorBrushObject.GetComponent<EnableAfterFrame>();
				component.obj.SetActive(true);
			}
			m_LevelManager.StripObject(m_BrushObject);
			m_LevelManager.StripObject(m_MirrorBrushObject);
			m_BrushObject.name = "BRUSHOBJECT";
			m_MirrorBrushObject.name = "BRUSHOBJECT_MIRROR";
			PropSpecialBehaviourBase component2 = m_BrushObject.GetComponent<PropSpecialBehaviourBase>();
			if ((bool)component2)
			{
				component2.Begin();
				component2 = m_MirrorBrushObject.GetComponent<PropSpecialBehaviourBase>();
				component2.Begin();
			}
		}

		private static bool IsHoldingDelete()
		{
			return LevelEditorInputManager.IsHoldingDelete();
		}

		private static bool DidReleasedMouse()
		{
			return LevelEditorInputManager.DidReleasedMouse();
		}

		private static bool IsHoldingPlace()
		{
			return LevelEditorInputManager.IsHoldingPlace();
		}

		private static bool DidPressRotate()
		{
			return LevelEditorInputManager.DidPressRotate();
		}

		private static bool DidPressFlip()
		{
			return LevelEditorInputManager.DidPressFlip();
		}

		private static bool DidClickPlace()
		{
			return LevelEditorInputManager.DidClickPlace();
		}

		private static bool DidClickDelete()
		{
			return LevelEditorInputManager.DidClickDelete();
		}

		public void OnMirrorStateChanged()
		{
			if (m_ToolsHandler.IsMirroring)
			{
				UpdateMousePosition();
				if ((bool)m_MirrorBrushObject)
				{
					m_MirrorBrushObject.SetActive(true);
				}
				if (m_IsDragging)
				{
					m_DragWindowMirror.gameObject.SetActive(true);
					if ((bool)m_DragGroundObjectMirror)
					{
						m_DragGroundObjectMirror.SetActive(true);
					}
				}
			}
			else
			{
				if ((bool)m_MirrorBrushObject)
				{
					m_MirrorBrushObject.SetActive(false);
				}
				m_DragWindowMirror.gameObject.SetActive(false);
				if ((bool)m_DragGroundObjectMirror)
				{
					m_DragGroundObjectMirror.SetActive(false);
				}
			}
			if (m_OnToggleChangedAction != null)
			{
				m_OnToggleChangedAction();
			}
		}

		public void OnSnapStateChanged()
		{
			if (!m_ToolsHandler.IsSnapping)
			{
				EndSnap();
			}
			if (m_OnToggleChangedAction != null)
			{
				m_OnToggleChangedAction();
			}
		}

		public void OnToolChanged(LevelToolsHandler.ToolState state)
		{
			if (state != LevelToolsHandler.ToolState.Placing && state == LevelToolsHandler.ToolState.Dragging)
			{
				m_IsDragging = false;
				m_DragWindow.gameObject.SetActive(false);
				m_DragWindowMirror.gameObject.SetActive(false);
			}
		}

		public void OnPlayTestStarted()
		{
			if ((bool)m_BrushObject)
			{
				m_BrushObject.SetActive(false);
			}
			if ((bool)m_MirrorBrushObject)
			{
				m_MirrorBrushObject.SetActive(false);
			}
			Transform[] spawnPoints = m_SpawnPoints;
			foreach (Transform transform in spawnPoints)
			{
				transform.gameObject.SetActive(false);
			}
		}

		public void OnPlayTestEnded()
		{
			if ((bool)m_BrushObject)
			{
				m_BrushObject.SetActive(true);
			}
			if (m_ToolsHandler.IsMirroring && (bool)m_MirrorBrushObject)
			{
				m_MirrorBrushObject.SetActive(true);
			}
			Transform[] spawnPoints = m_SpawnPoints;
			foreach (Transform transform in spawnPoints)
			{
				transform.gameObject.SetActive(true);
			}
			GenerateSnapCornersFaces();
		}
	}
}
