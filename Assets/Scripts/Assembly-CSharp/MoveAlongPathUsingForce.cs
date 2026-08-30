using System.IO;
using UnityEngine;

public class MoveAlongPathUsingForce : MapInfoSyncableBase
{
	public bool canPlay;

	public bool auto = true;

	public float forceAmount;

	public float changeDirectionDelay;

	public float speedUpTime = 1f;

	private float speedChangeMultiplier = 1f;

	private float counter;

	private float inRangeCounter;

	public Transform[] targets;

	public Vector3[] positions;

	public float[] forceMultipliers;

	private Rigidbody rig;

	private int currentTargetId;

	public float startCounter;

	protected override void Awake()
	{
		base.Awake();
	}

	private void Start()
	{
		rig = GetComponent<Rigidbody>();
		if (targets.Length > 0)
		{
			positions = new Vector3[targets.Length];
			for (int i = 0; i < positions.Length; i++)
			{
				positions[i] = targets[i].localPosition;
			}
		}
		else
		{
			for (int j = 0; j < positions.Length; j++)
			{
				positions[j] = base.transform.TransformPoint(positions[j]);
			}
		}
	}

	protected override void Update()
	{
		if (!auto && !canPlay && currentTargetId == 0)
		{
			return;
		}
		startCounter -= Time.deltaTime;
		if (startCounter > 0f)
		{
			return;
		}
		counter += Time.deltaTime;
		speedChangeMultiplier = Mathf.Clamp(counter - changeDirectionDelay, 0f, speedUpTime) * 1f / speedUpTime;
		Vector3 vector = positions[currentTargetId] - base.transform.position;
		if (vector.magnitude > 5f)
		{
			vector = vector.normalized * 5f;
		}
		float num = 1f;
		if (forceMultipliers != null && forceMultipliers.Length > 0)
		{
			num = forceMultipliers[currentTargetId];
		}
		rig.AddForce(vector * num * speedChangeMultiplier * Time.deltaTime * forceAmount, ForceMode.Acceleration);
		if (MatchmakingHandler.IsNetworkMatch && !MapInfoSyncableBase.m_NetworkControl)
		{
			return;
		}
		if (Vector3.Distance(base.transform.position, positions[currentTargetId]) < 1f)
		{
			inRangeCounter += Time.deltaTime;
			if (inRangeCounter > changeDirectionDelay)
			{
				inRangeCounter = 0f;
				counter = 0f;
				currentTargetId++;
				if (currentTargetId >= positions.Length)
				{
					currentTargetId = 0;
					canPlay = false;
				}
				if (MapInfoSyncableBase.m_NetworkControl)
				{
					SendNewStatePackage();
				}
			}
		}
		base.Update();
	}

	public byte GetCurrentPositionIndexWithLatency(float latencyInMS)
	{
		byte b = (byte)currentTargetId;
		Debug.Log("Current MovePosition index for: " + base.gameObject.name + " Is " + b);
		return b;
	}

	public void SetNewPositionIndex(byte newIndex)
	{
		Debug.Log("Set New Position INdex For: " + base.gameObject.name + " : " + newIndex);
		currentTargetId = newIndex;
	}

	public override byte[] GetData()
	{
		byte[] array = new byte[1];
		byte b = (byte)currentTargetId;
		using (MemoryStream output = new MemoryStream(array))
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(output))
			{
				binaryWriter.Write(b);
			}
		}
		Debug.Log("Returning new state: " + b);
		return array;
	}

	public override void SetData(byte[] data)
	{
		int num;
		using (MemoryStream input = new MemoryStream(data))
		{
			using (BinaryReader binaryReader = new BinaryReader(input))
			{
				num = binaryReader.ReadByte();
			}
		}
		if (num != currentTargetId)
		{
			currentTargetId = num;
			inRangeCounter = 0f;
			counter = 0f;
			Debug.Log("Setting new state: " + num);
		}
	}
}
