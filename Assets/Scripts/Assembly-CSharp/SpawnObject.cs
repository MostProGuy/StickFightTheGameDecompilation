using UnityEngine;

public class SpawnObject : MonoBehaviour
{
	public GameObject obj;

	public bool canOnlyBeCalledOnce;

	private bool done;

	public bool identityRotation;

	public bool mirrorLayers;

	private Controller controller;

	private void Start()
	{
		DamagerHolder component = GetComponent<DamagerHolder>();
		if ((bool)component)
		{
			controller = component.damager;
		}
		else
		{
			Debug.LogWarning("SpawnObject missing DamagerHolder");
		}
	}

	private void Update()
	{
	}

	public void GO()
	{
		if (done)
		{
			return;
		}
		Quaternion rotation = base.transform.rotation;
		if (identityRotation)
		{
			rotation = Quaternion.identity;
		}
		GameObject gameObject = Object.Instantiate(obj, base.transform.position, rotation);
		gameObject.AddComponent<RemoveOnLevelChange>();
		if (mirrorLayers)
		{
			RayCastForward[] componentsInChildren = gameObject.GetComponentsInChildren<RayCastForward>();
			foreach (RayCastForward rayCastForward in componentsInChildren)
			{
				if (!rayCastForward.dontChangeMask)
				{
					rayCastForward.mask = 1 << controller.playerID + 8;
					rayCastForward.mask = ~(int)rayCastForward.mask;
				}
			}
		}
		if (canOnlyBeCalledOnce)
		{
			done = true;
		}
	}
}
