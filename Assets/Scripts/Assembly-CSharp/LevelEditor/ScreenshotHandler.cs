using System.Collections;
using System.IO;
using UnityEngine;

namespace LevelEditor
{
	public class ScreenshotHandler
	{
		private static ScreenshotHandler _instance = new ScreenshotHandler();

		public static ScreenshotHandler Instance
		{
			get
			{
				return _instance;
			}
		}

		public Camera GetCamera()
		{
			return Object.FindObjectOfType<EditorLoadSaveUI>().ScreenshotCameraObject.GetComponentInChildren<Camera>();
		}

		public void TakeScreenshot(string filename)
		{
			CaptureScreenshot(filename);
		}

		public IEnumerator RenderFrameAndCaptureScreenshot(string filename)
		{
			CaptureScreenshot(filename);
			yield return 0;
		}

		public void CaptureScreenshot(string filename)
		{
			Debug.Log("Trying to capture screenshot to parh: " + filename);
			RenderTexture renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
			GameObject screenshotCameraObject = Object.FindObjectOfType<EditorLoadSaveUI>().ScreenshotCameraObject;
			screenshotCameraObject.SetActive(true);
			Camera componentInChildren = screenshotCameraObject.GetComponentInChildren<Camera>();
			componentInChildren.targetTexture = renderTexture;
			Texture2D texture2D = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, true);
			componentInChildren.Render();
			RenderTexture.active = renderTexture;
			texture2D.ReadPixels(new Rect(0f, 0f, Screen.width, Screen.height), 0, 0);
			texture2D.Apply();
			Texture2D texture2D2 = ScaleTexture(texture2D, 640, 360);
			byte[] bytes = texture2D2.EncodeToPNG();
			File.WriteAllBytes(filename, bytes);
			componentInChildren.targetTexture = null;
			RenderTexture.active = null;
			screenshotCameraObject.SetActive(false);
			Object.Destroy(renderTexture);
		}

		private Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
		{
			Texture2D texture2D = new Texture2D(targetWidth, targetHeight, source.format, true);
			Color[] pixels = texture2D.GetPixels(0);
			float num = 1f / (float)source.width * ((float)source.width / (float)targetWidth);
			float num2 = 1f / (float)source.height * ((float)source.height / (float)targetHeight);
			for (int i = 0; i < pixels.Length; i++)
			{
				pixels[i] = source.GetPixelBilinear(num * ((float)i % (float)targetWidth), num2 * Mathf.Floor(i / targetWidth));
			}
			texture2D.SetPixels(pixels, 0);
			texture2D.Apply();
			return texture2D;
		}
	}
}
