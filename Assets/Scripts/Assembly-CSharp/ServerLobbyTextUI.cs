using Steamworks;
using TMPro;
using UnityEngine;

public class ServerLobbyTextUI : MonoBehaviour
{
	private const string k_HOSTED_PRIVATE_LOBBY_TEXT = "INVITE YOUR FRIENDS THROUGH STEAM";

	private const string k_HOSTED_PUBLIC_LOBBY_TEXT = "WAITING FOR PLAYERS";

	private TextMeshPro m_Text;

	private void Awake()
	{
		m_Text = GetComponent<TextMeshPro>();
	}

	private void OnEnable()
	{
		ELobbyType lobbyType = MatchmakingHandler.LobbyType;
		if (lobbyType == ELobbyType.k_ELobbyTypePublic)
		{
			m_Text.text = "WAITING FOR PLAYERS";
		}
		else
		{
			m_Text.text = "INVITE YOUR FRIENDS THROUGH STEAM";
		}
	}
}
