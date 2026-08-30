using System;

[Flags]
public enum SpawnableObjectType : byte
{
	Default = 0,
	Weapon = 1,
	ShallSyncPosition = 2
}
