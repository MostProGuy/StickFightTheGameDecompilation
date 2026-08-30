using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class ConstrainSelectionToChildren : MonoBehaviour
{
	public bool isActive;

	private GameObject lastAcceptedGameObject;

	private void Awake()
	{
	}

	private void Update()
	{
		if (isActive && (bool)EventSystem.current.currentSelectedGameObject)
		{
			if (!EventSystem.current.currentSelectedGameObject.GetComponentInParent<ConstrainSelectionToChildren>())
			{
				Select(lastAcceptedGameObject);
			}
			else
			{
				lastAcceptedGameObject = EventSystem.current.currentSelectedGameObject;
			}
		}
	}

	public void Select(GameObject go)
	{
		StartCoroutine(SetSelected(go));
	}

	private IEnumerator SetSelected(GameObject go)
	{
		EventSystem.current.SetSelectedGameObject(null);
		yield return new WaitForEndOfFrame();
		EventSystem.current.SetSelectedGameObject(go);
	}
}
