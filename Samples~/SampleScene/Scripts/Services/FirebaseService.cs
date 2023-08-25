using Firebase;
using Firebase.Extensions;
using Firebase.RemoteConfig;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class FirebaseService
{
	private const string Break = "~";

	// Key save data to player prefs
	private const string showAppOpenAdKey = "saoa";
	private const string limitedTimeAdsKey = "lta";

	// Name key from firebase
	private const string nameShowAppOpenAd = "show_app_open_ad";
	private const string nameLimitedTimeAds = "limited_time_ads";

	// Action
	public Action OnFetchSuccess;
	public Action<bool> OnShowAppOpenAdChange;
	public Action<int> OnLimitTimeAdsChanged;

	private FirebaseApp firebaseApp;

	// Cache
	private bool isShowAppOpenAd = false;
	private int lmtTimeAds = 70;
	public FirebaseService(Action onFetchSuccess)
	{
		OnFetchSuccess = onFetchSuccess;

		_ = InitFirebaseAsync();
	}
	private async Task InitFirebaseAsync()
	{
		await FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
		{
			var dependencyStatus = task.Result;
			if (dependencyStatus == DependencyStatus.Available)
			{
#if UNITY_EDITOR
				firebaseApp = FirebaseApp.Create();
#else
				firebaseApp = FirebaseApp.DefaultInstance;
#endif
				InitRemoteConfig();
			}
			else
			{
				Logger.Error(
				  "Could not resolve all Firebase dependencies: " + dependencyStatus);
			}
		});

	}
	public void InitRemoteConfig()
	{
		// Set default values
		Dictionary<string, object> defaults = new()
		{
			{ nameShowAppOpenAd, false },
			{ nameLimitedTimeAds, 70 }
		};

		if (Application.internetReachability == NetworkReachability.NotReachable)
		{
			GetData();
			OnFetchSuccess?.Invoke();
			Logger.Debug("System is not connected to internet");
			return;
		}

		FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(defaults).ContinueWithOnMainThread(task =>
		{
			_ = FetchDataAsync();
		});

	}
	private void GetData()
	{
		// AOA
		int tempAOA = PlayerPrefs.GetInt(showAppOpenAdKey, 0);
		isShowAppOpenAd = tempAOA != 0;
		OnShowAppOpenAdChange?.Invoke(isShowAppOpenAd);

		// limit time ad
		lmtTimeAds = PlayerPrefs.GetInt(limitedTimeAdsKey, 70);
		OnLimitTimeAdsChanged?.Invoke(lmtTimeAds);
	}
	public Task FetchDataAsync()
	{
		Logger.Debug("Fetching data...");
#if UNITY_DEBUG
		Task fetchTask = FirebaseRemoteConfig.DefaultInstance.FetchAsync(TimeSpan.Zero);
#else
		Task fetchTask = FirebaseRemoteConfig.DefaultInstance.FetchAsync(TimeSpan.FromHours(12.0));
#endif
		return fetchTask.ContinueWithOnMainThread(FetchComplete);
	}
	private void FetchComplete(Task fetchTask)
	{
		if (fetchTask.IsCanceled)
		{
			GetData();
			OnFetchSuccess?.Invoke();
			Logger.Debug("Fetching Remote Config values was cancelled.");
			return;
		}
		if (fetchTask.IsFaulted)
		{
			GetData();
			OnFetchSuccess?.Invoke();
			Logger.Debug("Fetching Remote Config values encountered an error: " + fetchTask.Exception);
			return;
		}
		if(fetchTask.IsCompletedSuccessfully == false)
		{
			GetData();
			OnFetchSuccess?.Invoke();
			Logger.Debug("Fetching Failed.");
			return;
		}

		var remoteConfig = FirebaseRemoteConfig.DefaultInstance;
		var info = remoteConfig.Info;
		if (info.LastFetchStatus != LastFetchStatus.Success)
		{
			GetData();
			OnFetchSuccess?.Invoke();
			Logger.Error($"{nameof(FetchComplete)} was unsuccessful\n{nameof(info.LastFetchStatus)}: {info.LastFetchStatus}");
			return;
		}

		remoteConfig.ActivateAsync()
		  .ContinueWithOnMainThread(
			task =>
			{
				Logger.Debug($"Remote data loaded and ready for use. Last fetch time {info.FetchTime}.");

				// limit time ad
				lmtTimeAds = (int)remoteConfig.GetValue(nameLimitedTimeAds).LongValue;
				PlayerPrefs.SetInt(limitedTimeAdsKey, lmtTimeAds);
				OnLimitTimeAdsChanged?.Invoke(lmtTimeAds);

				// AOA
				isShowAppOpenAd = remoteConfig.GetValue(nameShowAppOpenAd).BooleanValue;
				PlayerPrefs.SetInt(showAppOpenAdKey, isShowAppOpenAd == true ? 1 : 0);
				OnShowAppOpenAdChange?.Invoke(isShowAppOpenAd);

				OnFetchSuccess?.Invoke();
			});
	}
	private List<T> GetListValue<T>(string value)
	{
		if (value == string.Empty)
			return new List<T>();

		string[] list = value.Split(Break);
		List<T> result = new();

		foreach (string s in list)
		{
			T item = (T)Convert.ChangeType(s, typeof(T));
			result.Add(item);
		}
		return result;
	}
}

