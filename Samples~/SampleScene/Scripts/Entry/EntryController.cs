// Nguyen Ngoc Kha - Base - V 1.0.6
using System.Collections.Generic;
using UnityEngine;
using Services;
using UnityEngine.SceneManagement;
using Audio;

namespace Entry
{
	public class EntryController : MonoBehaviour
	{
		private const string soundObjectName = "Sound";
		private const string UnUsed = "unused";
		[SerializeField] private EntryModel model;

		[SerializeField] private List<Sound> sounds;
		[SerializeField] private Music music;
		[SerializeField] private GameObject musicObject;

		[Space(8.0f)]
		[SerializeField] private bool isFake = false;

		private GameServices gameServices = null;

		void Awake()
		{

			if (GameObject.FindGameObjectWithTag(Constants.ServicesTag) == null)
			{
                GameObject gameServiceObject = new(nameof(GameServices))
                {
                    tag = Constants.ServicesTag
                };
                gameServices = gameServiceObject.AddComponent<GameServices>();

				// Instantie Audio
				DontDestroyOnLoad(musicObject);

				GameObject soundObject = new(soundObjectName);
				DontDestroyOnLoad(soundObject);

				// Add Services
				gameServices.AddService(new AudioService(music, sounds, soundObject));
				gameServices.AddService(new DisplayService());
				gameServices.AddService(new InputService());
				gameServices.AddService(new PlayerService());
				gameServices.AddService(new GameService(model.TOSURL, model.PrivacyURL, model.RateURL));
				gameServices.AddService(new FirebaseService(OnFetchSuccess));
				gameServices.AddService(new IAPService(model.IAPRemoveAdsKey));

#if UNITY_ANDROID
				gameServices.AddService(new AdsService(
					gameServices,
					model.MaxSDKKey,
					model.IntersIdAndroid,
					model.BannerIdAndroid,
					model.RewardedIdAndroid,
					model.MrecIdAndroid,
					model.AOAIdAndroid));
				gameServices.AddService(new AppsFlyerService(
					model.AppsFlyerDevKey,
					model.AppFlyerAppIdAndroid));
#elif UNITY_IPHONE || UNITY_IOS
				gameServices.AddService(new AdsService(
					gameServices,
					model.MaxSDKKey,
					model.IntersIdIOS,
					model.BannerIdIOS,
					model.RewardedIdIOS,
					model.MrecIdIos,
					model.AOAIdIOS));
				gameServices.AddService(new AppsFlyerService(
					model.AppsFlyerDevKey,
					model.AppsFlyerAppIdIos));
#else
				gameServices.AddService(new AdsService(
					gameServices,
					model.MaxSDKKey,
					UnUsed,
					UnUsed,
					UnUsed,
					UnUsed,
					UnUsed));
				gameServices.AddService(new AppsFlyerService(
					model.AppsFlyerDevKey,
					UnUsed));
#endif

				// Get services
				var firebaseServices = gameServices.GetService<FirebaseService>();
				var adsServices = gameServices.GetService<AdsService>();
				var displayService = gameServices.GetService<DisplayService>();
				var audioService = gameServices.GetService<AudioService>();
				var playerService = gameServices.GetService<PlayerService>();
				var iapService = gameServices.GetService<IAPService>();

				// --------------------------- Ads ---------------------------------
				// AOA
				//adsServices.EnableAppOpenAdStateChange();
				//gameServices.OnDestroyAction += () =>
				//{
				//	adsServices.DisableAppOpenAdStateChange();
				//};
				iapService.OnRemoveAdsChanged = adsServices.SetRemoveAds;

				// Setting ads from firebase
				firebaseServices.OnLimitTimeAdsChanged = adsServices.SetLimitTimeShowAds;
				firebaseServices.OnShowAppOpenAdChange = adsServices.OnShowAppOpenAdChange;
				// ------------------------------------------------------------------

				// --------------------------- Display ---------------------------------
#if UNITY_EDITOR
				displayService.IsFake = isFake;
#endif
				// Change position Logo
				var safeArea = displayService.SafeArea();
				// ------------------------------------------------------------------

				// --------------------------- Audio ---------------------------------
				// Set Volume
				playerService.OnMusicVolumeChange = audioService.SetMusicVolume;
				playerService.OnSoundVolumeChange = audioService.SetSoundVolume;

				playerService.OnVibrateChange = audioService.SetVibrate;

				audioService.MusicVolume = playerService.GetMusicVolume();
				audioService.SoundVolume = playerService.GetSoundVolume();

				audioService.VibrateOn = playerService.GetVibrate();

				audioService.MusicOn = true;
				audioService.SoundOn = true;

				audioService.StopMusic();
				// ------------------------------------------------------------------
			}
		}
		private void OnFetchSuccess()
		{
			Time.timeScale = 1.0f;
			gameServices.GetService<AdsService>().LoadAppOpenAd();
			SceneManager.LoadScene(Constants.MainScene);
		}
		private void Start()
		{
			Time.timeScale = 1.0f;
			gameServices.GetService<AdsService>().LoadAppOpenAd();
			SceneManager.LoadScene(Constants.MainScene);
		}
	}
}
