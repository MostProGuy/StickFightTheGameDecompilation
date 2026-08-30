using UnityEngine;

public class ThrownWeapon : MonoBehaviour
{
	private Rigidbody rig;

	private Vector3 lastPos;

	public LayerMask mask;

	private ScreenshakeHandler screenShake;

	private bool done;

	private bool hasCollision;

	public bool pierce;

	private Transform stickObject;

	public Controller damager;

	public GameObject effect;

	public int weaponID;

	private Blink blink;

	private void Start()
	{
		rig = GetComponent<Rigidbody>();
		lastPos = base.transform.position;
		screenShake = ScreenshakeHandler.Instance;
		blink = GetComponent<Blink>();
	}

	private void OnDestroy()
	{
		if ((bool)stickObject)
		{
			Object.Destroy(stickObject.gameObject);
		}
	}

	private void LateUpdate()
	{
		base.transform.position = new Vector3(0f, base.transform.position.y, base.transform.position.z);
		if (done)
		{
			if ((bool)stickObject)
			{
				if (hasCollision)
				{
					base.transform.position = stickObject.position;
				}
				else
				{
					base.transform.position = stickObject.position + Vector3.right;
				}
				base.transform.rotation = stickObject.rotation;
				if (!stickObject.gameObject.activeInHierarchy)
				{
					Object.Destroy(base.gameObject);
				}
			}
		}
		else
		{
			Vector3 direction = base.transform.position - lastPos;
			Ray ray = new Ray(base.transform.position, direction);
			RaycastHit hitInfo;
			Physics.Raycast(ray, out hitInfo, direction.magnitude, mask);
			if ((bool)hitInfo.transform)
			{
				Stick(hitInfo);
				Debug.Log(hitInfo.transform.name);
			}
			lastPos = base.transform.position;
		}
	}

	private void Stick(RaycastHit hit)
	{
		if ((bool)blink)
		{
			blink.Go(damager);
			Object.Destroy(base.gameObject);
		}
		if (pierce)
		{
			stickObject = new GameObject().transform;
			stickObject.transform.rotation = Quaternion.LookRotation(rig.velocity.normalized + Random.insideUnitSphere * 0.3f);
			stickObject.position = hit.point - stickObject.forward * 0.2f;
			stickObject.SetParent(hit.transform, true);
		}
		if ((bool)hit.rigidbody)
		{
			DestructiblePiece component = hit.rigidbody.GetComponent<DestructiblePiece>();
			if ((bool)component)
			{
				component.damager = damager;
				component.Collide(base.transform.forward * 55f, 1f);
			}
			BodyPart component2 = hit.rigidbody.GetComponent<BodyPart>();
			if ((bool)component2)
			{
				BlockHandler component3 = component2.transform.root.GetComponent<BlockHandler>();
				if (Vector3.Angle(-component3.blockAngle, rig.velocity) < component3.blockForce * 40f && component3.isBlocking)
				{
					component3.GetComponent<Fighting>().PickUpWeapon(weaponID, null);
					Object.Destroy(base.gameObject);
					return;
				}
				base.gameObject.AddComponent<RemoveOnLevelChange>();
				component2.TakeDamageWithParticle(55f, hit.point, rig.velocity, damager);
				if ((bool)damager)
				{
					damager.GetComponent<CharacterStats>().weaponsThrown++;
				}
			}
		}
		else
		{
			Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
			foreach (Collider collider in componentsInChildren)
			{
				collider.gameObject.layer = 22;
				collider.enabled = true;
			}
			hasCollision = true;
		}
		if ((bool)hit.rigidbody)
		{
			hit.rigidbody.AddForce(rig.velocity * 100f, ForceMode.Impulse);
		}
		screenShake.AddShake(rig.velocity.normalized * 0.3f);
		rig.isKinematic = true;
		done = true;
		if (!pierce)
		{
			GameObject gameObject = (GameObject)Object.Instantiate(Resources.Load("GunPart"), hit.point, Quaternion.Euler(0f, 90f, 0f));
			Object.Destroy(base.gameObject);
		}
	}
}
