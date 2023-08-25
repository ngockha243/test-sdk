using AppsFlyerSDK;
using Firebase.Analytics;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using static MaxSdkBase;

namespace Services
{
	public class AdsService
	{
		// Const
		private const string showfirsttimeKey = "sft";
		private const string removeAdsKey = "rma";

		// Action
		public Action<bool> OnRewardedAdsLoad;
		public Action OnInterstitialClose;
		public Action OnAppOpenAdClose;

		// Action for Rewarded ADs
		private Action onRewardedComplete;
		private Action onRewardedFailed;

		// MaxSDK key
		private string MaxSdkKey = string.Empty;
		// ID ads
		private string interstitialAdUnitId = string.Empty;
		private string bannerAdUnitId = string.Empty;
		private string rewardedAdUnitId = string.Empty;
		private string MRECAdUnitId = string.Empty;
		private string appOpenAdUnitId = string.Empty;

		private AppOpenAd appOpenAd;
		private DateTime appOpenAdExpireTime;

		// Retry Attempt Ads
		private int retryAttemptInters;
		private int retryAttemptReward;
		private int retryAttemptBanner;
		private int retryAttemptMREC;

		// Flag show Ads
		private bool isShowInters = false;
		private bool isShowRewarded = false;
		private bool isShowAppOpenAd = true;

		// Limit time to show Ads
		private int limitTimeShowAds = 70;
		private DateTime timeToShowAds;

