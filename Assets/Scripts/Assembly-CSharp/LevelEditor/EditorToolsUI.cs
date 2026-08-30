using UnityEngine;
using UnityEngine.UI;

namespace LevelEditor
{
	public class EditorToolsUI : EditorUIBase
	{
		[SerializeField]
		private Button m_ClearButton;

		[SerializeField]
		private Toggle m_MirrorToggle;

		[SerializeField]
		private Toggle m_SnapToggle;

		[SerializeField]
		private Toggle m_MirrorRotationToggle;

		[SerializeField]
		private Toggle m_AudioToggle;

		private LevelManager m_LevelManager;

		private static EditorToolsUI _instance;

		public static EditorToolsUI Instance
		{
			get
			{
				return _instance;
			}
		}

		private void Awake()
		{
			_instance = this;
			AssignListeners();
		}

		private void Start()
		{
			m_LevelManager = LevelManager.Instance;
		}

		private void AssignListeners()
		{
			m_MirrorToggle.onValueChanged.AddListener(OnMirrorValueChanged);
			m_SnapToggle.onValueChanged.AddListener(OnSnapValueChanged);
			m_AudioToggle.onValueChanged.AddListener(OnAudioValueChanged);
			m_MirrorRotationToggle.onValueChanged.AddListener(OnMirrorRotationValueChanged);
			m_MirrorRotationToggle.interactable = m_MirrorToggle.isOn;
		}

		public void OnAudioValueChanged(bool isOn)
		{
			AudioListener.pause = !isOn;
		}

		public void OnSnapValueChanged(bool isOn)
		{
			LevelToolsHandler.SetNewSnapState(isOn);
		}

		public void OnMirrorValueChanged(bool isOn)
		{
			LevelToolsHandler.SetNewMirrorState(isOn);
			m_MirrorRotationToggle.interactable = m_MirrorToggle.isOn;
		}

		public void OnMirrorRotationValueChanged(bool isOn)
		{
			LevelToolsHandler.SetNewMirrorRotationState(isOn);
		}

		public void ToggleMirror()
		{
			m_MirrorToggle.isOn = !m_MirrorToggle.isOn;
		}

		public void ToggleSnap()
		{
			m_SnapToggle.isOn = !m_SnapToggle.isOn;
		}

		public void ToggleMirrorRotation()
		{
			if (!m_MirrorToggle.isOn && !m_MirrorRotationToggle.isOn)
			{
				ToggleMirror();
			}
			m_MirrorRotationToggle.isOn = !m_MirrorRotationToggle.isOn;
		}
	}
}
