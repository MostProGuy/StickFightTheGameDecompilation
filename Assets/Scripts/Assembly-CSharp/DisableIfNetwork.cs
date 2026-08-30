using UnityEngine;

public class DisableIfNetwork : MonoBehaviour
{
	[SerializeField]
	private bool mDisableIfClientOnly;

	private void OnEnable()
	{
		if (MatchmakingHandler.IsNetworkMatch)
		{
			if (mDisableIfClientOnly)
			{
				if (!MultiplayerManager.IsServer)
				{
					base.gameObject.SetActive(false);
				}
			}
			else
			{
				base.gameObject.SetActive(false);
			}
		}
		else
		{
			base.gameObject.SetActive(true);
		}
	}

	public void Check()
	{
		OnEnable();
	}
}
