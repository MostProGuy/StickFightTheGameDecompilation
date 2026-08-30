using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

public class CodeAnimation : MonoBehaviour
{
	public enum AnimationType
	{
		Scale = 0,
		Position = 1
	}

	public AnimationType animationType;

	[Header("Animation")]
	public bool looping;

	public bool playOnAwake = true;

	public bool disableObjectAfterAnimation;

	public bool ignoreTimeScale;

	public bool useX = true;

	public bool useY = true;

	public bool useZ = true;

	public AnimationCurve curve;

	public float duration = 1f;

	public float aditionalRandomDuration;

	public float multiplier = 1f;

	private float baseX;

	private float baseY;

	private float baseZ;

	public float firstDelay;

	public int loops;

	private int currentLoops;

	public UnityEvent EndEvent;

	private bool m_IsPlaying;

	[SerializeField]
	private bool m_ShallSync;

	private bool hasStarted;

	public bool IsPlaying
	{
		get
		{
			return m_IsPlaying;
		}
	}

	private void Awake()
	{
		if (MatchmakingHandler.IsNetworkMatch && m_ShallSync)
		{
			base.gameObject.AddComponent<MapInfoOnlineTag>();
		}
		RandomValue component = GetComponent<RandomValue>();
		if ((bool)component)
		{
			duration *= component.value;
		}
		duration += Random.Range(0f, aditionalRandomDuration);
		if (m_ShallSync && MatchmakingHandler.IsNetworkMatch && MultiplayerManager.IsServer)
		{
			byte b = (byte)base.gameObject.name.Length;
			byte[] array = new byte[4 + b];
			using (MemoryStream output = new MemoryStream(array))
			{
				using (BinaryWriter binaryWriter = new BinaryWriter(output))
				{
					binaryWriter.Write(b);
					binaryWriter.Write(duration);
				}
			}
			Object.FindObjectOfType<MultiplayerManager>().SendMapInfo(array);
		}
		if (animationType == AnimationType.Position)
		{
			baseX = base.transform.localPosition.x;
			baseY = base.transform.localPosition.y;
			baseZ = base.transform.localPosition.z;
		}
		if (animationType == AnimationType.Scale)
		{
			baseX = base.transform.localScale.x;
			baseY = base.transform.localScale.y;
			baseZ = base.transform.localScale.z;
		}
		hasStarted = true;
	}

	private void OnEnable()
	{
		if (playOnAwake)
		{
			StartCoroutine(Animation());
		}
	}

	public void Play()
	{
		if (!hasStarted)
		{
			Awake();
		}
		StartCoroutine(Animation());
		currentLoops = loops;
	}

	private IEnumerator Animation()
	{
		m_IsPlaying = true;
		while (firstDelay > 0f)
		{
			firstDelay -= Time.deltaTime;
			yield return null;
		}
		float t = 0f;
		while (t < duration)
		{
			t = ((!ignoreTimeScale || !(TimeHandler.pauseTime > 0f)) ? (t + Time.deltaTime) : (t + Time.unscaledDeltaTime));
			float curveValue = curve.Evaluate(t / duration) * multiplier;
			if (animationType == AnimationType.Position)
			{
				Vector3 localPosition = base.transform.localPosition;
				if (useX)
				{
					localPosition.x = curveValue + baseX;
				}
				if (useY)
				{
					localPosition.y = curveValue + baseY;
				}
				if (useZ)
				{
					localPosition.z = curveValue + baseZ;
				}
				base.transform.localPosition = localPosition;
			}
			if (animationType == AnimationType.Scale)
			{
				if (curveValue == 0f)
				{
					curveValue = 0.001f;
				}
				Vector3 localScale = base.transform.localScale;
				if (useX)
				{
					localScale.x = curveValue * baseX;
				}
				if (useY)
				{
					localScale.y = curveValue * baseY;
				}
				if (useZ)
				{
					localScale.z = curveValue * baseZ;
				}
				base.transform.localScale = localScale;
			}
			yield return new WaitForEndOfFrame();
		}
		if (currentLoops > 0)
		{
			currentLoops--;
			StartCoroutine(Animation());
		}
		if (looping)
		{
			Play();
		}
		if (disableObjectAfterAnimation)
		{
			base.gameObject.SetActive(false);
		}
		EndEvent.Invoke();
		m_IsPlaying = false;
	}

	private void OnDisable()
	{
		m_IsPlaying = false;
		if (animationType == AnimationType.Scale)
		{
			base.transform.localScale = new Vector3(baseX, baseY, baseZ);
		}
		if (animationType == AnimationType.Position)
		{
			base.transform.localPosition = new Vector3(baseX, baseY, baseZ);
		}
	}

	public void RecieveMapInfo(byte[] data)
	{
		byte b = data[0];
		if (base.gameObject.name.Length != b)
		{
			return;
		}
		float num = 0f;
		using (MemoryStream input = new MemoryStream(data))
		{
			using (BinaryReader binaryReader = new BinaryReader(input))
			{
				binaryReader.Read();
				num = binaryReader.ReadSingle();
			}
		}
		duration = num;
	}
}
