using UnityEngine;

public class SetLinePositions : MonoBehaviour
{
	public Transform[] transforms;

	private LineRenderer line;

	private HealthHandler health;

	public Material dmgMat;

	private Material startMat;

	private void Start()
	{
		line = GetComponent<LineRenderer>();
		health = base.transform.root.GetComponent<HealthHandler>();
		line.positionCount = transforms.Length;
		startMat = line.sharedMaterial;
		Update();
	}

	private void Update()
	{
		for (int i = 0; i < transforms.Length; i++)
		{
			line.SetPosition(i, new Vector3(0f, transforms[i].position.y, transforms[i].position.z));
		}
		line.enabled = true;
		if ((bool)health)
		{
			if (health.sinceDamage < 0.1f)
			{
				line.sharedMaterial = dmgMat;
			}
			else
			{
				line.sharedMaterial = startMat;
			}
		}
	}
}
