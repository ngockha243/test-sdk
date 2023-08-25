using Services;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainSceneController : MonoBehaviour
{
	// Service
	private GameServices gameServices;
	private PlayerService playerService;
	private AdsService adsService;
	private DisplayService displayService;
	private GameService gameService;
	private IAPService iapService;
	private AudioService audioService;
	private void Start()
	{
		GameObject gameServiceObject = GameObject.FindGameObjectWithTag("Services");
		gameServices = gameServiceObject.GetComponent<GameServices>();
		playerService = gameServices.GetService<PlayerService>();
		adsService = gameServices.GetService<AdsService>();
		displayService = gameServices.GetService<DisplayService>();
		gameService = gameServices.GetService<GameService>();
		iapService = gameServices.GetService<IAPService>();
		audioService = gameServices.GetService<AudioService>();

	}
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.K))
		{
			iapService.PurchaseRemoveAds((state) => { });
		}
	}
}