		private bool isFirstTime = true;
		private bool isRemoveAds = false;
		public void SetRemoveAds(bool removeAds)
		{
			isRemoveAds = removeAds;
		}
		public AdsService(GameServices gameServices, string maxSdkKey, string interstitalId, string bannerId, string rewardedId, string mrecId, string aOAId)
		{
			isRemoveAds = PlayerPrefs.HasKey(removeAdsKey);

			this.MaxSdkKey = maxSdkKey;
			this.interstitialAdUnitId = interstitalId;
			this.bannerAdUnitId = bannerId;
			this.rewardedAdUnitId = rewardedId;
			this.MRECAdUnitId = mrecId;
			this.appOpenAdUnitId = aOAId;

			timeToShowAds = DateTime.Now;

			MaxSdk.SetHasUserConsent(true);
			//MobileAds.SetiOSAppPauseOnBackground(true);
			MobileAds.RaiseAdEventsOnUnityMainThread = true;

#if UNITY_ANDROID && UNITY_DEBUG
			MaxSdk.SetCreativeDebuggerEnabled(true);
			appOpenAdUnitId = "ca-app-pub-3940256099942544/3419835294";
#else
			MaxSdk.SetCreativeDebuggerEnabled(false);
#endif
			MaxSdkCallbacks.OnSdkInitializedEvent += OnSdkInitialized =>
			{
#if UNITY_DEBUG
				MaxSdk.ShowMediationDebugger();
#endif
			};
			MaxSdk.SetSdkKey(MaxSdkKey);
			MaxSdk.InitializeSdk();
			#region Load Ads
			// Load Ad
			LoadInterstitial();
			LoadBanner();
			LoadRewardedAd();
			LoadMRECAd();
			#endregion
			#region Add event Ads
			// Interstitial
			MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoaded;
			MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnInterstitialDisplayed;
			MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialLoadFailed;
			MaxSdkCallbacks.Interstitial.OnAdClickedEvent += OnInterstitialClicked;
			MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnInterstitialDisplayFailed;
			MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialHidden;
			MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnInterstitialRevenuePaid;

			// Rewarded
			MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoaded;
			MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayed;
			MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceived;
			MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedAdClicked;
			MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdLoadFailed;
			MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdHidden;
			MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnRewardedRevenuePaid;
			MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedDisplayFailed;

			// Banner
			MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnBannerLoaded;
			MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnBannerLoadFailed;
			MaxSdkCallbacks.Banner.OnAdClickedEvent += OnBannerClicked;
			MaxSdkCallbacks.Banner.OnAdCollapsedEvent += OnBannerCollapsed;
			MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnBannerRevenuePaid;

			// MRECs
			MaxSdkCallbacks.MRec.OnAdLoadedEvent += OnMRECLoaded;
			MaxSdkCallbacks.MRec.OnAdLoadFailedEvent += OnMRECLoadFailed;
			MaxSdkCallbacks.MRec.OnAdExpandedEvent += OnMRECExpanded;
			MaxSdkCallbacks.MRec.OnAdCollapsedEvent += OnMRECCollapsed;
			#endregion
			#region Initialize Mobile Ads
			MobileAds.RaiseAdEventsOnUnityMainThread = true;
			RequestConfiguration requestConfiguration = new()
			{
#if !UNITY_EDITOR && UNITY_ANDROID
				TestDeviceIds = new List<string>() { GetDeviceID() }
#endif
			};

			MobileAds.SetRequestConfiguration(requestConfiguration);
			MobileAds.Initialize((InitializationStatus initStatus) =>
			{
				if (initStatus == null)
				{
					Logger.Debug("Google Mobile Ads initialization failed.");
					return;
				}

				LoadAppOpenAd();

				EnableAppOpenAdStateChange();
				gameServices.OnDestroyAction += () =>
				{
					DisableAppOpenAdStateChange();
				};
			});
			#endregion
		}
		/// <summary>
		/// Return true if interstital ads is ready
		/// </summary>
		/// <returns></returns>
		public bool IsInterstitialReady() => MaxSdk.IsInterstitialReady(interstitialAdUnitId);
		/// <summary>
		/// Show interstial Ads
		/// </summary>
		public void ShowLimitInterstitialAd()
		{
			if (MaxSdk.IsInterstitialReady(interstitialAdUnitId) && timeToShowAds < DateTime.Now)
			{
				isShowInters = true;
				MaxSdk.ShowInterstitial(interstitialAdUnitId);

				// Log event Appsflyer
				Dictionary<string, string> eventData = new()
				{
					{ "key1", "value1" },
					{ "key2", "value2" }
				};
				AppsFlyer.sendEvent("af_inter_logicgame", eventData);
			}
			else
			{
				Logger.Debug("Not ready for show ads");
			}
		}
		/// <summary>
		/// Show banner ads
		/// </summary>
		public void ShowBannerAds()
		{
			MaxSdk.CreateBanner(bannerAdUnitId, MaxSdkBase.BannerPosition.BottomCenter);
			MaxSdk.ShowBanner(bannerAdUnitId);
			MaxSdk.StartBannerAutoRefresh(bannerAdUnitId);
			LoadBanner();
		}
		/// <summary>
		/// Hide banner ads
		/// </summary>
		public void HideBannerAds()
		{
			MaxSdk.StopBannerAutoRefresh(bannerAdUnitId);
			MaxSdk.DestroyBanner(bannerAdUnitId);
		}
		/// <summary>
		/// Get height of banner
		/// </summary>
		/// <returns></returns>
		public float GetHeightBanner()
		{
#if UNITY_EDITOR
			return 168.0f;
#elif UNITY_IOS
			var height = MaxSdkUtils.GetAdaptiveBannerHeight();
			var density = MaxSdkUtils.GetScreenDensity();
			return (height + 21.0f) * density;
#else
			var height = MaxSdkUtils.GetAdaptiveBannerHeight();
			var density = MaxSdkUtils.GetScreenDensity();
			return height * density;
#endif
		}
		/// <summary>
		/// Add event when rewarded ads complete and failed
		/// </summary>
		/// <param name="onRewardedComplete"></param>
		/// <param name="onRewardedFailed"></param>
		public void InitRewardedAd(Action onRewardedComplete, Action onRewardedFailed)
		{
			this.onRewardedComplete = onRewardedComplete;
			this.onRewardedFailed = onRewardedFailed;
		}
		public bool IsRewardedReady() => MaxSdk.IsRewardedAdReady(rewardedAdUnitId);
		/// <summary>
		/// Show rewarded ads
		/// </summary>
		public void ShowRewardedAd()
		{
			if (MaxSdk.IsRewardedAdReady(rewardedAdUnitId))
			{
				isShowRewarded = true;
				MaxSdk.ShowRewardedAd(rewardedAdUnitId);

				// Log event Appsflyer
				Dictionary<string, string> eventData = new()
				{
					{ "key1", "value1" },
					{ "key2", "value2" }
				};
				AppsFlyer.sendEvent("af_rewarded_logicgame", eventData);
			}
		}
		/// <summary>
		/// Create mrec with default position Top Center / custom position
		/// MRECs are sized to 300x250 on phones and tablets
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void CreateMREC()
		{
			MaxSdk.CreateMRec(MRECAdUnitId, MaxSdkBase.AdViewPosition.TopCenter);
			LoadMRECAd();
		}
		public void CreateMREC(MaxSdkBase.AdViewPosition adViewPosition)
		{
			MaxSdk.CreateMRec(MRECAdUnitId, adViewPosition);
			LoadMRECAd();
		}
		public void CreateMREC(float x, float y)
		{
			MaxSdk.CreateMRec(MRECAdUnitId, x, y);
			LoadMRECAd();
		}
		public void UpdateMRECPosition(float x, float y)
		{
			MaxSdk.UpdateMRecPosition(MRECAdUnitId, x, y);
		}
		public void UpdateMRECPosition(MaxSdkBase.AdViewPosition adViewPosition)
		{
			MaxSdk.UpdateMRecPosition(MRECAdUnitId, adViewPosition);
		}
		/// <summary>
		/// Show Mrec ads
		/// </summary>
		public void ShowMRECAd()
		{
			MaxSdk.ShowMRec(MRECAdUnitId);
		}
		public void HideMRECAds()
		{
			MaxSdk.HideMRec(MRECAdUnitId);
		}
		public Rect GetMRECLayout()
		{
			try
			{
				return MaxSdk.GetMRecLayout(MRECAdUnitId);
			}
			catch
			{
				return new Rect();
			}
		}
		public void SetLimitTimeShowAds(int time)
		{
			limitTimeShowAds = time;
			Logger.Debug($"NNK: {time}");
		}
		#region Load ads
		private void LoadInterstitial()
		{
			MaxSdk.LoadInterstitial(interstitialAdUnitId);
		}
		private void LoadBanner()
		{
			MaxSdk.LoadBanner(bannerAdUnitId);
		}
		private void LoadRewardedAd()
		{
			MaxSdk.LoadRewardedAd(rewardedAdUnitId);
		}
		private void LoadMRECAd()
		{
			MaxSdk.LoadMRec(MRECAdUnitId);
		}
		#endregion
		#region Interstial Ads
		private void OnInterstitialLoaded(string adUnitId, MaxSdkBase.AdInfo adInfo)
		{
			if (adUnitId.Equals(interstitialAdUnitId))
			{
				retryAttemptInters = 0;

				// Log event Firebase and Appsflyer
				Dictionary<string, string> eventData = new()
				{
					{ "key1", "value1" },
					{ "key2", "value2" }
				};
				AppsFlyer.sendEvent("af_inters_successfullyloaded", eventData);
				FirebaseAnalytics.LogEvent("ad_inter_load");

			}
		}
		private void OnInterstitialDisplayed(string adUnitId, MaxSdkBase.AdInfo adInfo)
		{
			if (adUnitId.Equals(interstitialAdUnitId))
			{
				isShowInters = true;

				// Log event Firebase and Appsflyer
				Dictionary<string, string> eventData = new()
				{
					{ "key1", "value1" },
					{ "key2", "value2" }
				};
				AppsFlyer.sendEvent("af_inters_displayed", eventData);
				FirebaseAnalytics.LogEvent("ad_inter_show");
			}
		}
		private void OnInterstitialDisplayFailed(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
		{
			LoadInterstitial();
			OnInterstitialClose?.Invoke();
			OnInterstitialClose = null;
		}
		private void OnInterstitialLoadFailed(string adUnitId, MaxSdk.ErrorInfo errorInfo)
		{
			if (adUnitId.Equals(interstitialAdUnitId))
			{
				retryAttemptInters++;
				var retryDelay = Math.Pow(2.0, (double)Math.Min(6, retryAttemptInters));
				Observable.Timer(TimeSpan.FromSeconds(retryDelay)).Subscribe(_ => { LoadInterstitial(); });

				// Log event Firebase
				FirebaseAnalytics.LogEvent("ad_inter_fail", "errormsg", "Error Message: FailToLoad, Unavailable");
			}
		}
		private void OnInterstitialClicked(string adUnitId, MaxSdk.AdInfo adInfo)
		{
			if (adUnitId.Equals(interstitialAdUnitId))
			{
				FirebaseAnalytics.LogEvent("ad_inter_click");
			}
		}
		private void OnInterstitialHidden(string adUnitId, MaxSdkBase.AdInfo adInfo)
		{
			LoadInterstitial();
			timeToShowAds = DateTime.Now.AddSeconds(limitTimeShowAds);
			isShowInters = false;
			OnInterstitialClose?.Invoke();
			OnInterstitialClose = null;
		}
		private void OnInterstitialRevenuePaid(string adUnitId, MaxSdkBase.AdInfo impressionData)
		{
			OnAdRevenuePaidEvent(impressionData);
		}
		#endregion
		#region Banner Ads
		//Banner
		private void OnBannerLoaded(string adUnitId, MaxSdkBase.AdInfo adInfo)
		{
			if (adUnitId.Equals(bannerAdUnitId))
			{
				retryAttemptBanner = 0;
			}
		}
		private void OnBannerLoadFailed(string adUnitId, MaxSdk.ErrorInfo errorInfo)
		{
			if (adUnitId.Equals(bannerAdUnitId))
			{
				retryAttemptBanner++;
				var retryDelay = Math.Pow(2.0, (double)Math.Min(6, retryAttemptBanner));
				Observable.Timer(TimeSpan.FromSeconds(retryDelay)).Subscribe(_ => { LoadBanner(); });
			}
		}
		private void OnBannerClicked(string adUnitId, MaxSdk.AdInfo adInfo)
		{
			if (adUnitId.Equals(bannerAdUnitId))
			{

			}
		}
		private void OnBannerCollapsed(string adUnitId, MaxSdk.AdInfo adInfo)
		{
			if (adUnitId.Equals(bannerAdUnitId))
			{

			}
		}
		private void OnBannerRevenuePaid(string adUnitId, MaxSdkBase.AdInfo impressionData)
		{
			OnAdRevenuePaidEvent(impressionData);
		}
		#endregion
		#region Rewarded Ads
		// Rewarded
		private void OnRewardedAdLoaded(string adUnitId, MaxSdkBase.AdInfo adInfo)
		{
			if (adUnitId.Equals(rewardedAdUnitId))
			{
				OnRewardedAdsLoad?.Invoke(true);
				OnRewardedAdsLoad?.Invoke(IsRewardedReady());
				retryAttemptReward = 0;

				// Log event Firebase and Appsflyer
				Dictionary<string, string> eventData = new()
				{
					{ "key1", "value1" },
					{ "key2", "value2" }
				};
				AppsFlyer.sendEvent("af_rewarded_successfullyloaded", eventData);
				FirebaseAnalytics.LogEvent("ads_reward_offer", "placement", null);
			}
		}
		private void OnRewardedAdDisplayed(string adUnitId, MaxSdkBase.AdInfo adInfo)
		{
			if (adUnitId.Equals(rewardedAdUnitId))
			{
				OnRewardedAdsLoad?.Invoke(IsRewardedReady());
				LoadRewardedAd();
				isShowRewarded = true;

				// Log event Firebase and Appsflyer
				Dictionary<string, string> eventData = new()
				{
					{ "key1", "value1" },
					{ "key2", "value2" }
				};
				AppsFlyer.sendEvent("af_rewarded_displayed", eventData);
				FirebaseAnalytics.LogEvent("ads_reward_show", "placement", null);

			}
		}
		private void OnRewardedDisplayFailed(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
		{
			OnRewardedAdsLoad?.Invoke(IsRewardedReady());
			LoadRewardedAd();
			onRewardedFailed?.Invoke();
		}
		private void OnRewardedAdReceived(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo)
		{
			if (adUnitId.Equals(rewardedAdUnitId))
			{
				OnRewardedAdsLoad?.Invoke(IsRewardedReady());
				LoadRewardedAd();
				onRewardedComplete?.Invoke();

				Logger.Debug("Received from rewarded ad.");
				FirebaseAnalytics.LogEvent("ads_reward_complete", "placement", null);
			}

			Logger.Debug("NNK: receive");
		}
		private void OnRewardedAdClicked(string adUnitId, MaxSdkBase.AdInfo adInfo)
		{
			if (adUnitId.Equals(rewardedAdUnitId))
			{
				OnRewardedAdsLoad?.Invoke(IsRewardedReady());
				FirebaseAnalytics.LogEvent("ads_reward_click", "placement", null);
			}
		}
		private void OnRewardedAdLoadFailed(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
		{
			if (adUnitId.Equals(rewardedAdUnitId))
			{
				OnRewardedAdsLoad?.Invoke(false);
				OnRewardedAdsLoad?.Invoke(IsRewardedReady());

				retryAttemptReward++;
				var retryDelay = Math.Pow(2.0, (double)Math.Min(6, retryAttemptReward));
				Observable.Timer(TimeSpan.FromSeconds(retryDelay)).Subscribe(_ => { LoadRewardedAd(); });

				FirebaseAnalytics.LogEvent("ads_reward_fail", "placement", null);

			}
		}
		private void OnRewardedAdHidden(string adUnitId, MaxSdkBase.AdInfo adInfo)
		{
			OnRewardedAdsLoad?.Invoke(IsRewardedReady());
			LoadRewardedAd();
			isShowRewarded = false;
		}
		private void OnRewardedRevenuePaid(string adUnitId, MaxSdkBase.AdInfo adInfo)
		{
			LoadRewardedAd();
			OnAdRevenuePaidEvent(adInfo);
		}
		#endregion
		#region MRECs
		private void OnMRECLoaded(string adUnitId, MaxSdkBase.AdInfo adInfo)
		{
			if (adUnitId.Equals(MRECAdUnitId))
			{
				retryAttemptMREC = 0;
			}
		}
		private void OnMRECLoadFailed(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
		{
			if (adUnitId.Equals(MRECAdUnitId))
			{
				retryAttemptMREC++;
				var retryDelay = Math.Pow(2.0, (double)Math.Min(6, retryAttemptMREC));
				Observable.Timer(TimeSpan.FromSeconds(retryDelay)).Subscribe(_ => { LoadMRECAd(); });
			}
		}
		private void OnMRECExpanded(string adUnitId, MaxSdkBase.AdInfo adInfo)
		{
			if (adUnitId.Equals(MRECAdUnitId))
			{
				LoadMRECAd();
			}
		}
		private void OnMRECCollapsed(string adUnitId, MaxSdkBase.AdInfo adInfo)
		{
			if (adUnitId.Equals(MRECAdUnitId))
			{
				LoadMRECAd();
			}
		}
		#endregion
		#region App Open Ads

		// OpenAppAd
		public void OnShowAppOpenAdChange(bool state)
		{
			isShowAppOpenAd = state;
		}
		public bool IsShowFirstTime()
		{
			int first = PlayerPrefs.GetInt(showfirsttimeKey, 0);
			if (first == 0)
			{
				PlayerPrefs.SetInt(showfirsttimeKey, 1);
				return true;
			}
			else
			{
				return false;
			}
		}
		public void LoadAppOpenAd()
		{
			if (appOpenAd != null)
			{
				appOpenAd.Destroy();
				appOpenAd = null;
			}

			Logger.Debug("Loading the app open ad.");

			var adRequest = new AdRequest();

			AppOpenAd.Load(appOpenAdUnitId, ScreenOrientation.Portrait, adRequest,
			(AppOpenAd ad, LoadAdError error) =>
			{
				if (error != null || ad == null)
				{
					Logger.Error($"App open ad failed to load an ad with error : {error}");
					OnAppOpenAdClose?.Invoke();
					return;
				}

				Logger.Debug("App open ad loaded with response : " + ad.GetResponseInfo());

				appOpenAdExpireTime = DateTime.Now + TimeSpan.FromHours(4);

				appOpenAd = ad;
				RegisterEventHandlers(ad);
				if (isFirstTime == true)
				{
					isFirstTime = false;
					if (IsShowFirstTime() == false && isRemoveAds == false)
					{
						appOpenAd.Show();
					}
				}
			});
		}
#if !UNITY_EDITOR && UNITY_ANDROID
	private string GetDeviceID()
	{
		// Get Android ID
		AndroidJavaClass clsUnity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject objActivity = clsUnity.GetStatic<AndroidJavaObject>("currentActivity");
		AndroidJavaObject objResolver = objActivity.Call<AndroidJavaObject>("getContentResolver");
		AndroidJavaClass clsSecure = new AndroidJavaClass("android.provider.Settings$Secure");

		string android_id = clsSecure.CallStatic<string>("getString", objResolver, "android_id");

		// Get bytes of Android ID
		System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
		byte[] bytes = ue.GetBytes(android_id);

		// Encrypt bytes with md5
		System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
		byte[] hashBytes = md5.ComputeHash(bytes);

		// Convert the encrypted bytes back to a string (base 16)
		string hashString = "";

		for (int i = 0; i < hashBytes.Length; i++)
		{
			hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
		}

		string device_id = hashString.PadLeft(32, '0');
		return device_id;
	}
#endif
		private void RegisterEventHandlers(AppOpenAd ad)
		{
			// Raised when the ad is estimated to have earned money.
			ad.OnAdPaid += (AdValue adValue) =>
			{
				Logger.Debug(String.Format("App open ad paid {0} {1}.",
					adValue.Value,
					adValue.CurrencyCode));
			};
			// Raised when an impression is recorded for an ad.
			ad.OnAdImpressionRecorded += () =>
			{
				Logger.Debug("App open ad recorded an impression.");
			};
			// Raised when a click is recorded for an ad.
			ad.OnAdClicked += () =>
			{
				Logger.Debug("App open ad was clicked.");
				OnAppOpenAdClose?.Invoke();
			};
			// Raised when an ad opened full screen content.
			ad.OnAdFullScreenContentOpened += () =>
			{
				Time.timeScale = 0.0f;
				Logger.Debug("App open ad full screen content opened.");
			};
			// Raised when the ad closed full screen content.
			ad.OnAdFullScreenContentClosed += () =>
			{
				Time.timeScale = 1.0f;
				Logger.Debug("App open ad full screen content closed.");
				OnAppOpenAdClose?.Invoke();
				LoadAppOpenAd();
			};
			// Raised when the ad failed to open full screen content.
			ad.OnAdFullScreenContentFailed += (AdError error) =>
			{
				Logger.Error("App open ad failed to open full screen content " +
							   "with error : " + error);
				OnAppOpenAdClose?.Invoke();
				LoadAppOpenAd();
			};
		}
		private void ShowAppOpenAd()
		{
			if (appOpenAd != null && appOpenAd.CanShowAd() && IsShowFirstTime() == false && isRemoveAds == false)
			{
				appOpenAd.Show();
			}
			else
			{
				Logger.Error("App open ad is not ready yet.");
				OnAppOpenAdClose?.Invoke();
			}
		}

		private bool IsAppOpenAdAvailable
		{
			get
			{
				return appOpenAd != null
					   && appOpenAd.CanShowAd()
					   && DateTime.Now < appOpenAdExpireTime;
			}
		}

		private void OnAppStateChanged(AppState state)
		{
			if (isShowInters == true || isShowRewarded == true || isShowAppOpenAd == false)
			{
				return;
			}

			if (state == AppState.Foreground && isRemoveAds == false)
			{
				ShowAppOpenAd();
			}
		}

		public void EnableAppOpenAdStateChange()
		{
			AppStateEventNotifier.AppStateChanged += OnAppStateChanged;
		}

		public void DisableAppOpenAdStateChange()
		{
			AppStateEventNotifier.AppStateChanged -= OnAppStateChanged;
		}
		#endregion
		private void OnAdRevenuePaidEvent(MaxSdkBase.AdInfo impressionData)
		{
			double revenue = impressionData.Revenue;
			var impressionParameters = new[]
			{
				new Firebase.Analytics.Parameter("ad_platform", "AppLovin"),
				new Firebase.Analytics.Parameter("ad_source", impressionData.NetworkName),
				new Firebase.Analytics.Parameter("ad_unit_name", impressionData.AdUnitIdentifier),
				new Firebase.Analytics.Parameter("ad_format", impressionData.AdFormat),
				new Firebase.Analytics.Parameter("value", revenue),
				new Firebase.Analytics.Parameter("currency", "USD"), // All AppLovin revenue is sent in USD
		    };
			Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_impression", impressionParameters);
		}
	}
}
