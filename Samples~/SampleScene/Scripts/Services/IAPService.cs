using System;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.Purchasing;

public class IAPService : IStoreListener
{
    private const string removeAdsKey = "rma";
	private const string priceKey = "prc";
	private const string defaultPrice = "0.99$";

    public bool AdsRemoved { get => adsRemoved; }
    public Action OnIAPInitializedSuccess;
	public Action<bool> OnRemoveAdsChanged;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private string environment = "test";
#else
	public string environment = "production";
#endif
    private IStoreController storeController = null;
    private IExtensionProvider extensionsProvider = null;

    private string removeAdsProductID = string.Empty;
    private bool adsRemoved = false;

    private event Action<bool> OnPurchaseComplete;
    public IAPService(string removeKey)
    {
        this.removeAdsProductID = removeKey;
        if (storeController == null)
        {
            InitializeUnityServices();
#if UNITY_EDITOR || UNITY_DEBUG
            StandardPurchasingModule.Instance().useFakeStoreUIMode = FakeStoreUIMode.StandardUser;
            StandardPurchasingModule.Instance().useFakeStoreAlways = true;
#endif

#if UNITY_ANDROID
			ConfigurationBuilder builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance(AppStore.GooglePlay));
#elif UNITY_IOS
			ConfigurationBuilder builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance(AppStore.AppleAppStore));
#else
            ConfigurationBuilder builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance(AppStore.NotSpecified));
#endif

            builder.AddProduct(removeAdsProductID, ProductType.NonConsumable, new IDs
            {
                {removeAdsProductID, GooglePlay.Name},
                {removeAdsProductID, MacAppStore.Name}
            });

            UnityPurchasing.Initialize(this, builder);

            if (PlayerPrefs.HasKey(removeAdsKey))
            {
                adsRemoved = true;
				OnRemoveAdsChanged?.Invoke(true);

			}
        }
    }
    /// <summary>
    /// Return true if user had purchased remove ads
    /// </summary>
    /// <returns></returns>
    public bool IsRemoveAds()
	{
		OnRemoveAdsChanged?.Invoke(PlayerPrefs.HasKey(removeAdsKey));
		return PlayerPrefs.HasKey(removeAdsKey);
    }
    /// <summary>
    /// Purchase remove ads
    /// </summary>
    /// <param name="OnPurchaseCompleted"></param>
    public void PurchaseRemoveAds(Action<bool> OnPurchaseCompleted)
    {
        OnPurchaseComplete = OnPurchaseCompleted;
        if (storeController != null && extensionsProvider != null)
        {
            Product removeAdsProduct = storeController.products.WithID(removeAdsProductID);

            if (removeAdsProduct != null && removeAdsProduct.availableToPurchase)
            {
                storeController.InitiatePurchase(removeAdsProduct);
            }
            else
            {
                Logger.Debug("Remove ads product unavailable");
            }
        }
        else
        {
            Logger.Debug("Store controller not initialized");
        }
    }
    /// <summary>
    /// Restore transaction if user delete game (For IOS)
    /// </summary>
    /// <param name="OnPurchaseCompleted"></param>
    public void RestorePurchase(Action<bool> OnPurchaseCompleted)
    {
        if (extensionsProvider != null)
        {
            var apple = extensionsProvider.GetExtension<IAppleExtensions>();

            apple?.RestoreTransactions((result, message) =>
                {
                    if (result)
                    {
                        Product removeAdsProduct = storeController.products.WithID(removeAdsProductID);

                        if (removeAdsProduct != null && removeAdsProduct.hasReceipt)
						{
							OnRemoveAdsChanged?.Invoke(true);
							PlayerPrefs.SetString(removeAdsKey, "purchased");
                            adsRemoved = true;
                            OnPurchaseCompleted?.Invoke(true);
                        }
                        else
                        {
                            OnPurchaseCompleted?.Invoke(false);
                        }
                    }
                    else
                    {
                        Logger.Debug("Transaction restore failed");
                        OnPurchaseCompleted?.Invoke(false);
                    }
                });
        }
        else
        {
            Logger.Debug("IAP service is not initialized.");
        }
    }
    /// <summary>
    /// Get price of remove ads
    /// </summary>
    /// <returns></returns>
    public string GetPrice()
    {
		if(storeController != null)
		{
			var product = storeController.products.WithID(removeAdsProductID);
			if (product != null)
			{
				string price = product.metadata.localizedPriceString;
				PlayerPrefs.SetString(priceKey, price);
				return price;
			}
		}
        return PlayerPrefs.GetString(priceKey, defaultPrice);
    }
    #region IAP
    private async void InitializeUnityServices()
    {
        try
        {
            InitializationOptions options = new InitializationOptions()
                .SetEnvironmentName(environment);

            await UnityServices.InitializeAsync(options);
            Logger.Debug("Unity Services initialized successfully.");
        }
        catch (Exception exception)
        {
            Logger.Debug(exception.InnerException.Message);
        }
    }
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        storeController = controller;
        extensionsProvider = extensions;

        if (PlayerPrefs.HasKey(removeAdsKey))
		{
			OnRemoveAdsChanged?.Invoke(true);
			adsRemoved = true;
        }
        else
		{
			OnRemoveAdsChanged?.Invoke(false);
			adsRemoved = false;
        }

        extensions.GetExtension<IAppleExtensions>().RestoreTransactions((result, message) => {
            if (result)
            {
                // This does not mean anything was restored,
                // merely that the restoration process succeeded.
                Logger.Debug(message);

                // Get the purchases after restoring transactions
                Product products = storeController.products.WithID(removeAdsProductID);

                if (products != null)
                {
                    OnIAPInitializedSuccess?.Invoke();
                    //adsRemoved = true;
                }
            }
            else
            {
                // Restoration failed.
                Logger.Debug(message);
            }
        });
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Logger.Error($"Error initializing IAP because of {error}.");
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
    {
        if (String.Equals(e.purchasedProduct.definition.id, removeAdsProductID, StringComparison.Ordinal))
        {
            Logger.Debug("Ads removed successfully.");
            // Remove the ads here.

            OnPurchaseComplete?.Invoke(true);
            OnPurchaseComplete = null;

			OnRemoveAdsChanged?.Invoke(true);
			PlayerPrefs.SetString(removeAdsKey, "purchased");
            adsRemoved = true;

            var product = storeController.products.WithID(removeAdsProductID);
            //var parameters = new[]
            //{
            //    new Firebase.Analytics.Parameter(FirebaseAnalytics.ParameterItemId, product.definition.id),
            //    new Firebase.Analytics.Parameter(FirebaseAnalytics.ParameterTransactionId, product.transactionID),
            //    new Firebase.Analytics.Parameter(FirebaseAnalytics.ParameterValue, (float)product.metadata.localizedPrice),
            //    new Firebase.Analytics.Parameter(FirebaseAnalytics.ParameterCurrency, product.metadata.isoCurrencyCode),
            //};
            //Firebase.Analytics.FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventPurchase, parameters);
        }
        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product i, PurchaseFailureReason p)
    {
        Logger.Debug("IAP Manager purchase failed: " + p);

        OnPurchaseComplete?.Invoke(false);
        OnPurchaseComplete = null;
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Logger.Debug("IAP Manager initialization failed: " + error + " - " + message);
        throw new System.NotImplementedException();
    }
    #endregion
}
