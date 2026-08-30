using UnityEngine;

public class EnableObjectsPerPlayer : MonoBehaviour
{
	public GameObject[] objects;

	private ControllerHandler manager;

	private void Awake()
	{
		if (MatchmakingHandler.IsNetworkMatch)
		{
			base.gameObject.AddComponent<MapInfoOnlineTag>();
		}
	}

	private void Start()
	{
		for (int i = 0; i < objects.Length; i++)
		{
			objects[i].SetActive(false);
		}
		manager = ControllerHandler.Instance;
		byte[] array = new byte[4];
		byte b = (byte)base.gameObject.name.Length;
		array[0] = b;
		for (int j = 0; j < 3; j++)
		{
			byte b2 = (byte)Random.Range(0, objects.Length);
			if (MatchmakingHandler.IsNetworkMatch)
			{
				if (MultiplayerManager.IsServer)
				{
					array[j + 1] = b2;
				}
			}
			else
			{
				objects[b2].SetActive(true);
			}
		}
		if (MatchmakingHandler.IsNetworkMatch && MultiplayerManager.IsServer)
		{
			Object.FindObjectOfType<MultiplayerManager>().SendMapInfo(array);
		}
	}

	public void RecieveMapInfo(byte[] data)
	{
		byte b = data[0];
		if (b != base.gameObject.name.Length)
		{
			Debug.LogError("The Name Length Does Not Match On This Mapinfo object, got: " + b + " Excpecting: " + base.gameObject.name.Length);
		}
		else
		{
			for (int i = 0; i < 3; i++)
			{
				objects[data[i + 1]].SetActive(true);
			}
		}
	}
}
