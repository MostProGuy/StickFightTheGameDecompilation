using UnityEngine;

public class ReachForPlayer : MonoBehaviour
{
	public float force;

	public float selfForce;

	public float damage;

	public float range;

	private Rigidbody rig;

	private AI ai;

	private float dmgCounter;

	private CharacterInformation info;

	private void Start()
	{
		ai = GetComponentInParent<AI>();
		info = GetComponentInParent<CharacterInformation>();
		rig = GetComponent<Rigidbody>();
	}

	private void FixedUpdate()
	{
		if (info.isDead || !ai.target)
		{
			return;
		}
		rig.AddForce(-(base.transform.position - ai.target.position).normalized * Time.fixedDeltaTime * selfForce * 0.5f, ForceMode.Acceleration);
		if (Vector3.Distance(base.transform.position, ai.target.position) < range)
		{
			dmgCounter += Time.fixedDeltaTime;
			ai.target.AddForce((base.transform.position - ai.target.position).normalized * Time.fixedDeltaTime * force * 0.5f, ForceMode.Acceleration);
			ai.target.AddForce(Vector3.down * Time.fixedDeltaTime * force * 0.5f, ForceMode.Acceleration);
			if (dmgCounter > 0.3f && damage > 0f)
			{
				ai.target.transform.root.GetComponent<HealthHandler>().TakeDamage(damage, null);
				dmgCounter = 0f;
			}
		}
		else
		{
			dmgCounter = 0f;
		}
	}
}
