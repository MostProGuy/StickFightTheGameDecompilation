using System.Collections;
using UnityEngine;

public class PlayMoveAnimations : MonoBehaviour
{
	public float delay;

	public MoveAlongPathUsingForce[] animations;

	public PhysicsAnimation[] anims;

	public CodeAnimation[] codeAnims;

	public CodeStateAnimation[] codeStateAnim;

	private void Start()
	{
		animations = GetComponentsInChildren<MoveAlongPathUsingForce>();
		anims = GetComponentsInChildren<PhysicsAnimation>();
		codeAnims = GetComponentsInChildren<CodeAnimation>();
		codeStateAnim = GetComponentsInChildren<CodeStateAnimation>();
	}

	private void Update()
	{
	}

	public void GO()
	{
		StartCoroutine(PlayThings());
	}

	public void Stop()
	{
		for (int i = 0; i < anims.Length; i++)
		{
			anims[i].Stop();
		}
	}

	private IEnumerator PlayThings()
	{
		for (int i = 0; i < anims.Length; i++)
		{
			anims[i].Play();
			yield return new WaitForSeconds(delay);
		}
		for (int j = 0; j < animations.Length; j++)
		{
			animations[j].canPlay = true;
			yield return new WaitForSeconds(delay);
		}
		for (int k = 0; k < codeAnims.Length; k++)
		{
			codeAnims[k].Play();
			yield return new WaitForSeconds(delay);
		}
		for (int l = 0; l < codeStateAnim.Length; l++)
		{
			codeStateAnim[l].state1 = !codeStateAnim[l].state1;
			yield return new WaitForSeconds(delay);
		}
	}
}
