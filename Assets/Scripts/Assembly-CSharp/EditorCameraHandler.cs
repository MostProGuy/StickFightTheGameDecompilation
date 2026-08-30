using System.Collections;
using UnityEngine;

public class EditorCameraHandler : MonoBehaviour
{
	private Camera m_MainCamera;

	private Vector2 m_StartRect;

	private static EditorCameraHandler _instance;

	public static EditorCameraHandler Instance
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

	private void Start()
	{
		m_MainCamera = Camera.main;
		Rect rect = m_MainCamera.rect;
		m_StartRect = new Vector2(rect.x, rect.y);
	}

	public void FillScreen()
	{
		StartCoroutine(ChangeCameraViewPortCoroutine(Vector2.zero));
	}

	public void BackToEditorMode()
	{
		StartCoroutine(ChangeCameraViewPortCoroutine(m_StartRect));
	}

	private IEnumerator ChangeCameraViewPortCoroutine(Vector2 to)
	{
		m_MainCamera.rect = new Rect(to.x, to.y, 1f, 1f);
		yield return null;
	}
}
