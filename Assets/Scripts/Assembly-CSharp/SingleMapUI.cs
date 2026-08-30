using System;
using System.IO;
using LevelEditor;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class SingleMapUI
{
	public bool IsLocallyActive;

	public string MapName;

	public string CategoryName;

	public string MapIndex;

	public string Description;

	public string DateTime;

	public MapType MapTypeEnum;

	private byte[] m_ImageData;

	public WorkshopMapWrapper CustomWrapper;

	public Toggle MapToggle { get; private set; }

	public SingleMapUI(string mapName, string category, int loadIndex, bool isOn, byte[] imageData)
	{
		MapName = mapName;
		CategoryName = category;
		MapIndex = loadIndex.ToString();
		IsLocallyActive = isOn;
		m_ImageData = imageData;
		DateTime = string.Empty;
		MapTypeEnum = MapType.Landfall;
		Debug.Log("New map: " + mapName + " Data: " + imageData.Length);
	}

	public SingleMapUI(WorkshopMapWrapper wrapper, string category, bool isOn)
	{
		CustomWrapper = wrapper;
		MapName = wrapper.LevelName;
		CategoryName = category;
		MapIndex = wrapper.PublishID.m_PublishedFileId.ToString();
		IsLocallyActive = isOn;
		DateTime = wrapper.DateTime;
		MapTypeEnum = MapType.CustomOnline;
	}

	public SingleMapUI(string mapName, string category, string mapIndex, bool isOn, byte[] imageData)
	{
		MapName = mapName;
		CategoryName = category;
		MapIndex = mapIndex;
		m_ImageData = imageData;
		IsLocallyActive = isOn;
		DateTime = new FileInfo(mapIndex + "/Level.bin").CreationTimeUtc.ToShortDateString();
		MapTypeEnum = MapType.CustomLocal;
	}

	public byte[] GetImageData()
	{
		if (MapTypeEnum == MapType.CustomOnline)
		{
			return CustomWrapper.PreviewFileData;
		}
		return m_ImageData;
	}

	public void AddToggle(Toggle t)
	{
		MapToggle = t;
	}
}
