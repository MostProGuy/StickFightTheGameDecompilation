using UnityEngine;

public class FadeSprite : MonoBehaviour
{
	private SpriteRenderer sprite;

	private LineRenderer line;

	private void Start()
	{
		line = GetComponent<LineRenderer>();
		sprite = GetComponent<SpriteRenderer>();
	}

	private void Update()
	{
		if ((bool)sprite)
		{
			sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, sprite.color.a - Time.deltaTime * 2f);
			if (sprite.color.a < 0f)
			{
				base.gameObject.SetActive(false);
			}
		}
		if ((bool)line)
		{
			line.material.color = new Color(line.material.color.r, line.material.color.g, line.material.color.b, line.material.color.a - Time.deltaTime * 2f);
			if (line.material.color.a < 0f)
			{
				base.gameObject.SetActive(false);
			}
		}
	}
}
