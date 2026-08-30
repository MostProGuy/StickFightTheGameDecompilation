using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ChatManager : MonoBehaviour
{
	private TMP_InputField chatField;

	private CodeStateAnimation anim;

	private CodeStateAnimation chatBubbleAnim;

	private CodeAnimation bobAnim;

	public static bool isTyping;

	private float disableChatIn;

	private TextMeshPro text;

	private AudioSource au;

	private DontBeRude textFilter = new DontBeRude();

	private NetworkPlayer m_NetworkPlayer;

	public AudioClip StartTypeClip;

	public AudioClip endTypeClip;

	public AudioClip startBubbleClip;

	public AudioClip updateBubbleClip;

	private void Awake()
	{
		if (!MatchmakingHandler.IsNetworkMatch)
		{
			base.enabled = false;
		}
	}

	private void Start()
	{
		m_NetworkPlayer = GetComponentInParent<NetworkPlayer>();
		au = base.transform.root.GetComponentInChildren<AudioSource>();
		chatBubbleAnim = GetComponent<CodeStateAnimation>();
		bobAnim = GetComponentInChildren<CodeAnimation>();
		chatField = GameManager.Instance.chatInputField;
		anim = chatField.GetComponent<CodeStateAnimation>();
		text = GetComponentInChildren<TextMeshPro>();
		text.richText = false;
		isTyping = false;
	}

	private void Update()
	{
		if (m_NetworkPlayer.HasLocalControl)
		{
			if (isTyping)
			{
				if (EventSystem.current.currentSelectedGameObject != chatField)
				{
					chatField.Select();
				}
				if (Input.GetKeyDown(KeyCode.Escape))
				{
					StopTyping();
				}
			}
			if (Input.anyKeyDown && isTyping)
			{
				ScreenshakeHandler.Instance.AddShake(Random.insideUnitSphere * 0.01f);
			}
			if (Input.GetKeyDown(KeyCode.Return))
			{
				if (OptionsHolder.chat == 1)
				{
					if (isTyping)
					{
						StopTyping();
					}
					return;
				}
				if (isTyping)
				{
					StopTyping();
				}
				else if (!PauseManager.isPaused)
				{
					StartTyping();
				}
			}
		}
		disableChatIn -= Time.deltaTime;
		if (disableChatIn < 0f && chatBubbleAnim.state1)
		{
			chatBubbleAnim.state1 = false;
		}
	}

	private void StartTyping()
	{
		au.PlayOneShot(StartTypeClip);
		chatField.Select();
		anim.state1 = false;
		isTyping = true;
	}

	private void StopTyping()
	{
		EventSystem.current.SetSelectedGameObject(null);
		anim.state1 = true;
		isTyping = false;
		if (!string.IsNullOrEmpty(chatField.text))
		{
			string message = ReplaceUnacceptableWords(chatField.text);
			SendChatMessage(message);
		}
		else
		{
			au.PlayOneShot(endTypeClip);
		}
		chatField.text = string.Empty;
	}

	private void SendChatMessage(string message)
	{
		if (m_NetworkPlayer.HasLocalControl)
		{
			m_NetworkPlayer.OnTalked(message);
		}
	}

	public void Talk(string t)
	{
		if (disableChatIn > 0f)
		{
			bobAnim.Play();
			au.PlayOneShot(updateBubbleClip);
		}
		else
		{
			au.PlayOneShot(startBubbleClip);
		}
		disableChatIn = 1.5f + (float)t.Length * 0.075f;
		chatBubbleAnim.state1 = true;
		text.text = t;
		ScreenshakeHandler.Instance.AddShake(Vector3.up * 0.3f);
	}

	private string ReplaceUnacceptableWords(string message)
	{
		textFilter.CensorString(ref message);
		return message;
	}
}
