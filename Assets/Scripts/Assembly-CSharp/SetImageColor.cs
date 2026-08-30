using UnityEngine;
using UnityEngine.UI;

public class SetImageColor : MonoBehaviour
{
	private void Awake()
	{
		GetComponent<Image>().color = base.transform.root.GetComponentInChildren<LineRenderer>().sharedMaterial.color;
	}
}
