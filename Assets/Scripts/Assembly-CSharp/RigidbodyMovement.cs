using System;
using UnityEngine;

[Serializable]
public class RigidbodyMovement
{
	public Rigidbody rigidbody;

	public float forceMultiplier = 1f;

	public AnimationCurve movementCurve;
}
