using Steamworks;
using UnityEngine;

public class HostLeverHandler : MonoBehaviour
{
	public Transform lever;

	public bool isOn;

	private void Start()
	{
		lever.gameObject.SetActive(false);
	}

	private void Update()
	{
		bool flag = false;
		if (lever == null)
		{
			base.enabled = false;
			return;
		}
		if (lever.transform.forward.z < 0f)
		{
			flag = true;
		}
		if (isOn != flag)
		{
			if (flag)
			{
				EnableLever();
			}
			else
			{
				DisableLever();
			}
		}
	}

	public void SetLeverActive()
	{
		lever.gameObject.SetActive(true);
	}

	public void EnableLever()
	{
		isOn = true;
		if (MultiplayerManager.IsServer)
		{
			MatchmakingHandler.Instance.ChangeLobbyType(ELobbyType.k_ELobbyTypePublic);
		}
	}

	public void DisableLever()
	{
		isOn = false;
		if (MultiplayerManager.IsServer)
		{
			MatchmakingHandler.Instance.ChangeLobbyType(ELobbyType.k_ELobbyTypeFriendsOnly);
		}
	}
}
