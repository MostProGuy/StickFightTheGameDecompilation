using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResolutionSetting : MonoBehaviour
{
	public OptionsButton ResolutionOptionsButton;

	private void Awake()
	{
		if (ResolutionOptionsButton != null)
		{
			List<Resolution> list = new List<Resolution>(Screen.resolutions).Distinct().ToList();
			list.Sort((Resolution x, Resolution y) => x.width - y.width);
			ResolutionOptionsButton.values = new string[list.Count];
			for (int num = 0; num < list.Count; num++)
			{
				ResolutionOptionsButton.values[num] = list[num].width + "x" + list[num].height;
			}
		}
	}
}
