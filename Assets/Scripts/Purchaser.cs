using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

[System.Serializable]
public class BookPurchase
{
    public BookID BookID;
    public string AndroidKey;
    public string IOSKey;
}


public class Purchaser : MonoBehaviour, IDetailedStoreListener
{
    public bool IsFakeShop = true;
    public bool AnyPurchase;

    private static IStoreController m_StoreController;         
    private static IExtensionProvider m_StoreExtensionProvider;

    [SerializeField]
    private BookPurchase[] _bookPurchases;

    private BookPurchase GetBookPurchase(BookID id) => _bookPurchases.FirstOrDefault(book => book.BookID == id);


    public event Action<BookPurchase> OnBookPurchased;

    public void Awake()
    {   
        if (m_StoreController == null)
        {
            InitializePurchasing();
        }
        AnyPurchase = PlayerPrefs.GetInt("AP") != 0;
    }

    private void InitializePurchasing()
    {
        if (IsInitialized())
        {
            return;
        }
        
        var module = StandardPurchasingModule.Instance();
        module.useFakeStoreAlways = false;
            
        module.useFakeStoreUIMode = FakeStoreUIMode.Default;
        var builder = ConfigurationBuilder.Instance(module);

        foreach (var book in _bookPurchases)
        {
            builder.AddProduct(book.AndroidKey, ProductType.NonConsumable, new IDs()
            {
                {book.AndroidKey, GooglePlay.Name},
                {book.IOSKey, MacAppStore.Name},
            });
        }
        UnityPurchasing.Initialize(this, builder);
    }


    private bool IsInitialized()
    {
        return m_StoreController != null && m_StoreExtensionProvider != null;
    }


    public void BuyBook(BookID id)
    {
        var bookData = GetBookPurchase(id);
        if (IsFakeShop)
        {
            OnBookPurchased?.Invoke(bookData);
        }
        else
        {
#if UNITY_ANDROID
            BuyProductID(bookData.AndroidKey);
#else
            BuyProductID(bookData.IOSKey);
#endif
        }
    }

    void BuyProductID(string productId)
    {
        if (IsInitialized())
        {
            Product product = m_StoreController.products.WithID(productId);

            if (product != null && product.availableToPurchase)
            {
                Debug.Log($"Purchasing product asychronously: '{product.definition.id}'");
                m_StoreController.InitiatePurchase(product);
            }
            else
            {
                Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
            }
        }
        else
        {
            Debug.Log("BuyProductID FAIL. Not initialized.");
        }
    }

    public void RestorePurchases()
    {
        if (!IsInitialized())
        {    
            Debug.Log("RestorePurchases FAIL. Not initialized.");
            return;
        }
        if (Application.platform == RuntimePlatform.IPhonePlayer ||
            Application.platform == RuntimePlatform.OSXPlayer)
        {
            Debug.Log("RestorePurchases started ...");
            var apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();
            apple.RestoreTransactions((result, _) => {
                Debug.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");
            });
        }

        else
        {

            Debug.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
        }
    }


    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("OnInitialized: PASS");
        m_StoreController = controller;
        m_StoreExtensionProvider = extensions;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
    {
        Debug.Log($"OnPurchaseFailed: {failureDescription.message}");
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.Log($"OnInitializeFailed InitializationFailureReason: {error}");
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.Log($"OnInitializeFailed InitializationFailureReason: {error} {message}");
    }


    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        foreach (var book in _bookPurchases)
        {
#if UNITY_ANDROID
            if (String.Equals(args.purchasedProduct.definition.id, book.AndroidKey, StringComparison.Ordinal))
            {
                OnBookPurchased?.Invoke(book);
                Debug.Log($"ProcessPurchase: PASS. Product: '{args.purchasedProduct.definition.id}'");
            }
            else
            {
                Debug.Log($"ProcessPurchase: FAIL. Unrecognized product: '{args.purchasedProduct.definition.id}'");
            }

#else
            if (String.Equals(args.purchasedProduct.definition.id, book.IOSKey, StringComparison.Ordinal))
            {
                OnBookPurchased?.Invoke(book);
                Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
            }
            else
            {
                Debug.Log(string.Format("ProcessPurchase: FAIL. Unrecognized product: '{0}'", args.purchasedProduct.definition.id));
            }
#endif
        }
        PlayerPrefs.SetInt("AP", 1);
        return PurchaseProcessingResult.Complete;
    }


    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.Log(
            $"OnPurchaseFailed: FAIL. Product: '{product.definition.storeSpecificId}', PurchaseFailureReason: {failureReason}");
    }
}
