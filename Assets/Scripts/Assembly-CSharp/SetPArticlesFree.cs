using UnityEngine;

public class SetPArticlesFree : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
	}

	public void GO()
	{
		ParticleSystem[] componentsInChildren = GetComponentsInChildren<ParticleSystem>();
		foreach (ParticleSystem particleSystem in componentsInChildren)
		{
			if (!particleSystem.GetComponent<RemovePart>())
			{
				particleSystem.transform.parent = null;
				particleSystem.gameObject.AddComponent<RemoveAfterSeconds>();
			}
		}
	}
}
