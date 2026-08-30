using System;
using UnityEngine;

[Serializable]
public class AnimationInfo
{
	public enum AnimationType
	{
		Torque = 0,
		Force = 1
	}

	public enum Direction
	{
		CharacterRight = 0,
		CharacterForward = 1,
		CharacterUp = 2,
		SelfRight = 3,
		SelfForward = 4,
		SelfUp = 5,
		WorldRight = 6,
		WorldForward = 7,
		WorldUp = 8,
		MainBodyVelocity = 9
	}

	public AnimationType type;

	public Direction direction;

	public float forwardForceMultiplier = 1f;

	public float backForceMultiplier = -1f;

	public float smoothing = 1f;

	public bool isLeftSide;

	private float activeForceAmount;

	[Header("Pace Multipliers")]
	public float walking = 1f;

	public float running = 1f;

	public float back = 1f;

	public float side = 1f;
}
