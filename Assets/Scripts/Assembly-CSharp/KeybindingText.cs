using TMPro;
using UnityEngine;

public class KeybindingText : MonoBehaviour
{
	public string Keybinding;

	private void OnEnable()
	{
		UpdateText();
	}

	public void UpdateText()
	{
		TextMeshProUGUI component = base.gameObject.GetComponent<TextMeshProUGUI>();
		if (component != null)
		{
			string keybindingString = CharacterActions.GetKeybindingString(Keybinding);
			if (keybindingString != null && keybindingString.Length > 0)
			{
				component.text = keybindingString;
				component.fontSize = Mathf.Min(300 / keybindingString.Length, 45);
			}
		}
	}

	private void Update()
	{
	}
}
