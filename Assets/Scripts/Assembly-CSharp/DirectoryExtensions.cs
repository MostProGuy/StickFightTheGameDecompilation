using System.Collections.Generic;
using System.IO;

public static class DirectoryExtensions
{
	public static DirectoryInfo[] Merge(this DirectoryInfo[] dir, DirectoryInfo[] mergeWith)
	{
		List<DirectoryInfo> list = new List<DirectoryInfo>();
		foreach (DirectoryInfo item in dir)
		{
			list.Add(item);
		}
		foreach (DirectoryInfo item2 in mergeWith)
		{
			list.Add(item2);
		}
		return list.ToArray();
	}
}
