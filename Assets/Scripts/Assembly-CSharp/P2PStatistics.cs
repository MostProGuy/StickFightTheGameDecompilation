using System;
using System.Collections.Generic;
using UnityEngine;

public class P2PStatistics : MonoBehaviour
{
	private static Dictionary<P2PPackageHandler.MsgType, PacketTypeStatisticsWrap> mPackageTypeStats = new Dictionary<P2PPackageHandler.MsgType, PacketTypeStatisticsWrap>();

	private static bool mIsRecording = false;

	private byte mInterval = 1;

	private static uint mBytesSentLastInterval = 0u;

	private static uint mbytesSentTotal = 0u;

	private static uint mbytesPeak = 0u;

	private static uint mTotalPackages = 0u;

	private float mCurrentTimer;

	private static float mTotalTime = 0f;

	private void Awake()
	{
		Array values = Enum.GetValues(typeof(P2PPackageHandler.MsgType));
		foreach (object item in values)
		{
			mPackageTypeStats.Add((P2PPackageHandler.MsgType)item, new PacketTypeStatisticsWrap());
		}
	}

	private void Update()
	{
		if (mIsRecording)
		{
			TickInterval();
		}
	}

	private void TickInterval()
	{
		mCurrentTimer += Time.deltaTime;
		mTotalTime += Time.deltaTime;
		if (mCurrentTimer >= (float)(int)mInterval)
		{
			PrintSentBytes();
		}
	}

	private void PrintSentBytes()
	{
		mCurrentTimer = 0f;
		Debug.Log("kB sent last " + mInterval + " Second/s  : " + mBytesSentLastInterval * 8 / 1000);
		mbytesSentTotal += mBytesSentLastInterval;
		if (mBytesSentLastInterval > mbytesPeak)
		{
			mbytesPeak = mBytesSentLastInterval;
		}
		mBytesSentLastInterval = 0u;
	}

	public static void BytesWasSent(uint nrOfBytes, P2PPackageHandler.MsgType type)
	{
		if (mIsRecording)
		{
			mBytesSentLastInterval += nrOfBytes;
			mTotalPackages++;
			PacketTypeStatisticsWrap value;
			mPackageTypeStats.TryGetValue(type, out value);
			value.AddPackage(nrOfBytes);
		}
	}

	public static void StartRecording()
	{
		ResetBytes();
		mIsRecording = true;
	}

	private static void ResetBytes()
	{
		mbytesPeak = 0u;
		mBytesSentLastInterval = 0u;
		mbytesSentTotal = 0u;
		mTotalTime = 0f;
		mTotalPackages = 0u;
	}

	public static void StopRecording()
	{
		mIsRecording = false;
		uint num = mbytesSentTotal * 8 / 1000;
		uint num2 = mbytesPeak * 8 / 1000;
		float num3 = (float)mbytesSentTotal / mTotalTime * 8f / 1000f;
		Debug.Log("MATCH FINISHED! Total kB data sent this match " + num + "  With a peak of: " + num2 + "kb/s Lasted: " + mTotalTime + " Seconds AND " + mTotalPackages + " Total packages was sent!   AVG kbps: " + num3);
		PrintMessageTypes();
	}

	private static void PrintMessageTypes()
	{
		uint num = 0u;
		uint num2 = 0u;
		foreach (KeyValuePair<P2PPackageHandler.MsgType, PacketTypeStatisticsWrap> mPackageTypeStat in mPackageTypeStats)
		{
			P2PPackageHandler.MsgType key = mPackageTypeStat.Key;
			num = mPackageTypeStat.Value.NumberOfPackages;
			if (num != 0)
			{
				num2 = mPackageTypeStat.Value.AmountOfDataSent;
				Debug.Log(key.ToString() + " Msg: " + num + " Data: " + (float)(num2 * 8) / 1000f + " Kb");
				mPackageTypeStat.Value.Clear();
			}
		}
	}
}
