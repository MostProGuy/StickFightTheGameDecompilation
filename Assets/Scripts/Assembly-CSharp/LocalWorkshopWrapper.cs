public class LocalWorkshopWrapper
{
	public string MapName { get; private set; }

	public string MapPath { get; private set; }

	public byte[] ImageData { get; private set; }

	public LocalWorkshopWrapper(string mapName, string mapPath, byte[] imageData)
	{
		MapName = mapName;
		MapPath = mapPath;
		ImageData = imageData;
	}
}
