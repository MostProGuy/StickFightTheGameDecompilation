using UnityEngine;

namespace LevelEditor
{
	public class ToolButtonUI : MonoBehaviour
	{
		private string m_Name = string.Empty;

		public void Init(string objectName)
		{
			m_Name = objectName;
			base.gameObject.name = m_Name + "ToolButton";
		}

		public void OnButtonWasClicked()
		{
			if (LevelEditorInputManager.CanUseMouse)
			{
				if (m_Name == string.Empty)
				{
					Debug.LogError("Name is Empty!", this);
				}
				Object.FindObjectOfType<LevelCreator>().SetSelectedGameObjectFromUI(m_Name);
			}
		}
	}
}
