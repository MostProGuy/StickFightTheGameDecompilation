using UnityEngine;

public class DestructibleObject : MonoBehaviour
{
	public float forceThreshold = 8f;

	public bool simpleDestruction = true;

	public bool shouldDisableAllRigidBodiesOnInit = true;

	public Controller damager;

	public IceDestructionAudio m_iceDestructionAudio;
}
