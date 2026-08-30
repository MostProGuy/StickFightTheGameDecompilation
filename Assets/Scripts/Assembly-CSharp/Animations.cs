using UnityEngine;

public class Animations : MonoBehaviour
{
	private JointAnimation[] jointAnimations;

	private CharacterInformation info;

	private void Start()
	{
		jointAnimations = GetComponentsInChildren<JointAnimation>();
		info = GetComponent<CharacterInformation>();
	}

	private void Update()
	{
	}

	public void Run()
	{
		if (!(info.sinceFallen < 0f))
		{
			JointAnimation[] array = jointAnimations;
			foreach (JointAnimation jointAnimation in array)
			{
				jointAnimation.Animate(0, info.leftSideForward);
			}
		}
	}
}
