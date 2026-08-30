using LevelEditor;
using UnityEngine;

public class LevelEditorSprite : MonoBehaviour
{
	private SpriteRenderer renderer;

	private void Start()
	{
		if (WorkshopStateHandler.IsPlayTestingMode)
		{
			renderer = GetComponent<SpriteRenderer>();
		}
		else
		{
			Object.Destroy(this);
		}
	}

	private void Update()
	{
	}
}
