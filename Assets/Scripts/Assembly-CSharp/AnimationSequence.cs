using System.Collections;
using UnityEngine;

public class AnimationSequence : MonoBehaviour
{
	public CodeStateAnimation[] stateAnimations;

	public float[] delayBeforePlayingEvent;

	public float[] delayBeforePlayingEventWhenReturning;

	public bool isPlaying;

	public bool selectedState = true;

	public bool currentState = true;

	private void Update()
	{
		if (selectedState != currentState && !isPlaying)
		{
			if (!selectedState)
			{
				StartCoroutine(PlayOpenAnimations());
			}
			else
			{
				StartCoroutine(PlayCloseAnimations());
			}
		}
	}

	public void Play()
	{
		selectedState = !selectedState;
	}

	public void Play(bool stateToSet)
	{
		selectedState = stateToSet;
	}

	private IEnumerator PlayAnimations()
	{
		isPlaying = true;
		currentState = false;
		for (int i = 0; i < stateAnimations.Length; i++)
		{
			yield return new WaitForSecondsRealtime(delayBeforePlayingEvent[i]);
			stateAnimations[i].state1 = true;
		}
		yield return new WaitForSecondsRealtime(0.3f);
		isPlaying = false;
	}

	private IEnumerator PlayOpenAnimations()
	{
		isPlaying = true;
		currentState = false;
		for (int i = 0; i < stateAnimations.Length; i++)
		{
			yield return new WaitForSecondsRealtime(delayBeforePlayingEvent[i]);
			stateAnimations[i].state1 = false;
		}
		yield return new WaitForSecondsRealtime(0.3f);
		isPlaying = false;
	}

	private IEnumerator PlayCloseAnimations()
	{
		isPlaying = true;
		currentState = true;
		for (int i = stateAnimations.Length - 1; i >= 0; i--)
		{
			yield return new WaitForSecondsRealtime(delayBeforePlayingEventWhenReturning[i]);
			stateAnimations[i].state1 = !stateAnimations[i].state1;
		}
		yield return new WaitForSecondsRealtime(0.3f);
		isPlaying = false;
	}
}
