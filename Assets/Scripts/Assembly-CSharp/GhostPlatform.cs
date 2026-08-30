using System.Collections;
using UnityEngine;

public class GhostPlatform : MapInfoSyncableBase
{
	public Material offMat;

	public Material spriteDefault;

	private Material onMat;

	public float onTime;

	public float offTime;

	public float startAfter;

	public bool startOn = true;

	private float counter;

	private bool isOn = true;

	private Collider[] colliders;

	private Renderer[] rends;

	private CodeStateAnimation anim;

	private Vector3 startPosition;

	protected override void Awake()
	{
		base.Awake();
	}

	private void Start()
	{
		anim = GetComponent<CodeStateAnimation>();
		colliders = GetComponentsInChildren<Collider>();
		rends = GetComponentsInChildren<Renderer>();
		onMat = rends[0].sharedMaterial;
		startPosition = base.transform.localPosition;
		Renderer[] array = rends;
		foreach (Renderer renderer in array)
		{
			if ((bool)renderer.GetComponent<SpriteRenderer>())
			{
				spriteDefault = renderer.sharedMaterial;
			}
		}
		anim.dontPlayFor = 1f;
		anim.state1 = startOn;
		if (!startOn)
		{
			TurnOff();
			Collider[] array2 = colliders;
			foreach (Collider collider in array2)
			{
				collider.enabled = false;
			}
			isOn = false;
		}
		else
		{
			TurnOn();
		}
	}

	protected override void Update()
	{
		base.Update();
		if (startAfter > 0f)
		{
			startAfter -= Time.deltaTime;
			return;
		}
		counter += Time.deltaTime;
		if (!MatchmakingHandler.IsNetworkMatch || MultiplayerManager.IsServer)
		{
			if (isOn && counter > onTime)
			{
				isOn = false;
				counter = -3f;
				StartCoroutine(FadeOut());
			}
			if (!isOn && counter > offTime)
			{
				isOn = true;
				counter = -3f;
				StartCoroutine(FadeIn());
			}
		}
	}

	private IEnumerator FadeIn()
	{
		GhostPlatformSound.Instance.PlaySound();
		anim.state1 = true;
		yield return new WaitForSeconds(2f);
		TurnOn();
		Collider[] array = colliders;
		foreach (Collider collider in array)
		{
			collider.enabled = true;
		}
	}

	private IEnumerator FadeOut()
	{
		yield return new WaitForSeconds(1f);
		GhostPlatformSound.Instance.PlaySound();
		anim.state1 = false;
		yield return new WaitForSeconds(2f);
		TurnOff();
		yield return new WaitForSeconds(0.1f);
		Collider[] array = colliders;
		foreach (Collider collider in array)
		{
			collider.enabled = false;
		}
	}

	private void TurnOff()
	{
		Renderer[] array = rends;
		foreach (Renderer renderer in array)
		{
			ParticleSystem component = renderer.GetComponent<ParticleSystem>();
			if ((bool)component)
			{
				component.enableEmission = false;
			}
			else
			{
				renderer.sharedMaterial = offMat;
			}
		}
		base.transform.localPosition = startPosition + Vector3.right * 2f;
	}

	private void TurnOn()
	{
		Renderer[] array = rends;
		foreach (Renderer renderer in array)
		{
			ParticleSystem component = renderer.GetComponent<ParticleSystem>();
			if ((bool)component)
			{
				component.enableEmission = true;
			}
			else if ((bool)renderer.GetComponent<SpriteRenderer>())
			{
				renderer.sharedMaterial = spriteDefault;
			}
			else
			{
				renderer.sharedMaterial = onMat;
			}
		}
		base.transform.localPosition = startPosition;
	}

	public override byte[] GetData()
	{
		return new byte[1] { (byte)(isOn ? 1u : 0u) };
	}

	public override void SetData(byte[] data)
	{
		bool flag = data[0] == 1;
		if (isOn)
		{
			if (!flag)
			{
				isOn = false;
				counter = -2f;
				StartCoroutine(FadeOut());
			}
		}
		else if (flag)
		{
			isOn = true;
			counter = -2f;
			StartCoroutine(FadeIn());
		}
	}
}
