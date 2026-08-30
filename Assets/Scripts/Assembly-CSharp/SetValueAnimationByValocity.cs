using UnityEngine;

public class SetValueAnimationByValocity : MonoBehaviour
{
	public Rigidbody referenceRig;

	private ValueAnimation anims;

	private void Start()
	{
		anims = GetComponent<ValueAnimation>();
	}

	private void Update()
	{
		if (referenceRig.velocity.y > 3f)
		{
			anims.currentAnimationID = 0;
		}
		else if (referenceRig.velocity.y < -3f)
		{
			anims.currentAnimationID = 1;
		}
		else
		{
			anims.currentAnimationID = 2;
		}
	}
}
