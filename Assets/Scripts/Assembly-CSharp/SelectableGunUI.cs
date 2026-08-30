using UnityEngine;

public class SelectableGunUI : MonoBehaviour
{
	[SerializeField]
	private GameObject m_UI;

	public int CurrentIndex { get; private set; }

	private void Start()
	{
	}
}
