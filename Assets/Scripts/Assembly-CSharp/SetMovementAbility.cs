using UnityEngine;

public class SetMovementAbility : MonoBehaviour
{
	public MovementAbility[] abilities;

	private Controller controller;

	public BossHealth bossHealth;

	private HealthHandler health;

	private CodeStateAnimation anim;

	private void Start()
	{
		controller = GetComponent<Controller>();
		health = GetComponent<HealthHandler>();
		anim = bossHealth.GetComponentInChildren<CodeStateAnimation>();
	}

	public void SetAbility(int i)
	{
		GameObject[] objects = abilities[i].objects;
		foreach (GameObject gameObject in objects)
		{
			gameObject.SetActive(true);
			ParticleSystemRenderer[] componentsInChildren = gameObject.GetComponentsInChildren<ParticleSystemRenderer>(true);
			foreach (ParticleSystemRenderer particleSystemRenderer in componentsInChildren)
			{
				particleSystemRenderer.sharedMaterial = abilities[i].wingMat;
			}
		}
		controller.canFly = abilities[i].canFly;
		controller.flySpeed = abilities[i].flySpeed;
		anim.state1 = abilities[i].showHPBar;
		float num = (float)(ControllerHandler.Instance.players.Count - 1) * 1f;
		health.bossHealthZ = abilities[i].maxHP + abilities[i].maxHP * num;
		Debug.Log("reviving all players!");
		GameManager.Instance.ReviveAllPlayers(false);
	}

	public void Reset()
	{
		MovementAbility[] array = abilities;
		foreach (MovementAbility movementAbility in array)
		{
			GameObject[] objects = movementAbility.objects;
			foreach (GameObject gameObject in objects)
			{
				gameObject.SetActive(false);
			}
		}
		health.bossHealthZ = 100f;
		controller.canFly = false;
		anim.state1 = false;
	}
}
