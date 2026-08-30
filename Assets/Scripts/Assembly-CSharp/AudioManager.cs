using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
	public AudioMixer masterMixer;

	public AnimationCurve curve;

	private float targetMaster;

	private float targetMusic;

	private float targetSFX;

	private void Start()
	{
	}

	private void Update()
	{
		float value;
		masterMixer.GetFloat("master", out value);
		masterMixer.SetFloat("master", Mathf.Lerp(value, targetMaster, Time.unscaledDeltaTime * 10f));
		float value2;
		masterMixer.GetFloat("music", out value2);
		masterMixer.SetFloat("music", Mathf.Lerp(value2, targetMusic, Time.unscaledDeltaTime * 10f));
		float value3;
		masterMixer.GetFloat("sfx", out value3);
		masterMixer.SetFloat("sfx", Mathf.Lerp(value3, targetSFX, Time.unscaledDeltaTime * 10f));
		masterMixer.SetFloat("blackHole", Mathf.Lerp(value3, targetSFX, Time.unscaledDeltaTime * 10f));
	}

	public void SetMixers()
	{
		targetMaster = curve.Evaluate(OptionsHolder.masterVolume);
		targetMusic = curve.Evaluate(OptionsHolder.musicVolume);
		targetSFX = curve.Evaluate(OptionsHolder.SFXVolume);
	}
}
