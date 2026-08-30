using UnityEngine;

public class MusicHandler : MonoBehaviour
{
	public MusicClip[] myMusic;

	private AudioSource au;

	public AudioSource specialAu;

	public AudioSource specialAu2;

	private float stopCounter;

	private int currntSong;

	private AudioLowPassFilter lowPassFilter;

	private bool hasStarted;

	private int fadeOutType;

	private bool isPlayingSpecial1;

	private bool isPlayingSpecial2;

	private static MusicHandler instance;

	public static MusicHandler Instance
	{
		get
		{
			return instance;
		}
	}

	private void Awake()
	{
		instance = this;
		au = GetComponent<AudioSource>();
		au.clip = myMusic[0].clip;
		lowPassFilter = GetComponent<AudioLowPassFilter>();
		myMusic = RandomizeArray(myMusic);
	}

	private void Start()
	{
	}

	public void StartMusic()
	{
		hasStarted = true;
		au.Play();
	}

	private void Update()
	{
		if (hasStarted)
		{
			SpecialSongUpdate();
			stopCounter += Time.deltaTime;
			if (!au.isPlaying)
			{
				PlayNext();
			}
			if (GameManager.stillInMenu || PauseManager.isPaused)
			{
				lowPassFilter.cutoffFrequency = Mathf.Lerp(lowPassFilter.cutoffFrequency, 600f, Time.unscaledDeltaTime * 15f);
			}
			else
			{
				lowPassFilter.cutoffFrequency = Mathf.Lerp(lowPassFilter.cutoffFrequency, 22000f, Time.unscaledDeltaTime * 15f);
			}
		}
	}

	public bool FadeOutVolume()
	{
		specialAu.volume = Mathf.Clamp(specialAu.volume - Time.unscaledDeltaTime * 10f, 0f, 1f);
		specialAu2.volume = Mathf.Clamp(specialAu2.volume - Time.unscaledDeltaTime * 10f, 0f, 1f);
		au.volume = Mathf.Clamp(au.volume - Time.unscaledDeltaTime * 10f, 0f, 1f);
		Debug.Log("Volume: " + au.volume);
		return specialAu.volume == 0f && specialAu2.volume == 0f && au.volume == 0f;
	}

	private void SpecialSongUpdate()
	{
		if (isPlayingSpecial1)
		{
			specialAu.volume = Mathf.Clamp(specialAu.volume + Time.deltaTime * 1f, 0f, 1f);
		}
		else
		{
			specialAu.volume = Mathf.Clamp(specialAu.volume - Time.deltaTime * 1f, 0f, 1f);
		}
		if (isPlayingSpecial2)
		{
			specialAu2.volume = Mathf.Clamp(specialAu2.volume + Time.deltaTime * 1f, 0f, 1f);
		}
		else
		{
			specialAu2.volume = Mathf.Clamp(specialAu2.volume - Time.deltaTime * 1f, 0f, 1f);
		}
		if (isPlayingSpecial1 || isPlayingSpecial2)
		{
			if (fadeOutType == 0)
			{
				au.pitch = Mathf.Clamp(au.pitch - Time.deltaTime * 0.5f, 0f, 1f);
			}
		}
		else if (au.pitch < 1f)
		{
			au.pitch = Mathf.Clamp(au.pitch + Time.deltaTime * 0.5f, 0f, 1f);
		}
	}

	private void PlayNext()
	{
		currntSong++;
		if (currntSong >= myMusic.Length)
		{
			currntSong = 0;
			myMusic = RandomizeArray(myMusic);
		}
		au.clip = myMusic[currntSong].clip;
		au.volume = myMusic[currntSong].volume;
		au.Play();
	}

	private MusicClip[] RandomizeArray(MusicClip[] clips)
	{
		for (int num = clips.Length - 1; num > 1; num--)
		{
			int num2 = Random.Range(1, num);
			MusicClip musicClip = clips[num];
			clips[num] = clips[num2];
			clips[num2] = musicClip;
		}
		return clips;
	}

	public void PlaySpecialSong(AudioClip clip, int fadeType = 0, int audioSourceID = 0)
	{
		fadeOutType = fadeType;
		if (audioSourceID == 0)
		{
			specialAu.clip = clip;
			specialAu.Play();
			isPlayingSpecial1 = true;
			isPlayingSpecial2 = false;
		}
		if (audioSourceID == 1)
		{
			specialAu2.clip = clip;
			specialAu2.Play();
			isPlayingSpecial2 = true;
			isPlayingSpecial1 = false;
		}
	}

	public void StopPlayingSpecialSong()
	{
		isPlayingSpecial1 = false;
		isPlayingSpecial2 = false;
	}
}
