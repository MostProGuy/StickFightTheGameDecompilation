using LevelEditor;
using UnityEngine;

public class BackGround : MonoBehaviour
{
	private SpriteRenderer[] sprites;

	private bool leaving;

	[HideInInspector]
	public Vector3 StartScale;

	private Color[] startColors;

	private void Start()
	{
		StartScale = base.transform.localScale;
		sprites = GetComponentsInChildren<SpriteRenderer>();
		startColors = new Color[sprites.Length];
		for (int i = 0; i < startColors.Length; i++)
		{
			startColors[i] = sprites[i].color;
			sprites[i].color = new Color(startColors[i].r, startColors[i].g, startColors[i].b, 0f);
		}
		MapSizeHandler mapSizeHandler = Object.FindObjectOfType<MapSizeHandler>();
		if ((bool)mapSizeHandler)
		{
			mapSizeHandler.ScaleMe(this);
			return;
		}
		float lastAppliedScale = Object.FindObjectOfType<GameManager>().LastAppliedScale;
		base.transform.localScale = StartScale * lastAppliedScale;
	}

	private void LateUpdate()
	{
		float deltaTime = Time.deltaTime;
		deltaTime = Mathf.Clamp(deltaTime, 0f, 0.02f);
		for (int i = 0; i < sprites.Length; i++)
		{
			if (!leaving)
			{
				sprites[i].color = Color.Lerp(sprites[i].color, startColors[i], deltaTime * 3f);
			}
			else
			{
				sprites[i].color = Color.Lerp(sprites[i].color, new Color(startColors[i].r, startColors[i].g, startColors[i].b, 0f), deltaTime * 3f);
			}
		}
		if (!GameManager.stillInMenu)
		{
			base.transform.position = Vector3.zero + Vector3.right * 15f;
		}
	}

	public void FadeOut()
	{
		leaving = true;
	}
}
