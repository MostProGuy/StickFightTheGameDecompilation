using UnityEngine;
using UnityEngine.Events;

public class PlayParticlesFromAnimation : MonoBehaviour
{
	public UnityEvent event1;

	public UnityEvent event2;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void Play1()
	{
		event1.Invoke();
	}

	public void Play2()
	{
		event2.Invoke();
	}
}
