using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class OnechainSDKWindow : EditorWindow
{
	private const string PackageDownloadedKey = "pcdl";
	private const float buttonWidth = 100;
	private const int paddingRectangle = 10;

	[MenuItem("Window/OnechainSDK")]
	public static void ShowWindow()
	{
		var window = GetWindow<OnechainSDKWindow>(nameof(OnechainSDKWindow));
		window.minSize = new Vector2(300, 100); // Set the minimum width and height
		window.maxSize = new Vector2(600, 600); // Set the maximum width and height
	}

	private void OnGUI()
	{
		GUILayout.Label("Advertisement", EditorStyles.boldLabel);

		// Define the rectangle style
		GUIStyle rectStyle = new(GUI.skin.box)
		{
			padding = new RectOffset(paddingRectangle, paddingRectangle, paddingRectangle, paddingRectangle)
		};
		EditorGUILayout.BeginVertical(rectStyle);

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Max SDK", GUILayout.Width(buttonWidth));
		GUILayout.FlexibleSpace();

		//bool packageDownloaded = EditorPrefs.GetBool(PackageDownloadedKey, false);
		//GUI.enabled = !packageDownloaded; // Disable the button if package has been downloaded
		if (GUILayout.Button("Download", GUILayout.Width(buttonWidth)))
		{
			InstallPackages();
			EditorPrefs.SetBool(PackageDownloadedKey, true);
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Mobile Ads", GUILayout.Width(buttonWidth));
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Download", GUILayout.Width(buttonWidth)))
		{
			InstallPackagesMobileAds();
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.EndVertical();

		GUILayout.Label("Base", EditorStyles.boldLabel);// Define the rectangle style
		GUIStyle rectStyle1 = new(GUI.skin.box)
		{
			padding = new RectOffset(paddingRectangle, paddingRectangle, paddingRectangle, paddingRectangle)
		};
		EditorGUILayout.BeginVertical(rectStyle1);

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Entry Scene", GUILayout.Width(buttonWidth));
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Import", GUILayout.Width(buttonWidth)))
		{
			//InstallPackagesMobileAds();
			TestPath();
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.EndVertical();
	}
	private void CopySamplesFolder()
	{

		string sourceFolderPath = "../Packages/OnechainSDK";
		string destinationFolderPath = "Assets/Sample1";
		string sourcePath = Application.dataPath + "/" + sourceFolderPath;
		string destinationPath = Application.dataPath + "/" + destinationFolderPath;

		//FileUtil.CopyFileOrDirectory(sourcePath, destinationPath);

		//AssetDatabase.Refresh();
		//Debug.Log("Files copied successfully!");
		Debug.Log(Application.dataPath+ "/..");
	}
	private void InstallPackages()
	{
		string tempFilePath = Path.Combine(Application.temporaryCachePath, "package.unitypackage");
		Debug.Log(tempFilePath);

		using (var www = new WWW("https://artifacts.applovin.com/unity/com/applovin/applovin-sdk/AppLovin-MAX-Unity-Plugin-5.11.3-Android-11.11.3-iOS-11.11.3.unitypackage"))
		{
			while (!www.isDone)
			{
				// Show progress if needed
			}

			if (!string.IsNullOrEmpty(www.error))
			{
				Debug.LogError($"Error downloading package: {www.error}");
				return;
			}

			// Save the downloaded package to the temp file
			File.WriteAllBytes(tempFilePath, www.bytes);
		}

		// Import the package
		AssetDatabase.ImportPackage(tempFilePath, true);
		Debug.Log("External package installed successfully!");
	}
	private void InstallPackagesMobileAds()
	{
		string tempFilePath = Path.Combine(Application.temporaryCachePath, "package.unitypackage");

		using (var www = new WWW("https://github.com/googleads/googleads-mobile-unity/releases/download/v8.5.2/GoogleMobileAds-v8.5.2.unitypackage"))
		{
			while (!www.isDone)
			{
				// Show progress if needed
			}

			if (!string.IsNullOrEmpty(www.error))
			{
				Debug.LogError($"Error downloading package: {www.error}");
				return;
			}

			// Save the downloaded package to the temp file
			File.WriteAllBytes(tempFilePath, www.bytes);
		}

		// Import the package
		AssetDatabase.ImportPackage(tempFilePath, true);
		Debug.Log("External package installed successfully!");
	}
	private void TestPath()
	{
		string samplesFolderPath = FindPackagePath();
		if (!string.IsNullOrEmpty(samplesFolderPath))
		{
			string fullPath = Path.Combine(Application.dataPath, samplesFolderPath);
			Debug.Log($"Full Samples Path: {fullPath}");
		}
		else
		{
			Debug.LogError("Samples folder not found.");
		}
	}
	private string FindPackagePath()
	{
		string[] packageGUIDs = AssetDatabase.FindAssets("OnechainSDK");
		if (packageGUIDs.Length > 0)
		{
			string packagePath = AssetDatabase.GUIDToAssetPath(packageGUIDs[0]);
			return Path.GetDirectoryName(packagePath);
		}
		return null;
	}
}
