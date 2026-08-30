using System.Collections.Generic;
using UnityEngine;

public class TreadMill : MonoBehaviour
{
	private List<TreadmillPerson> treadmillPeople = new List<TreadmillPerson>();

	private void Start()
	{
	}

	private void Update()
	{
		TreadmillPerson[] array = treadmillPeople.ToArray();
		foreach (TreadmillPerson treadmillPerson in array)
		{
			int num = 1;
			if (base.transform.InverseTransformPoint(treadmillPerson.rigs[0].position).y < 0f)
			{
				num = -1;
			}
			Rigidbody[] rigs = treadmillPerson.rigs;
			foreach (Rigidbody rigidbody in rigs)
			{
				rigidbody.AddForce(base.transform.forward * Time.deltaTime * 500f * num * Mathf.Clamp(rigidbody.drag, 3f, 10f), ForceMode.Acceleration);
			}
			if (treadmillPerson.time > 0.3f)
			{
				treadmillPeople.Remove(treadmillPerson);
			}
			else
			{
				treadmillPerson.time += Time.deltaTime;
			}
		}
	}

	private void OnCollisionStay(Collision collision)
	{
		if (!collision.rigidbody)
		{
			return;
		}
		int num = 1;
		if (base.transform.InverseTransformPoint(collision.contacts[0].point).y < 0f)
		{
			num = -1;
		}
		CharacterInformation component = collision.transform.root.GetComponent<CharacterInformation>();
		if ((bool)component)
		{
			bool flag = false;
			foreach (TreadmillPerson treadmillPerson2 in treadmillPeople)
			{
				if (component == treadmillPerson2.info)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				TreadmillPerson treadmillPerson = new TreadmillPerson();
				treadmillPerson.info = component;
				treadmillPerson.rigs = component.GetComponentsInChildren<Rigidbody>();
				treadmillPeople.Add(treadmillPerson);
			}
		}
		else
		{
			collision.rigidbody.transform.position += base.transform.forward * Time.deltaTime * num;
		}
		collision.rigidbody.AddForce(base.transform.forward * Time.deltaTime * 500f * num * Mathf.Clamp(collision.rigidbody.drag, 3f, 10f), ForceMode.Acceleration);
	}
}
