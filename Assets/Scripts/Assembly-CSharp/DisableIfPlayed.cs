using UnityEngine;

public class DisableIfPlayed : MonoBehaviour
{
	public static bool hasPlayed;

	private void Start()
	{
		if (hasPlayed)
		{
			base.gameObject.SetActive(false);
			Object.FindObjectOfType<MusicHandler>().StartMusic();
		}
	}

	private void Update()
	{
	}
}
