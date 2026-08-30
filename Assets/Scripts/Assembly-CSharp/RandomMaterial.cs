using System.Collections.Generic;
using UnityEngine;

public class RandomMaterial : MonoBehaviour
{
	[SerializeField]
	private List<Material> PossibleMaterials = new List<Material>();

	private void Start()
	{
		GetComponent<Renderer>().material = PossibleMaterials[Random.Range(0, PossibleMaterials.Count)];
	}

	private void Update()
	{
	}
}
