using AppsFlyerSDK;

namespace Services
{
	public class AppsFlyerService
	{
		public AppsFlyerService(string devKey, string appId)
		{
			AppsFlyer.initSDK(devKey, appId);
#if UNITY_DEBUG
			AppsFlyer.setIsDebug(true);
#endif
			AppsFlyer.startSDK();
		}
	}
}
