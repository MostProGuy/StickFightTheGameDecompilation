using System;
using System.Collections.Generic;
using UltimateFracturing;
using UnityEngine;

[Serializable]
[ExecuteInEditMode]
public class FracturedChunk : MonoBehaviour
{
	[Serializable]
	public class AdjacencyInfo
	{
		public FracturedChunk chunk;

		public float fArea;

		public AdjacencyInfo(FracturedChunk chunk, float fArea)
		{
			this.chunk = chunk;
			this.fArea = fArea;
		}
	}

	public class CollisionInfo
	{
		public FracturedChunk chunk;

		public Vector3 collisionPoint;

		public bool bIsMain;

		public bool bCancelCollisionEvent;

		public CollisionInfo(FracturedChunk chunk, Vector3 collisionPoint, bool bIsMain)
		{
			this.chunk = chunk;
			this.collisionPoint = collisionPoint;
			this.bIsMain = bIsMain;
			bCancelCollisionEvent = false;
		}
	}

	public FracturedObject FracturedObjectSource;

	public int SplitSubMeshIndex = -1;

	public bool DontDeleteAfterBroken;

	public bool IsSupportChunk;

	public bool IsNonSupportedChunk;

	public bool IsDetachedChunk;

	public float RelativeVolume = 0.01f;

	public float Volume;

	public bool HasConcaveCollider;

	public float PreviewDecompositionValue;

	public Color RandomMaterialColor = Color.white;

	public bool Visited;

	public List<AdjacencyInfo> ListAdjacentChunks = new List<AdjacencyInfo>();

	[SerializeField]
	private Vector3 m_v3InitialLocalPosition;

	[SerializeField]
	private Quaternion m_qInitialLocalRotation;

	[SerializeField]
	private Vector3 m_v3InitialLocalScale;

	[SerializeField]
	private bool m_bInitialLocalRotScaleInitialized;

	private List<AdjacencyInfo> ListAdjacentChunksCopy;

	private float m_fInvisibleTimer;

	private bool m_bNonSupportedChunkStored;

	private void Awake()
	{
		if (Application.isPlaying)
		{
			IsDetachedChunk = false;
			base.transform.localPosition = m_v3InitialLocalPosition;
			if (m_bInitialLocalRotScaleInitialized)
			{
				base.transform.localRotation = m_qInitialLocalRotation;
				base.transform.localScale = m_v3InitialLocalScale;
			}
			ListAdjacentChunksCopy = new List<AdjacencyInfo>(ListAdjacentChunks);
			m_fInvisibleTimer = 0f;
		}
		m_bNonSupportedChunkStored = IsNonSupportedChunk;
	}

	private void Update()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		if (!GetComponent<Renderer>().isVisible && IsDetachedChunk)
		{
			m_fInvisibleTimer += Time.deltaTime;
			if (FracturedObjectSource != null && m_fInvisibleTimer > FracturedObjectSource.EventDetachedOffscreenLifeTime)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
		else
		{
			m_fInvisibleTimer = 0f;
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.contacts != null && collision.contacts.Length != 0)
		{
			HandleCollision(collision.collider, collision.contacts[0].point, collision.relativeVelocity.magnitude);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!(FracturedObjectSource == null) && !(other == null))
		{
			Rigidbody attachedRigidbody = other.attachedRigidbody;
			HandleCollision(other, other.transform.position, (!(attachedRigidbody != null)) ? 0f : attachedRigidbody.velocity.magnitude);
		}
	}

	private void HandleCollision(Collider other, Vector3 v3CollisionPos, float relativeSpeed)
	{
		if (FracturedObjectSource == null || other == null)
		{
			return;
		}
		if ((bool)other.gameObject)
		{
			FracturedChunk component = other.gameObject.GetComponent<FracturedChunk>();
			if ((bool)component && other.attachedRigidbody != null && other.attachedRigidbody.isKinematic && !IsDetachedChunk)
			{
				return;
			}
		}
		float num = float.PositiveInfinity;
		Rigidbody attachedRigidbody = other.attachedRigidbody;
		if (attachedRigidbody != null)
		{
			num = attachedRigidbody.mass;
		}
		if (!IsDetachedChunk)
		{
			bool flag = false;
			FracturedChunk component2 = other.gameObject.GetComponent<FracturedChunk>();
			if (component2 != null && component2.IsDetachedChunk && component2.FracturedObjectSource == FracturedObjectSource)
			{
				flag = true;
			}
			if (flag || !(relativeSpeed > FracturedObjectSource.EventDetachMinVelocity) || !(num > FracturedObjectSource.EventDetachMinMass) || !(GetComponent<Collider>() != null) || !GetComponent<Collider>().attachedRigidbody || !IsDestructibleChunk())
			{
				return;
			}
			CollisionInfo collisionInfo = new CollisionInfo(this, v3CollisionPos, true);
			FracturedObjectSource.NotifyDetachChunkCollision(collisionInfo);
			if (collisionInfo.bCancelCollisionEvent)
			{
				return;
			}
			List<FracturedChunk> list = new List<FracturedChunk>();
			list = ComputeRandomConnectionBreaks();
			list.Add(this);
			DetachFromObject();
			{
				foreach (FracturedChunk item in list)
				{
					FracturedChunk fracturedChunk = (collisionInfo.chunk = item);
					collisionInfo.bIsMain = false;
					collisionInfo.bCancelCollisionEvent = false;
					if (fracturedChunk != this)
					{
						FracturedObjectSource.NotifyDetachChunkCollision(collisionInfo);
					}
					if (!collisionInfo.bCancelCollisionEvent)
					{
						fracturedChunk.DetachFromObject();
						fracturedChunk.GetComponent<Collider>().attachedRigidbody.AddExplosionForce(relativeSpeed * FracturedObjectSource.EventDetachExitForce, attachedRigidbody.transform.position, 0f, FracturedObjectSource.EventDetachUpwardsModifier);
					}
				}
				return;
			}
		}
		Rigidbody attachedRigidbody2 = GetComponent<Collider>().attachedRigidbody;
		Vector3 vector = ((!(attachedRigidbody != null)) ? Vector3.zero : attachedRigidbody.velocity);
		float magnitude = (vector - attachedRigidbody2.velocity).magnitude;
		if (magnitude > FracturedObjectSource.EventDetachedMinVelocity && num > FracturedObjectSource.EventDetachedMinMass)
		{
			FracturedObjectSource.NotifyFreeChunkCollision(new CollisionInfo(this, v3CollisionPos, true));
		}
	}

