using System;

namespace Landfall.Network.Sockets
{
	[Flags]
	public enum SpawnableObjectType : byte
	{
		Default = 0,
		Weapon = 1,
		ShallSyncPosition = 2
	}
}
