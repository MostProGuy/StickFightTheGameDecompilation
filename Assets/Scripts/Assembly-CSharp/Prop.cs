using UnityEngine;

public class Prop : MonoBehaviour
{
	public ParticleSystem part;

	private void Awake()
	{
	}

	public void Destroy()
	{
		part.transform.parent = null;
		part.Play();
		part.gameObject.AddComponent<RemoveAfterSeconds>().time = 3f;
		Object.Destroy(base.gameObject);
	}
}
