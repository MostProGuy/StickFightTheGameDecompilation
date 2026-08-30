using UnityEngine;

namespace LevelEditor
{
	public class LevelEditorSoundHandler : MonoBehaviour
	{
		private AudioSource m_AudioSource;

		[SerializeField]
		private AudioClip[] m_PlaceSounds;

		[SerializeField]
		private AudioClip[] m_DragSounds;

		[SerializeField]
		private AudioClip m_ChangeThemeSound;

		[SerializeField]
		private AudioClip m_ChangeToggleSound;

		[SerializeField]
		private AudioClip[] m_RemoveSounds;

		[SerializeField]
		private AudioClip m_OkSound;

		[SerializeField]
		private AudioClip m_NoSound;

		private void Awake()
		{
			m_AudioSource = GetComponent<AudioSource>();
		}

		private void Start()
		{
			LevelCreator instance = LevelCreator.Instance;
			instance.AddOnPlaceAction(PlayPlaceSound);
			instance.AddOnDragAction(PlayDragSound);
			instance.AddOnRemoveAction(PlayRemoveSound);
			instance.AddOnToggleChangedAction(PlayChangeToggleSound);
			DialougePanelUI instance2 = DialougePanelUI.Instance;
			instance2.AddOnClickAction(PlayOkSound);
			instance2.AddOnClickedNoAction(PlayNoSound);
			ThemeButtonsUI instance3 = ThemeButtonsUI.Instance;
			instance3.AddOnThemeChangedAction(PlayChangeThemeSound);
		}

		public void PlayPlaceSound()
		{
			AudioClip audioClip = m_PlaceSounds[Random.Range(0, m_PlaceSounds.Length)];
			if (audioClip != null)
			{
				m_AudioSource.PlayOneShot(audioClip);
			}
		}

		public void PlayDragSound()
		{
			AudioClip audioClip = m_DragSounds[Random.Range(0, m_DragSounds.Length)];
			if (audioClip != null)
			{
				m_AudioSource.PlayOneShot(audioClip);
			}
		}

		public void PlayRemoveSound()
		{
			AudioClip audioClip = m_RemoveSounds[Random.Range(0, m_RemoveSounds.Length)];
			if (audioClip != null)
			{
				m_AudioSource.PlayOneShot(audioClip);
			}
		}

		public void PlayNoSound()
		{
			m_AudioSource.PlayOneShot(m_NoSound);
		}

		public void PlayOkSound()
		{
			m_AudioSource.PlayOneShot(m_OkSound);
		}

		public void PlayChangeToggleSound()
		{
			m_AudioSource.PlayOneShot(m_ChangeToggleSound);
		}

		public void PlayChangeThemeSound()
		{
			m_AudioSource.PlayOneShot(m_ChangeThemeSound);
		}
	}
}
