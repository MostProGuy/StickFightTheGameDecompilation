public class PacketTypeStatisticsWrap
{
	private uint mCount;

	private uint mAmountOfBytes;

	public uint NumberOfPackages
	{
		get
		{
			return mCount;
		}
	}

	public uint AmountOfDataSent
	{
		get
		{
			return mAmountOfBytes;
		}
	}

	public PacketTypeStatisticsWrap()
	{
		mCount = 0u;
		mAmountOfBytes = 0u;
	}

	public void AddPackage(uint size)
	{
		mCount++;
		mAmountOfBytes += size;
	}

	public void Clear()
	{
		mCount = 0u;
		mAmountOfBytes = 0u;
	}
}
