using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class ArrowHandler : MonoBehaviour
{
	public bool right;

	private OptionsButton lastOptionsButton;

	private CodeAnimation codeAnim;

	private PauseManager pause;

	private void Awake()
	{
		codeAnim = GetComponent<CodeAnimation>();
		pause = base.transform.root.GetComponent<PauseManager>();
	}

	private void Update()
	{
		OptionsButton optionsButton = null;
		if ((bool)EventSystem.current.currentSelectedGameObject)
		{
			optionsButton = EventSystem.current.currentSelectedGameObject.GetComponent<OptionsButton>();
		}
		if ((bool)lastOptionsButton && right)
		{
			base.transform.parent.position = lastOptionsButton.transform.GetChild(0).position;
		}
		if ((bool)optionsButton)
		{
			lastOptionsButton = optionsButton;
		}
		if (EventSystem.current.currentSelectedGameObject == base.gameObject)
		{
			StartCoroutine(SetSelected());
		}
	}

	private IEnumerator SetSelected()
	{
		pause.sinceTransition = 0f;
		EventSystem.current.SetSelectedGameObject(null);
		yield return new WaitForEndOfFrame();
		EventSystem.current.SetSelectedGameObject(lastOptionsButton.gameObject);
		if (!lastOptionsButton.Locked)
		{
			if (right)
			{
				lastOptionsButton.MoveIndexRight();
			}
			else
			{
				lastOptionsButton.MoveIndexLeft();
			}
			codeAnim.Play();
		}
	}
}
