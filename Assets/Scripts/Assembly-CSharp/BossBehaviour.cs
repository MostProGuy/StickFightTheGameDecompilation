using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class BossBehaviour : MonoBehaviour
{
	[Serializable]
	public class BehaviourEvents
	{
		public enum EventDelayType
		{
			Start = 0,
			End = 1,
			Count = 2
		}

		public EventDelayType eventDelayType;

		public UnityEvent behaviourEvent;

		public float eventDelay;
	}

	[Serializable]
	public class Behaviour
	{
		public float timeNeededToComplete;

		public Transform targetPosition;

		public float distanceToPointAllowed = 2f;

		public int loops;

		[HideInInspector]
		public int loopPoints;

		public BehaviourEvents[] events;
	}

	public Behaviour[] behaviours;

	private int currentBehaviour;

	private float timeNeededToCompleteCounter;

	private AI ai;

	private Transform torso;

	private void Start()
	{
		UnityEngine.Object.FindObjectOfType<BossHealth>().AttachBoss(GetComponent<HealthHandler>());
		ai = GetComponent<AI>();
		torso = GetComponentInChildren<Torso>().transform;
		SetBehaviour();
	}

	private void Update()
	{
		if (timeNeededToCompleteCounter >= behaviours[currentBehaviour].timeNeededToComplete)
		{
			currentBehaviour++;
			if (currentBehaviour >= behaviours.Length)
			{
				currentBehaviour = 0;
			}
			if (behaviours[currentBehaviour].loops <= behaviours[currentBehaviour].loopPoints)
			{
				behaviours[currentBehaviour].loopPoints = 0;
			}
			else
			{
				behaviours[currentBehaviour].loopPoints++;
				currentBehaviour = 0;
			}
			SetBehaviour();
		}
		if (!behaviours[currentBehaviour].targetPosition)
		{
			timeNeededToCompleteCounter += Time.deltaTime;
		}
		else if ((bool)behaviours[currentBehaviour].targetPosition && Vector3.Distance(behaviours[currentBehaviour].targetPosition.position, torso.position) < behaviours[currentBehaviour].distanceToPointAllowed)
		{
			timeNeededToCompleteCounter += Time.deltaTime;
		}
	}

	private void SetBehaviour()
	{
		timeNeededToCompleteCounter = 0f;
		BehaviourEvents[] events = behaviours[currentBehaviour].events;
		foreach (BehaviourEvents behaviourEvents in events)
		{
			if (behaviourEvents.eventDelayType == BehaviourEvents.EventDelayType.Count)
			{
				StartCoroutine(InvokeEventAfterDelay(behaviourEvents.eventDelay, behaviourEvents.behaviourEvent));
			}
			else if (behaviourEvents.eventDelayType == BehaviourEvents.EventDelayType.Start)
			{
				behaviourEvents.behaviourEvent.Invoke();
			}
			else
			{
				StartCoroutine(InvokeEventAfterDelay(behaviours[currentBehaviour].timeNeededToComplete, behaviourEvents.behaviourEvent));
			}
		}
		if ((bool)behaviours[currentBehaviour].targetPosition)
		{
			ai.behaviourTarget = behaviours[currentBehaviour].targetPosition;
		}
		else
		{
			ai.behaviourTarget = null;
		}
	}

	private IEnumerator InvokeEventAfterDelay(float delay, UnityEvent eventToInvoke)
	{
		yield return new WaitForSeconds(delay);
		eventToInvoke.Invoke();
	}
}
