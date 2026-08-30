using UnityEngine;

public class SetColorWhenDamaged : MonoBehaviour
{
	private SpriteRenderer sprite;

	private MeshRenderer meshRend;

	public Color startColor;

	public Material startMat;

	public Material red;

	private HealthHandler health;

	private void Start()
	{
		sprite = GetComponent<SpriteRenderer>();
		meshRend = GetComponent<MeshRenderer>();
		if ((bool)sprite)
		{
			startColor = sprite.color;
		}
		if ((bool)meshRend)
		{
			startMat = base.transform.root.GetComponent<CharacterInformation>().myMaterial;
		}
		health = base.transform.GetComponentInParent<HealthHandler>();
	}

	private void Update()
	{
		if (health.sinceDamage < 0.1f)
		{
			if ((bool)sprite)
			{
				sprite.color = Color.red;
			}
			if ((bool)meshRend)
			{
				meshRend.sharedMaterial = red;
			}
		}
		else
		{
			if ((bool)sprite)
			{
				sprite.color = startColor;
			}
			if ((bool)meshRend)
			{
				meshRend.sharedMaterial = startMat;
			}
		}
	}
}
