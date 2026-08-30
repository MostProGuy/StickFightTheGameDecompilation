using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class God : MonoBehaviour
{
	public UnityEvent startLevelEvent;

	public string[] playerNames;

	public GodText[] godText1;

	public GameObject item1;

	public TextMeshPro text;

	public ParticleSystem burst;

	public ParticleSystem ringBurst;

	public ParticleSystem ringBurstSmall;

	public ParticleSystem weaponSpawnPart;

	public CodeAnimation anim;

	private ScreenshakeHandler screenshake;

	private Color textStartColor;

	private AudioSource au;

	public AudioSource battleAu;

	public AudioSource calmAu;

	public AudioClip[] voices;

	private bool hasSpanwedWeapon;

	private void Start()
	{
		StartCoroutine(PlayDialogues(godText1, item1));
		screenshake = ScreenshakeHandler.Instance;
		textStartColor = text.color;
		au = GetComponent<AudioSource>();
	}

	private void Update()
	{
		text.color = new Color(text.color.r, text.color.g, text.color.b, text.color.a - Time.deltaTime * 0.5f);
		if (hasSpanwedWeapon)
		{
			calmAu.volume -= Time.deltaTime * 0.3f;
			battleAu.volume += Time.deltaTime * 0.3f;
		}
		else
		{
			calmAu.volume += Time.deltaTime * 0.3f;
			battleAu.volume -= Time.deltaTime * 0.3f;
		}
		battleAu.volume = Mathf.Clamp(battleAu.volume, 0f, 1f);
		calmAu.volume = Mathf.Clamp(calmAu.volume, 0f, 1f);
	}

	private IEnumerator PlayDialogues(GodText[] godText, GameObject item)
	{
		yield return new WaitForSeconds(2f);
		for (int i = 0; i < playerNames.Length; i++)
		{
			yield return new WaitForSeconds(1f);
			text.text = playerNames[i];
			Burst(1f);
		}
		for (int j = 0; j < godText.Length; j++)
		{
			yield return new WaitForSeconds(godText[j].delay);
			text.text = godText[j].texts;
			Burst(godText[j].importance);
		}
		yield return new WaitForSeconds(3f);
		weaponSpawnPart.Play();
		yield return new WaitForSeconds(0.15f);
		Spawn(item);
	}

	private void Spawn(GameObject item)
	{
		GameObject gameObject = Object.Instantiate(item, base.transform.position, base.transform.rotation);
		gameObject.GetComponent<WeaponPickUp>().flyUpAfter = float.PositiveInfinity;
		gameObject.GetComponent<Rigidbody>().useGravity = false;
		gameObject.GetComponent<ConstantForce>().force += Vector3.down * 200f;
		hasSpanwedWeapon = true;
		ParticleSystem[] componentsInChildren = GetComponentsInChildren<ParticleSystem>();
		foreach (ParticleSystem particleSystem in componentsInChildren)
		{
			particleSystem.Stop();
		}
		startLevelEvent.Invoke();
	}

	private void Burst(float str)
	{
		au.PlayOneShot(voices[Random.Range(0, voices.Length)]);
		text.color = textStartColor;
		screenshake.AddShake(Random.insideUnitSphere * str * 0.6f);
		burst.startSize = 0.05f + str * 0.1f;
		burst.Emit((int)(str * 200f));
		ringBurst.Emit((int)(str * 2f));
		ringBurstSmall.Emit((int)(str * 2f));
		anim.Play();
	}
}
