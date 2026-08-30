using System.Collections;
using System.Collections.ObjectModel;
using InControl;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectMe : MonoBehaviour
{
	private PauseManager pause;

	private MapSelectionHandler mapSelection;

	private void Awake()
	{
		pause = base.transform.root.GetComponent<PauseManager>();
	}

	private void Start()
	{
		mapSelection = base.transform.root.GetComponent<MapSelectionHandler>();
	}

	private void Update()
	{
		if (!(pause.sinceTransition > 0.5f) || !CheckUsable() || !PauseManager.isPaused || (bool)EventSystem.current.currentSelectedGameObject)
		{
			return;
		}
		ReadOnlyCollection<InputDevice> devices = InputManager.Devices;
		foreach (InputDevice item in devices)
		{
			if ((bool)item.AnyButton || item.LeftStick.Value.magnitude > 0.1f || item.RightStick.Value.magnitude > 0.1f)
			{
				Select();
				PauseManager.usedKeyboard = false;
			}
		}
	}

	public void Select()
	{
		pause.sinceTransition = 0f;
		StartCoroutine(SetSelected());
	}

	private IEnumerator SetSelected()
	{
		EventSystem.current.SetSelectedGameObject(null);
		yield return new WaitForEndOfFrame();
		EventSystem.current.SetSelectedGameObject(base.gameObject);
	}

	private bool CheckUsable()
	{
		bool result = false;
		if (base.transform.name == "OptionsButton" && pause.isInOptionsMenu && !mapSelection.Active)
		{
			result = true;
		}
		if (base.transform.name == "Play" && !pause.isInOptionsMenu && !mapSelection.Active)
		{
			result = true;
		}
		if (base.transform.name == "BackButtonMap" && mapSelection.Active)
		{
			result = true;
		}
		return result;
	}
}
