#if BICUTIL_IAP
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;
using BicUtil.SingletonBase;
using BicDB.Container;
using BicDB.Storage;
using BicDB.Variable;
using BicDB;
using BicDB.Core;

namespace BicUtil.Purchasing
{
    public static class PurchasingService
    {
        public static Func<string, ProductModelBase> GetProduct = null;
        public static Action<string, Action<PurchasingResult>> BuyProduct = null;
        public static Action<Action<bool, string>> RestorePurchases = null;

        public static string File = "Puma";

        public static string ErrorMessage = "";
        public static void AddError(string _error){
            ErrorMessage += "\n" + _error;
        }
    }

    /// <summary>
    /// Unity IAP 5.4.1 implementation.
    /// Public wrapper API is intentionally kept compatible with the previous IAP 4.x implementation.
    /// </summary>
    public class PurchasingManager<PRODUCTTYPE> : SingletonBase<PurchasingManager<PRODUCTTYPE>>, IPurchasingManager<PRODUCTTYPE>
        where PRODUCTTYPE : struct, Enum
    {
        public TableContainer<ProductModel<PRODUCTTYPE>> productTable =
            new TableContainer<ProductModel<PRODUCTTYPE>>(PurchasingService.File);

        public SubscriptionInfo SubscriptionInfo = null;

        private readonly EnumVariable<SubscriptionStateType> subscriptionState =
            new EnumVariable<SubscriptionStateType>(SubscriptionStateType.Inactive);

        public EnumVariable<SubscriptionStateType> SubscriptionState => subscriptionState;
        public List<PRODUCTTYPE> SubscriptionActiveIDs { get; set; } = new List<PRODUCTTYPE>();
        public List<PRODUCTTYPE> SubscriptionPerhapsIDs { get; set; } = new List<PRODUCTTYPE>();

        private bool isLoad;
        private bool isConnected;
        private bool isProductsFetched;
        private bool eventsRegistered;
        private bool initializationStarted;

        private StoreController storeController;
        private Action<PurchasingResult> buyCallback;
        private string purchasingProductId;

        private byte[] googleTangle;
        private byte[] appleTangle;

        public void AddProduct(
            PRODUCTTYPE _idType,
            string _id,
            ProductType _productType,
            int _amount,
            string _defaultCurrentCode,
            string _defaultPriceString,
            float _defaultPrice,
            string _title,
            Action<IVariableReadOnly> _valueChangedCallback)
        {
            if (!isLoad){
                PurchasingService.AddError("table not load");
                throw new SystemException("Not Load PurchasingManager");
            }

            if (googleTangle == null && appleTangle == null){
                PurchasingService.AddError("Tangle is null");
                throw new Exception("SETUP TANGLES");
            }

            var product = GetProduct(_idType);
            if (product == null)
            {
                product = new ProductModel<PRODUCTTYPE>(_idType, _id, _productType, _amount);
                productTable.Add(product);
            }

            product.Price.AsFloat = _defaultPrice;
            product.PriceString.AsString = _defaultPriceString;
            product.CurrencyCode.AsString = _defaultCurrentCode;
            product.Amount.AsInt = _amount;
            product.Title.AsString = _title;
            product.ProductType.AsEnum = _productType;
            product.Id.AsString = _id;

            if (_valueChangedCallback != null)
                product.PurchaseCount.Subscribe(_valueChangedCallback, true);
        }

        public void SetupTangle(Func<byte[]> _googleTangle, Func<byte[]> _appleTangle)
        {
#if UNITY_EDITOR
            googleTangle = new byte[1];
            appleTangle = new byte[1];
            return;
#endif

#if UNITY_ANDROID
            googleTangle = _googleTangle?.Invoke();
#endif

#if UNITY_IOS || UNITY_STANDALONE_OSX
            appleTangle = _appleTangle?.Invoke();
#endif
        }

        public override void Initialize()
        {
            PurchasingService.GetProduct = GetProductBase;
            PurchasingService.BuyProduct = BuyProductByStringId;
            PurchasingService.RestorePurchases = RestorePurchases;

            if (productTable.Count == 0 || IsInitialized() || initializationStarted){
                PurchasingService.AddError("Initialize Error " + (productTable.Count == 0 ? "productCount = 0" : "") + (IsInitialized() == true ? "already init" : "") + (initializationStarted == true ? "initializationStarted true" : ""));
                return;
            }

            initializationStarted = true;
            InitializeIAPAsync();
        }

        private async void InitializeIAPAsync()
        {
            try
            {
                storeController = UnityIAPServices.StoreController();
                RegisterEvents();

                await storeController.Connect();

                var catalogProvider = new CatalogProvider();
                foreach (var product in productTable)
                {
                    catalogProvider.AddProduct(
                        product.Id.AsString,
                        product.ProductType.AsEnum,
                        ConvertStoreSpecificIds(product.StoreIds));
                }

                catalogProvider.FetchProducts(definitions => storeController.FetchProducts(definitions));
            }
            catch (Exception exception)
            {
                initializationStarted = false;
                isConnected = false;
                PurchasingService.AddError("InitializeIAPAsync error : " + exception.Message);
                Debug.LogError("IAP Init Exception " + exception);
                LogInitFailure("Exception", exception.Message);
            }
        }

        private void RegisterEvents()
        {
            if (eventsRegistered || storeController == null)
                return;

            storeController.OnStoreConnected += OnStoreConnected;
            storeController.OnStoreDisconnected += OnStoreDisconnected;
            storeController.OnProductsFetched += OnProductsFetched;
            storeController.OnProductsFetchFailed += OnProductsFetchFailed;
            storeController.OnPurchasesFetched += OnPurchasesFetched;
            storeController.OnPurchasesFetchFailed += OnPurchasesFetchFailed;
            storeController.OnPurchasePending += OnPurchasePending;
            storeController.OnPurchaseFailed += OnPurchaseFailed;
            storeController.OnPurchaseDeferred += OnPurchaseDeferred;
            eventsRegistered = true;
        }

        private void OnStoreConnected()
        {
            isConnected = true;
            Debug.Log("IAP Store connected.");
        }

        /// <summary>
        /// Converts the old wrapper's StoreIds object without forcing callers or ProductModel to change.
        /// Both the old IDs type and dictionaries enumerate KeyValuePair&lt;string,string&gt;.
        /// </summary>
        private static StoreSpecificIds ConvertStoreSpecificIds(object source)
        {
            var result = new StoreSpecificIds();
            if (source is IEnumerable<KeyValuePair<string, string>> pairs)
            {
                foreach (var pair in pairs)
                    result.Add(pair.Value, pair.Key);
            }
            return result;
        }

        public void BuyProductByStringId(string _productId, Action<PurchasingResult> _callback)
        {
            var product = getProduct(_productId);
            if (product == null)
            {
                _callback?.Invoke(PurchasingResult.NotAvailable);
                return;
            }

            BuyProduct(product.IdType.AsEnum, _callback);
        }

        public void BuyProduct(PRODUCTTYPE _idType, Action<PurchasingResult> _callback)
        {
            buyCallback = _callback;
            var productModel = GetProduct(_idType);

            if (productModel == null)
            {
                CompleteBuyCallback(PurchasingResult.NotAvailable);
                return;
            }

#if UNITY_EDITOR
            completePurchase(productModel.Id.AsString);
            CompleteBuyCallback(PurchasingResult.Complete);
            return;
#else
            try
            {
                if (!IsInitialized())
                {
                    PurchasingService.AddError("BuyProduct Error " + (storeController == null ? "storeController is null" : "storeController ok") + (isConnected == true ? "isConnected true" : "isConnected false") + (isProductsFetched == true ? "isProductsFetched true" : "isProductsFetched false"));
                    CompleteBuyCallback(PurchasingResult.NotInitialized);
                    return;
                }

                var product = storeController.GetProducts()
                    .FirstOrDefault(item => item.definition.id == productModel.Id.AsString);

                if (product == null || !product.availableToPurchase)
                {
                    CompleteBuyCallback(PurchasingResult.NotAvailable);
                    return;
                }

                purchasingProductId = product.definition.id;
                storeController.PurchaseProduct(product.definition.id);
            }
            catch (Exception exception)
            {
                Debug.LogError("IAP Purchase Exception " + exception);
                CompleteBuyCallback(PurchasingResult.Unknown);
            }
#endif
        }

        public void RestorePurchases(Action<bool, string> _callback)
        {
            if (!IsInitialized())
            {
                _callback?.Invoke(false, string.Empty);
                Debug.Log("RestorePurchases FAIL. Not initialized.");
                return;
            }

            if (Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.OSXPlayer)
            {
                storeController.RestoreTransactions((success, error) =>
                {
                    _callback?.Invoke(success, error ?? string.Empty);
                });
                return;
            }

#if UNITY_ANDROID
            // Google Play restoration is performed by FetchPurchases in IAP v5.
            try
            {
                storeController.FetchPurchases();
                _callback?.Invoke(true, string.Empty);
            }
            catch (Exception exception)
            {
                _callback?.Invoke(false, exception.Message);
            }
#else
            _callback?.Invoke(false, string.Empty);
            Debug.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
#endif
        }

        private bool IsInitialized()
        {
            return storeController != null && isConnected && isProductsFetched;
        }

        public ProductModel<PRODUCTTYPE> GetProduct(PRODUCTTYPE _idType)
        {
            return productTable.FirstOrDefault(row => Enum.Equals(row.IdType.AsEnum, _idType));
        }

        private ProductModel<PRODUCTTYPE> getProduct(string _id)
        {
            return productTable.FirstOrDefault(row => row.Id.AsString == _id);
        }

        public ProductModelBase GetProductBase(string _id)
        {
            return productTable.FirstOrDefault(row => row.Id.AsString == _id);
        }

        private void OnStoreDisconnected(StoreConnectionFailureDescription failure)
        {
            isConnected = false;
            isProductsFetched = false;
            initializationStarted = false;

            var message = failure?.ToString() ?? "Unknown store connection failure";
            Debug.LogError("IAP Store disconnected: " + message);
            LogInitFailure("StoreDisconnected", message);
            PurchasingService.AddError("StoreDisconnected : " + message);
        }

        private void OnProductsFetched(List<Product> products)
        {
            isProductsFetched = true;
            initializationStarted = false;

            UpdateProductMetadata(products);

            // v5 initialization explicitly fetches existing entitlements/orders.
            storeController.FetchPurchases();
        }

        private void OnProductsFetchFailed(ProductFetchFailed failure)
        {

            var message = failure?.ToString() ?? "Unknown product fetch failure";
            
            var fetchedProducts = storeController.GetProducts();

            var fetchedIds = new HashSet<string>(
                fetchedProducts.Select(product => product.definition.id)
            );

            var requestedIds = productTable
                .Select(model => model.Id.AsString)
                .ToList();

            var missingIds = requestedIds
                .Where(id => !fetchedIds.Contains(id))
                .ToList();

            PurchasingService.AddError(
                "IAP products partially failed.\n" +
                "Reason: " + failure + "\n" +
                "Fetched: " + string.Join(", ", fetchedIds) + "\n" +
                "Missing: " + string.Join(", ", missingIds)
            );

            // 일부 상품이 성공한 경우 전체 초기화를 실패로 만들지 않는다.
            if (fetchedProducts.Count > 0)
            {
                isProductsFetched = true;
                initializationStarted = false;

                UpdateProductMetadata(fetchedProducts);
                storeController.FetchPurchases();
                return;
            }

            // 모든 상품 조회가 실패한 경우
            isProductsFetched = false;
            initializationStarted = false;
            Debug.LogError("IAP product fetch failed: " + message);
            LogInitFailure("ProductsFetchFailed", message);
        }

        private void UpdateProductMetadata(IEnumerable<Product> products)
        {
            SubscriptionState.AsEnum = SubscriptionStateType.Inactive;
            SubscriptionActiveIDs.Clear();

            foreach (var product in products)
            {
                var model = getProduct(product.definition.id);
                if (model == null)
                    continue;

                model.CurrencyCode.AsString = product.metadata.isoCurrencyCode;
                model.PriceString.AsString = product.metadata.localizedPriceString;
                model.Price.AsFloat = (float)product.metadata.localizedPrice;
                model.Title.AsString = product.metadata.localizedTitle;
            }

            TrySave();
        }

        private void OnPurchasesFetched(Orders orders)
        {
            SubscriptionState.AsEnum = SubscriptionStateType.Inactive;
            SubscriptionActiveIDs.Clear();

            // Confirmed orders represent restored/owned non-consumables and active subscriptions.
            foreach (var order in orders.ConfirmedOrders)
                RestoreConfirmedOrder(order);

            TrySave();
        }

        private void OnPurchasesFetchFailed(PurchasesFetchFailureDescription failure)
        {
            var message = failure?.ToString() ?? "Unknown purchases fetch failure";
            Debug.LogError("IAP purchases fetch failed: " + message);
            LogInitFailure("PurchasesFetchFailed", message);
            PurchasingService.AddError("PurchasesFetchFailed : " + message);
        }

        private void RestoreConfirmedOrder(ConfirmedOrder order)
        {
            foreach (var cartItem in order.CartOrdered.Items())
            {
                var product = cartItem.Product;
                var model = getProduct(product.definition.id);
                if (model == null || model.ProductType.AsEnum == ProductType.Consumable)
                    continue;

                if (model.ProductType.AsEnum == ProductType.NonConsumable)
                {
                    model.PurchaseCount.AsInt = 1;
                }
                else if (model.ProductType.AsEnum == ProductType.Subscription)
                {
                    ApplySubscriptionState(order, model);
                }
            }
        }

        private void OnPurchasePending(PendingOrder order)
        {
            var result = PurchasingResult.Complete;
            var receipt = order.Info?.Receipt;
            var purchasedIds = new List<string>();

            foreach (var cartItem in order.CartOrdered.Items())
            {
                var product = cartItem.Product;
                purchasedIds.Add(product.definition.id);

                var validationResult = checkRecipt(product.definition.id, receipt);
                if (validationResult == PurchasingResult.Complete)
                {
                    completePurchase(product.definition.id);

                    // Unity IAP 5 provides subscription status directly on the order.
                    // Apply the authoritative store subscription state after preserving
                    // the wrapper's existing purchase-completion behavior.
                    var model = getProduct(product.definition.id);
                    if (model != null &&
                        model.ProductType.AsEnum == ProductType.Subscription)
                    {
                        ApplySubscriptionState(order, model);
                    }
                }
                else if (validationResult == PurchasingResult.Refunded)
                {
                    completeRefund(product.definition.id);
                }

                if (validationResult != PurchasingResult.Complete)
                    result = validationResult;
            }

            // Equivalent to returning PurchaseProcessingResult.Complete in v4.
            if (result == PurchasingResult.Complete)
                storeController.ConfirmPurchase(order);

            if (!string.IsNullOrEmpty(purchasingProductId) && purchasedIds.Contains(purchasingProductId))
                CompleteBuyCallback(result);
        }

        private void OnPurchaseFailed(FailedOrder order)
        {
            var id = GetFirstProductId(order);
            var message = order?.ToString() ?? "Unknown purchase failure";

            BicUtil.Analytics.Analytics.Event("IAP_Fail_Detailed", new Dictionary<string, object>
            {
                {"reason", message},
                {"id", id},
                {"message", message}
            });

            Debug.LogError("OnPurchaseFailed: " + message);
            PurchasingService.AddError("OnPurchaseFailed : " + message);

            if (string.IsNullOrEmpty(purchasingProductId) || id == purchasingProductId)
                CompleteBuyCallback(PurchasingResult.Failed);
        }

        private void OnPurchaseDeferred(DeferredOrder order)
        {
            var id = GetFirstProductId(order);
            Debug.Log("IAP purchase deferred: " + id);

            // Preserve the old wrapper's result vocabulary; deferred is not a completed purchase.
            if (string.IsNullOrEmpty(purchasingProductId) || id == purchasingProductId)
                CompleteBuyCallback(PurchasingResult.Failed);
        }

        private static string GetFirstProductId(Order order)
        {
            return order?.CartOrdered?.Items()?.FirstOrDefault()?.Product?.definition?.id ?? string.Empty;
        }

        private void CompleteBuyCallback(PurchasingResult result)
        {
            var callback = buyCallback;
            buyCallback = null;
            purchasingProductId = null;
            callback?.Invoke(result);
        }

        private void completePurchase(string _id)
        {
            var productInfo = getProduct(_id);
            if (productInfo == null)
                return;

            if (productInfo.ProductType.AsEnum == ProductType.Consumable)
            {
                productInfo.PurchaseCount.AsInt += 1;
            }
            else if (productInfo.ProductType.AsEnum == ProductType.NonConsumable)
            {
                productInfo.PurchaseCount.AsInt = 1;
            }
            else if (productInfo.ProductType.AsEnum == ProductType.Subscription)
            {
                productInfo.PurchaseCount.AsInt = 1;
                AddSubscriptionActiveId(productInfo.IdType.AsEnum);
                SubscriptionState.AsEnum = SubscriptionStateType.Active;
            }

            productTable.Save();

            BicUtil.Analytics.Analytics.Event("IAP_Success", new Dictionary<string, object>
            {
                {"id", _id},
                {"currency", productInfo.CurrencyCode.AsString},
                {"revenue", productInfo.Price.AsFloat},
                {"title", productInfo.Title.AsString}
            });
        }

        public void Save(Action<BicDB.Result> _callback = null, object _parameter = null)
        {
            productTable.Save(_callback, _parameter);
        }

        private void completeRefund(string _id)
        {
            var productInfo = getProduct(_id);
            if (productInfo == null)
                return;

            if (productInfo.ProductType.AsEnum == ProductType.Consumable)
                productInfo.PurchaseCount.AsInt = Math.Max(0, productInfo.PurchaseCount.AsInt - 1);
            else
                productInfo.PurchaseCount.AsInt = 0;

            BicUtil.Analytics.Analytics.Event("IAP_Refund", new Dictionary<string, object>
            {
                {"id", _id}
            });

            productTable.Save();
        }

        /// <summary>
        /// Kept public and misspelled for source compatibility with existing callers.
        /// IAP v5 deprecates Apple local receipt validation; Google receipt validation uses Order.Info.Receipt.
        /// </summary>
        public PurchasingResult checkRecipt(string _productId, string _recipt)
        {
#if UNITY_EDITOR
            return PurchasingResult.Complete;
#elif UNITY_ANDROID
            if (string.IsNullOrEmpty(_recipt) || googleTangle == null)
                return PurchasingResult.Unknown;

            try
            {
                var validator = new CrossPlatformValidator(googleTangle, null, Application.identifier);
                validator.Validate(_recipt);
                return PurchasingResult.Complete;
            }
            catch (IAPSecurityException)
            {
                return PurchasingResult.Unknown;
            }
#elif UNITY_IOS || UNITY_STANDALONE_OSX
            // IAP v5 / StoreKit 2: Apple local receipt validation is deprecated.
            // Validate OrderInfo.Apple.jwsRepresentation on your server when fraud prevention is required.
            return PurchasingResult.Complete;
#else
            return PurchasingResult.Complete;
#endif
        }

        /// <summary>
        /// Applies subscription state using Unity IAP 5's order-provided SubscriptionInfo.
        /// SubscriptionManager was part of the legacy IAP flow and is not available in
        /// the IAP 5 player runtime assemblies.
        /// </summary>
        private void ApplySubscriptionState(
            Order order,
            ProductModel<PRODUCTTYPE> model)
        {
        #if UNITY_EDITOR
            if (model.PurchaseCount.AsInt > 0)
            {
                AddSubscriptionActiveId(model.IdType.AsEnum);
                SubscriptionState.AsEnum = SubscriptionStateType.Active;
            }
        #else
            try
            {
                var info = order?.Info?.PurchasedProductInfo?
                    .Select(purchasedProduct => purchasedProduct.subscriptionInfo)
                    .FirstOrDefault(subscription => subscription != null);

                if (info == null)
                {
                    Debug.LogWarning(
                        "IAP subscription information is missing from the order: " +
                        model.Id.AsString);

                    model.PurchaseCount.AsInt = 0;
                    SubscriptionActiveIDs.Remove(model.IdType.AsEnum);
                    return;
                }

                var isSubscribed = info.IsSubscribed();
                var isExpired = info.IsExpired();

                if (isSubscribed == UnityEngine.Purchasing.Result.True &&
                    isExpired == UnityEngine.Purchasing.Result.False)
                {
                    model.PurchaseCount.AsInt = 1;
                    SubscriptionInfo = info;
                    AddSubscriptionActiveId(model.IdType.AsEnum);
                    SubscriptionState.AsEnum = SubscriptionStateType.Active;
                }
                else
                {
                    model.PurchaseCount.AsInt = 0;
                    SubscriptionActiveIDs.Remove(model.IdType.AsEnum);
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning(
                    "IAP subscription state check failed for " +
                    model.Id.AsString + ": " + exception.Message);

                model.PurchaseCount.AsInt = 0;
                SubscriptionActiveIDs.Remove(model.IdType.AsEnum);
            }
        #endif
        }

        private void AddSubscriptionActiveId(PRODUCTTYPE id)
        {
            if (!SubscriptionActiveIDs.Contains(id))
                SubscriptionActiveIDs.Add(id);
        }

        public TableLoadData GetTableLoadData()
        {
            productTable.SetStorage(FileStorage.GetInstance());
            return new TableLoadData(productTable, new FileStorageParameter("purchase"), result =>
            {
                if (!result.IsSuccess)
                    return false;

                isLoad = true;
                checkSubscribeMaybe();
                return true;
            });
        }

        private void checkSubscribeMaybe()
        {
            SubscriptionPerhapsIDs.Clear();
            subscriptionState.AsEnum = SubscriptionStateType.Inactive;

            for (var i = 0; i < productTable.Count; i++)
            {
                var product = productTable[i];
                if (product.ProductType.AsEnum == ProductType.Subscription && product.PurchaseCount.AsInt > 0)
                {
                    if (!SubscriptionPerhapsIDs.Contains(product.IdType.AsEnum))
                        SubscriptionPerhapsIDs.Add(product.IdType.AsEnum);
                    subscriptionState.AsEnum = SubscriptionStateType.Perhaps;
                }
            }
        }

        /// <summary>
        /// Retained for caller compatibility. In v5 this refreshes purchases/entitlements asynchronously.
        /// Results are applied in OnPurchasesFetched.
        /// </summary>
        public void CheckIfSubscriptionIsActive()
        {
            if (IsInitialized())
                storeController.FetchPurchases();
        }

        private void TrySave()
        {
            try
            {
                Save();
            }
            catch (Exception exception)
            {
                Debug.LogWarning("IAP save failed: " + exception.Message);
            }
        }

        private static void LogInitFailure(string reason, string message)
        {
            BicUtil.Analytics.Analytics.Event("IAP_Init_Fail", new Dictionary<string, object>
            {
                {"reason", reason},
                {"message", message ?? string.Empty}
            });
        }
    }

    public enum SubscriptionStateType
    {
        Inactive,
        Perhaps,
        Active
    }
}
#endif