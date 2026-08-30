using System.IO;
using UnityEngine;

public class PillarHandler : MapInfoSyncableBase
{
	public float value;

	public float valueCap;

	public bool isBeingStoodOn;

	private float sinceStoodOn = 1f;

	private float startPosition;

	private float velocity;

	public float multiplier = 1f;

	protected override void Awake()
	{
		base.Awake();
	}

	private void Start()
	{
		startPosition = base.transform.position.y;
	}

	protected override void Update()
	{
		MoveTowardsValue();
		sinceStoodOn += Time.deltaTime;
		if (isBeingStoodOn)
		{
			sinceStoodOn = 0f;
		}
		if (sinceStoodOn < 0.1f)
		{
			value -= Time.deltaTime * 1.5f * multiplier;
		}
		if (valueCap != 0f)
		{
			value = Mathf.Clamp(value, valueCap, 100f);
		}
		base.Update();
	}

	private void FixedUpdate()
	{
		velocity *= 0.8f;
	}

	private void MoveTowardsValue()
	{
		velocity += (startPosition + value - base.transform.position.y) * Time.deltaTime * 20f;
		base.transform.position += new Vector3(0f, velocity, 0f) * Time.deltaTime;
	}

	private void OnCollisionEnter(Collision collision)
	{
		Collide(collision);
	}

	public void OnCollisionStay(Collision collision)
	{
		Collide(collision);
	}

	private void Collide(Collision collision)
	{
		if ((bool)collision.rigidbody && (bool)collision.rigidbody.GetComponent<BodyPart>() && collision.contacts[0].point.y > base.transform.position.y + 5.5f)
		{
			sinceStoodOn = 0f;
		}
	}

	public override byte[] GetData()
	{
		byte[] array = new byte[4];
		using (MemoryStream output = new MemoryStream(array))
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(output))
			{
				binaryWriter.Write(value);
				return array;
			}
		}
	}

	public override void SetData(byte[] data)
	{
		float num;
		using (MemoryStream input = new MemoryStream(data))
		{
			using (BinaryReader binaryReader = new BinaryReader(input))
			{
				num = binaryReader.ReadSingle();
			}
		}
		value = num;
	}
}
