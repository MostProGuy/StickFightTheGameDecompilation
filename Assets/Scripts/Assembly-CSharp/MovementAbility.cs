using System;
using UnityEngine;

[Serializable]
public class MovementAbility
{
	public GameObject[] objects;

	public bool canFly;

	public float flySpeed = 1f;

	public float maxHP = 100f;

	public bool showHPBar = true;

	public Material wingMat;
}
