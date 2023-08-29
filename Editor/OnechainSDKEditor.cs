using System.IO;
using UnityEditor;
using UnityEngine;

public class OnechainSDKWindow : EditorWindow
{
	private DefaultAsset packageAsset;

	[MenuItem("Window/OnechainSDK")]
	public static void ShowWindow()
	{
		GetWindow<OnechainSDKWindow>(nameof(OnechainSDKWindow));
	}

	private void OnGUI()
	{
		GUILayout.Label("Copy Samples Folder", EditorStyles.boldLabel);

		if (GUILayout.Button("Copy"))
		{
			CopySamplesFolder();
		}
		if (GUILayout.Button("Install"))
		{
			InstallPackage();
		}
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
	private void InstallPackage()
	{
		if (packageAsset != null)
		{
			//string packagePath = AssetDatabase.GetAssetPath(packageAsset);
			//AssetDatabase.ImportPackage(packagePath, true);
			//Debug.Log("Package installed successfully!");
		}
		else
		{
			Debug.LogWarning("Please select a package asset before installing.");
		}
	}
}
