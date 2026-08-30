using UnityEngine;
using UnityEngine.Events;

public class ProjectileBounce : MonoBehaviour
{
	public int bounces = 5;

	public bool loseTeamOnBounce = true;

	private RayCastForward rayCaster;

	private ProjectileCollision collision;

	public UnityEvent bounceEvent;

	private void Start()
	{
		rayCaster = GetComponent<RayCastForward>();
		collision = GetComponent<ProjectileCollision>();
	}

	private void Update()
	{
	}

	public void Bounce(RaycastHit hit)
	{
		bounceEvent.Invoke();
		bounces--;
		base.transform.rotation = Quaternion.LookRotation(Vector3.Reflect(base.transform.forward, hit.normal));
		base.transform.rotation = Quaternion.LookRotation(new Vector3(0f, base.transform.forward.y, base.transform.forward.z));
		base.transform.position = hit.point;
		if (loseTeamOnBounce)
		{
			rayCaster.mask = -1;
		}
		collision.PlayParticles(hit.normal);
		if (bounces < 0)
		{
			Object.Destroy(this);
		}
	}
}
