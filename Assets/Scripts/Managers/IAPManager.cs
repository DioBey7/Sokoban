#pragma warning disable 0618

using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using System;

public class IAPManager : MonoBehaviour, IDetailedStoreListener
{
    public static IAPManager Instance { get; private set; }
    private IStoreController storeController;
    private IExtensionProvider storeExtensionProvider;

    public const string PRODUCT_100_GOLD = "com.teg.sokoban3d.100gold";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (storeController == null)
        {
            InitializePurchasing();
        }
    }

    public void InitializePurchasing()
    {
        if (IsInitialized()) return;

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        builder.AddProduct(PRODUCT_100_GOLD, UnityEngine.Purchasing.ProductType.Consumable);
        UnityPurchasing.Initialize(this, builder);
    }

    public bool IsInitialized()
    {
        return storeController != null && storeExtensionProvider != null;
    }

    public void Buy100Gold()
    {
        BuyProductID(PRODUCT_100_GOLD);
    }

    private void BuyProductID(string productId)
    {
        if (IsInitialized())
        {
            Product product = storeController.products.WithID(productId);
            if (product != null && product.availableToPurchase)
            {
                storeController.InitiatePurchase(product);
            }
        }
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        if (String.Equals(args.purchasedProduct.definition.id, PRODUCT_100_GOLD, StringComparison.Ordinal))
        {
            EconomyManager.Instance.AddGold(100);
        }
        return PurchaseProcessingResult.Complete;
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        storeController = controller;
        storeExtensionProvider = extensions;
    }

    public void OnInitializeFailed(InitializationFailureReason error) { }
    public void OnInitializeFailed(InitializationFailureReason error, string message) { }
    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason) { }
    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription) { }
}

#pragma warning restore 0618