using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LevelEditor
{
	public class DialougePanelUI : MonoBehaviour
	{
		[SerializeField]
		private GameObject m_Panel;

		[SerializeField]
		private TextMeshProUGUI m_Questiontext;

		[SerializeField]
		private Button m_YesButton;

		[SerializeField]
		private Button m_NoButton;

		[SerializeField]
		private Button m_CancelButton;

		private Action m_YesActionCallback;

		private Action m_NoActionCallback;

		private Action m_CancelCallback;

		private Action m_OnClickedAction;

		private Action m_OnClickedNoAction;

		private static DialougePanelUI _instance;

		public static bool IsOpen { get; private set; }

		public static DialougePanelUI Instance
		{
			get
			{
				return _instance;
			}
		}

		private void Awake()
		{
			_instance = this;
			if (IsShowing())
			{
				Hide();
			}
		}

		private void Start()
		{
			AddListeners();
		}

		public void AddOnClickedNoAction(Action a)
		{
			m_OnClickedNoAction = (Action)Delegate.Combine(m_OnClickedNoAction, a);
		}

		public void AddOnClickAction(Action a)
		{
			m_OnClickedAction = (Action)Delegate.Combine(m_OnClickedAction, a);
		}

		public void Prompt(string promptMessage)
		{
			Action yesAction = () =>
			{
			};
			GiveChoice(promptMessage, yesAction, null, null, "Ok", string.Empty, string.Empty);
		}

		public void Message(string message)
		{
			GiveChoice(message, null, null, null, string.Empty, string.Empty, string.Empty);
		}

		public void HideMessage()
		{
			Hide();
		}

		public void GiveChoice(string message, Action yesAction, Action noAction)
		{
			GiveChoice(message, yesAction, noAction, null, "Yes", "No", string.Empty);
		}

		public void GiveChoice(string message, Action yesAction, string yesMessage, Action noAction, string noMessage)
		{
			GiveChoice(message, yesAction, noAction, null, yesMessage, noMessage, string.Empty);
		}

		public void GiveChoice(string message, Action yesAction, string yesMessage, Action noAction, string noMessage, Action cancelAction, string cancelMessage)
		{
			GiveChoice(message, yesAction, noAction, cancelAction, yesMessage, noMessage, cancelMessage);
		}

		private void GiveChoice(string message, Action yesAction, Action noAction, Action cancelAction, string yesMessage, string noMessage, string cancelMessage)
		{
			ResetAllActions();
			HideAllButtons();
			m_Questiontext.text = message;
			if (yesAction != null)
			{
				m_YesActionCallback = yesAction;
				m_YesButton.GetComponentInChildren<TextMeshProUGUI>().text = yesMessage;
				m_YesButton.gameObject.SetActive(true);
			}
			if (noAction != null)
			{
				m_NoActionCallback = noAction;
				m_NoButton.GetComponentInChildren<TextMeshProUGUI>().text = noMessage;
				m_NoButton.gameObject.SetActive(true);
			}
			if (cancelAction != null)
			{
				m_CancelCallback = cancelAction;
				m_CancelButton.GetComponentInChildren<TextMeshProUGUI>().text = cancelMessage;
				m_CancelButton.gameObject.SetActive(true);
			}
			Show();
		}

		private void HideAllButtons()
		{
			m_YesButton.gameObject.SetActive(false);
			m_NoButton.gameObject.SetActive(false);
			m_CancelButton.gameObject.SetActive(false);
		}

		private void ResetAllActions()
		{
			m_YesActionCallback = null;
			m_NoActionCallback = null;
			m_CancelCallback = null;
		}

		private void AddListeners()
		{
			m_YesButton.onClick.AddListener(OnYesButtonClicked);
			m_NoButton.onClick.AddListener(OnNoButtonClicked);
			m_CancelButton.onClick.AddListener(OnCancelButtonClicked);
		}

		public void OnCancelButtonClicked()
		{
			Debug.Log("Calling Cancel Button Callback!");
			Hide();
			m_CancelCallback();
			if (m_OnClickedAction != null)
			{
				m_OnClickedAction();
			}
		}

		public void OnNoButtonClicked()
		{
			Debug.Log("Calling No Button Callback!");
			Hide();
			m_NoActionCallback();
			if (m_OnClickedNoAction != null)
			{
				m_OnClickedNoAction();
			}
		}

		public void OnYesButtonClicked()
		{
			Debug.Log("Calling Yes Button Callback!");
			Hide();
			m_YesActionCallback();
			if (m_OnClickedAction != null)
			{
				m_OnClickedAction();
			}
		}

		public bool IsShowing()
		{
			return m_Panel.activeInHierarchy;
		}

		private void Hide()
		{
			Debug.Log("Hiding");
			m_Panel.SetActive(false);
			IsOpen = false;
		}

		private void Show()
		{
			Debug.Log("Showing");
			m_Panel.SetActive(true);
			IsOpen = true;
		}
	}
}
