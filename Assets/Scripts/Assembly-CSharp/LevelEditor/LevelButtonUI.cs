using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LevelEditor
{
	public class LevelButtonUI : EditorUIBase
	{
		[SerializeField]
		private TextMeshProUGUI m_LevelNameText;

		[SerializeField]
		private Image m_WorkshopIcon;

		[SerializeField]
		private Button m_LevelButton;

		[SerializeField]
		private Button m_deleteButton;

		private Action m_OnClickAction;

		private Action m_OnDeleteAction;

		private void Awake()
		{
			m_LevelButton.onClick.AddListener(DoAction);
			m_deleteButton.onClick.AddListener(DoDeleteAction);
		}

		public void Init(string levelName, bool isWorkshop, bool isOwned, Action onClickAction, Action onDeleteAction)
		{
			m_LevelNameText.text = levelName;
			m_WorkshopIcon.enabled = isWorkshop;
			m_OnClickAction = onClickAction;
			m_OnDeleteAction = onDeleteAction;
		}

		public void DoAction()
		{
			if (m_OnClickAction == null)
			{
				Debug.LogError("OnClick Action Is null: ", this);
			}
			m_OnClickAction();
		}

		private void DoDeleteAction()
		{
			if (m_OnDeleteAction == null)
			{
				Debug.LogError("OnDelete Action Is Null: ", this);
			}
			m_OnDeleteAction();
		}
	}
}