	public bool IsDestructibleChunk()
	{
		if (FracturedObjectSource != null)
		{
			if (FracturedObjectSource.SupportChunksAreIndestructible)
			{
				return !IsSupportChunk;
			}
			if (!FracturedObjectSource.SupportChunksAreIndestructible)
			{
				return true;
			}
		}
		return !IsSupportChunk;
	}

	public void ResetChunk(FracturedObject fracturedObjectSource)
	{
		base.transform.parent = fracturedObjectSource.transform;
		GetComponent<Rigidbody>().isKinematic = true;
		GetComponent<Collider>().isTrigger = true;
		IsNonSupportedChunk = m_bNonSupportedChunkStored;
		FracturedObjectSource = fracturedObjectSource;
		IsDetachedChunk = false;
		base.transform.localPosition = m_v3InitialLocalPosition;
		if (m_bInitialLocalRotScaleInitialized)
		{
			base.transform.localRotation = m_qInitialLocalRotation;
			base.transform.localScale = m_v3InitialLocalScale;
		}
		ListAdjacentChunks = new List<AdjacencyInfo>(ListAdjacentChunksCopy);
		m_fInvisibleTimer = 0f;
	}

	public void Impact(Vector3 v3Position, float fExplosionForce, float fRadius, bool bAlsoImpactFreeChunks)
	{
		if (GetComponent<Rigidbody>() != null && IsDestructibleChunk())
		{
			List<FracturedChunk> list = new List<FracturedChunk>();
			if (!IsDetachedChunk)
			{
				list = ComputeRandomConnectionBreaks();
				list.Add(this);
				DetachFromObject();
				foreach (FracturedChunk item in list)
				{
					item.DetachFromObject();
					item.GetComponent<Rigidbody>().AddExplosionForce(fExplosionForce, v3Position, 0f, 0f);
				}
			}
			List<FracturedChunk> destructibleChunksInRadius = FracturedObjectSource.GetDestructibleChunksInRadius(v3Position, fRadius, bAlsoImpactFreeChunks);
			foreach (FracturedChunk item2 in destructibleChunksInRadius)
			{
				item2.DetachFromObject();
				item2.GetComponent<Rigidbody>().AddExplosionForce(fExplosionForce, v3Position, 0f, FracturedObjectSource.EventDetachUpwardsModifier);
			}
		}
		FracturedObjectSource.NotifyImpact(v3Position);
	}

