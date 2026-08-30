namespace Landfall.Network.Sockets
{
	public class SocketPlayerLoginInformation
	{
		public string PlayerName { get; private set; }

		public SocketPlayerLoginInformation(string playerName)
		{
			PlayerName = playerName;
		}
	}
}
