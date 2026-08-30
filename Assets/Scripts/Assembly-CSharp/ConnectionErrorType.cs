public enum ConnectionErrorType : byte
{
	None = 0,
	TimeOut = 1,
	MatchFull = 2,
	NoConnection = 3,
	NoConnectionToHost = 4,
	Unknown = 5,
	SteamNotInit = 6,
	InvalidVersion = 7,
	Kicked = 8,
	DownloadFailure = 9
}