	public void OnCreateFromFracturedObject(FracturedObject fracturedComponent, int nSplitSubMeshIndex)
	{
		FracturedObjectSource = fracturedComponent;
		SplitSubMeshIndex = nSplitSubMeshIndex;
		RandomMaterialColor = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value, 0.7f);
		m_v3InitialLocalPosition = base.transform.localPosition;
		m_qInitialLocalRotation = base.transform.localRotation;
		m_v3InitialLocalScale = base.transform.localScale;
		m_bInitialLocalRotScaleInitialized = true;
	}

	public void UpdatePreviewDecompositionPosition()
	{
		float num = 5f;
		float num2 = 1f;
		if (FracturedObjectSource != null)
		{
			num2 = m_v3InitialLocalPosition.magnitude / FracturedObjectSource.DecomposeRadius;
		}
		Vector3 normalized = m_v3InitialLocalPosition.normalized;
		base.transform.localPosition = m_v3InitialLocalPosition + normalized * (PreviewDecompositionValue * num2 * num);
	}

	public void ConnectTo(FracturedChunk chunk, float fArea)
	{
		if ((bool)chunk && !chunk.IsConnectedTo(this))
		{
			ListAdjacentChunks.Add(new AdjacencyInfo(chunk, fArea));
			chunk.ListAdjacentChunks.Add(new AdjacencyInfo(this, fArea));
		}
	}

	public void DisconnectFrom(FracturedChunk chunk)
	{
		if (!chunk || !chunk.IsConnectedTo(this))
		{
			return;
		}
		for (int i = 0; i < ListAdjacentChunks.Count; i++)
		{
			if (ListAdjacentChunks[i].chunk == chunk)
			{
				ListAdjacentChunks.RemoveAt(i);
				break;
			}
		}
		for (int j = 0; j < chunk.ListAdjacentChunks.Count; j++)
		{
			if (chunk.ListAdjacentChunks[j].chunk == this)
			{
				chunk.ListAdjacentChunks.RemoveAt(j);
				break;
			}
		}
	}

	public bool IsConnectedTo(FracturedChunk chunk)
	{
		foreach (AdjacencyInfo listAdjacentChunk in ListAdjacentChunks)
		{
			bool result = true;
			if ((bool)listAdjacentChunk.chunk.FracturedObjectSource)
			{
				result = listAdjacentChunk.fArea > listAdjacentChunk.chunk.FracturedObjectSource.ChunkConnectionMinArea;
			}
			if (listAdjacentChunk.chunk == chunk)
			{
				return result;
			}
		}
		return false;
	}

	public void DetachFromObject(bool bCheckStructureIntegrity = true)
	{
		if (!IsDestructibleChunk() || IsDetachedChunk || !GetComponent<Rigidbody>())
		{
			return;
		}
		m_bNonSupportedChunkStored = IsNonSupportedChunk;
		base.transform.parent = null;
		GetComponent<Rigidbody>().isKinematic = false;
		GetComponent<Collider>().isTrigger = false;
		IsDetachedChunk = true;
		IsNonSupportedChunk = true;
		RemoveConnectionInfo();
		if ((bool)FracturedObjectSource)
		{
			FracturedObjectSource.NotifyChunkDetach(this);
			if (bCheckStructureIntegrity)
			{
				FracturedObjectSource.CheckDetachNonSupportedChunks();
			}
		}
		if (!DontDeleteAfterBroken && FracturedObjectSource != null)
		{
			DieTimer dieTimer = base.gameObject.AddComponent<DieTimer>();
			dieTimer.SecondsToDie = UnityEngine.Random.Range(FracturedObjectSource.EventDetachedMinLifeTime, FracturedObjectSource.EventDetachedMaxLifeTime);
		}
	}

	private void RemoveConnectionInfo()
	{
		foreach (AdjacencyInfo listAdjacentChunk in ListAdjacentChunks)
		{
			if (!listAdjacentChunk.chunk)
			{
				continue;
			}
			foreach (AdjacencyInfo listAdjacentChunk2 in listAdjacentChunk.chunk.ListAdjacentChunks)
			{
				if (listAdjacentChunk2.chunk == this)
				{
					listAdjacentChunk.chunk.ListAdjacentChunks.Remove(listAdjacentChunk2);
					break;
				}
			}
		}
		ListAdjacentChunks.Clear();
	}

	public List<FracturedChunk> ComputeRandomConnectionBreaks()
	{
		List<FracturedChunk> list = new List<FracturedChunk>();
		if (FracturedObjectSource == null)
		{
			return list;
		}
		FracturedObjectSource.ResetAllChunkVisitedFlags();
		ComputeRandomConnectionBreaksRecursive(this, list, 1);
		return list;
	}

	private static void ComputeRandomConnectionBreaksRecursive(FracturedChunk chunk, List<FracturedChunk> listBreaksOut, int nLevel)
	{
		if (chunk.Visited)
		{
			return;
		}
		chunk.Visited = true;
		foreach (AdjacencyInfo listAdjacentChunk in chunk.ListAdjacentChunks)
		{
			if ((bool)listAdjacentChunk.chunk && chunk.FracturedObjectSource != null && !listAdjacentChunk.chunk.Visited && listAdjacentChunk.chunk.IsDestructibleChunk() && listAdjacentChunk.fArea > chunk.FracturedObjectSource.ChunkConnectionMinArea)
			{
				float value = UnityEngine.Random.value;
				if (value > chunk.FracturedObjectSource.ChunkConnectionStrength * (float)nLevel)
				{
					ComputeRandomConnectionBreaksRecursive(listAdjacentChunk.chunk, listBreaksOut, nLevel + 1);
					listBreaksOut.Add(listAdjacentChunk.chunk);
				}
			}
		}
	}

	public static FracturedChunk ChunkRaycast(Vector3 v3Pos, Vector3 v3Forward, out RaycastHit hitInfo)
	{
		FracturedChunk fracturedChunk = null;
		if (Physics.Raycast(v3Pos, v3Forward, out hitInfo))
		{
			fracturedChunk = hitInfo.collider.GetComponent<FracturedChunk>();
			if (fracturedChunk == null && hitInfo.collider.transform.parent != null)
			{
				fracturedChunk = hitInfo.collider.transform.parent.GetComponent<FracturedChunk>();
			}
		}
		return fracturedChunk;
	}
}
