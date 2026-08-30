using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class MemoryBucket
{
	public delegate void OnMemoryBucketEmptySignature(MemoryBucket memoryBucket);

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct MemoryEvent<T>
	{
		public delegate void OnValueChangedSignature(string key, T value);

		public delegate void OnValueRemovedSignature(string key);

		public static Dictionary<MemoryBucket, Dictionary<string, OnValueChangedSignature>> sOnValueChanged = new Dictionary<MemoryBucket, Dictionary<string, OnValueChangedSignature>>();

		public static Dictionary<MemoryBucket, Dictionary<string, OnValueRemovedSignature>> sOnValueRemoved = new Dictionary<MemoryBucket, Dictionary<string, OnValueRemovedSignature>>();

		public static void RemoveEventBindings(MemoryBucket key)
		{
			sOnValueChanged.Remove(key);
			sOnValueRemoved.Remove(key);
		}
	}

	public OnMemoryBucketEmptySignature mOnMemoryBucketCleared;

	private Dictionary<string, object> mMemory = new Dictionary<string, object>();

	public void Log()
	{
		foreach (KeyValuePair<string, object> item in mMemory)
		{
			Debug.Log(item.ToString());
		}
	}

	public void ClearMemory()
	{
		mMemory.Clear();
	}

	public void ClearEventBindings()
	{
		if (mOnMemoryBucketCleared != null)
		{
			mOnMemoryBucketCleared(this);
		}
	}

	~MemoryBucket()
	{
		ClearEventBindings();
		ClearMemory();
	}

	private void SetupAndBindMemoryEvent<T>()
	{
		if (!MemoryEvent<T>.sOnValueChanged.ContainsKey(this))
		{
			MemoryEvent<T>.sOnValueChanged.Add(this, new Dictionary<string, MemoryEvent<T>.OnValueChangedSignature>());
			MemoryEvent<T>.sOnValueRemoved.Add(this, new Dictionary<string, MemoryEvent<T>.OnValueRemovedSignature>());
			mOnMemoryBucketCleared = (OnMemoryBucketEmptySignature)Delegate.Combine(mOnMemoryBucketCleared, new OnMemoryBucketEmptySignature(MemoryEvent<T>.RemoveEventBindings));
		}
	}

	public void OnValueChanged<T>(string key, MemoryEvent<T>.OnValueChangedSignature binding)
	{
		SetupAndBindMemoryEvent<T>();
		if (!MemoryEvent<T>.sOnValueChanged[this].ContainsKey(key))
		{
			MemoryEvent<T>.sOnValueChanged[this].Add(key, binding);
		}
		else
		{
			MemoryEvent<T>.sOnValueChanged[this][key] = (MemoryEvent<T>.OnValueChangedSignature)Delegate.Combine(MemoryEvent<T>.sOnValueChanged[this][key], binding);
		}
	}

	public void OnValueRemoved<T>(string key, MemoryEvent<T>.OnValueRemovedSignature binding)
	{
		SetupAndBindMemoryEvent<T>();
		if (!MemoryEvent<T>.sOnValueRemoved[this].ContainsKey(key))
		{
			MemoryEvent<T>.sOnValueRemoved[this].Add(key, binding);
		}
		else
		{
			MemoryEvent<T>.sOnValueRemoved[this][key] = (MemoryEvent<T>.OnValueRemovedSignature)Delegate.Combine(MemoryEvent<T>.sOnValueRemoved[this][key], binding);
		}
	}

	public Optional<T> Copy<T>(string key)
	{
		if (mMemory.ContainsKey(key))
		{
			return new Optional<T>((T)mMemory[key]);
		}
		return new Optional<T>();
	}

	public Optional<T> Take<T>(string key)
	{
		if (mMemory.ContainsKey(key))
		{
			T value = (T)mMemory[key];
			mMemory.Remove(key);
			if (MemoryEvent<T>.sOnValueRemoved.ContainsKey(this) && MemoryEvent<T>.sOnValueRemoved[this].ContainsKey(key))
			{
				MemoryEvent<T>.sOnValueRemoved[this][key](key);
			}
			return new Optional<T>(value);
		}
		return new Optional<T>();
	}

	public void Put<T>(string key, T value)
	{
		if (!mMemory.ContainsKey(key))
		{
			mMemory.Add(key, value);
		}
		else
		{
			mMemory[key] = value;
		}
		if (MemoryEvent<T>.sOnValueChanged.ContainsKey(this) && MemoryEvent<T>.sOnValueChanged[this].ContainsKey(key))
		{
			MemoryEvent<T>.sOnValueChanged[this][key](key, value);
		}
	}
}
