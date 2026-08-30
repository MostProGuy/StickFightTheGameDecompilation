using UnityEngine;

public class SetParticleColor : MonoBehaviour
{
	public bool setMaterialInstead;

	private void Start()
	{
		ParticleSystemRenderer component = GetComponent<ParticleSystemRenderer>();
		ParticleSystem component2 = GetComponent<ParticleSystem>();
		if (setMaterialInstead && component != null)
		{
			CharacterInformation component3 = base.transform.root.GetComponent<CharacterInformation>();
			if (component3 != null)
			{
				component.sharedMaterial = component3.myMaterial;
			}
		}
		else if (component2 != null)
		{
			LineRenderer componentInChildren = base.transform.root.GetComponentInChildren<LineRenderer>();
			if (componentInChildren != null)
			{
				component2.startColor = componentInChildren.sharedMaterial.color;
			}
		}
	}
}
