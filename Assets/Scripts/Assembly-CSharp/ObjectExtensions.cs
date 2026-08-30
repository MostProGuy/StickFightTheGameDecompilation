using UnityEngine;

public static class ObjectExtensions
{
	public static T FetchComponent<T>(this GameObject obj) where T : MonoBehaviour
	{
		T component = obj.GetComponent<T>();
		if (component != null)
		{
			return component;
		}
		return obj.AddComponent<T>();
	}

	public static void SetRectWidth(this RectTransform trans, Vector2 newSize)
	{
		Vector2 size = trans.rect.size;
		Vector2 vector = newSize - size;
		trans.offsetMin -= new Vector2(vector.x * trans.pivot.x, vector.y * trans.pivot.y);
		trans.offsetMax += new Vector2(vector.x * (1f - trans.pivot.x), vector.y * (1f - trans.pivot.y));
	}
}
