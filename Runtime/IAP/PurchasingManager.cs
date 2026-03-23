#if BICUTIL_IAP
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using BicUtil.SingletonBase;
using UnityEngine.Purchasing.Security;
using BicDB.Container;
using System.Linq;
using BicDB.Storage;
using BicDB.Variable;
using BicDB;
using BicDB.Core;
using BicUtil.Analytics;
using UnityEngine.Purchasing.Extension;

namespace BicUtil.Purchasing{
	public static class PurchasingService{
		public static Func<string, ProductModelBase> GetProduct = null;
		public static Action<string, Action<PurchasingResult>> BuyProduct = null;
		public static Action<Action<bool, string>> RestorePurchases = null;

		public static string File = "Puma";
	}
	public class PurchasingManager<PRODUCTTYPE> : SingletonBase<PurchasingManager<PRODUCTTYPE>>, IDetailedStoreListener, IPurchasingManager<PRODUCTTYPE> where PRODUCTTYPE : struct, Enum {
		public TableContainer<ProductModel<PRODUCTTYPE>> productTable = new TableContainer<ProductModel<PRODUCTTYPE>>(PurchasingService.File);
		public SubscriptionInfo SubscriptionInfo = null;
		private EnumVariable<SubscriptionStateType> subscriptionState = new EnumVariable<SubscriptionStateType>(Purchasing.SubscriptionStateType.Inactive);
		public EnumVariable<SubscriptionStateType> SubscriptionState{get{return subscriptionState;}}
		public List<PRODUCTTYPE> SubscriptionActiveIDs {get;set;} = new List<PRODUCTTYPE>();
		public List<PRODUCTTYPE> SubscriptionPerhapsIDs {get;set;} = new List<PRODUCTTYPE>();

		private bool isLoad = false;
		public void AddProduct(PRODUCTTYPE _idType, string _id, ProductType _productType, int _amount, string _defaultCurrentCode, string _defaultPriceString, float _defaultPrice, string _title, Action<IVariableReadOnly> _valueChangedCallback){
			if(isLoad == false){
				throw new SystemException("Not Load PurchasingManager");
			}

			if(googleTangle == null && appleTangle == null){
				throw new Exception("SETUP TANGLES");
			}
			
			var _product = GetProduct(_idType);

			if(_product == null){
				_product = new ProductModel<PRODUCTTYPE>(_idType, _id, _productType, _amount);
				productTable.Add(_product);
			}

            _product.Price.AsFloat = _defaultPrice;
            _product.PriceString.AsString = _defaultPriceString;
            _product.CurrencyCode.AsString = _defaultCurrentCode;
            _product.Amount.AsInt = _amount;
            _product.Title.AsString = _title;
            _product.ProductType.AsEnum = _productType;
			_product.Id.AsString = _id;

			if(_valueChangedCallback != null){
				_product.PurchaseCount.Subscribe(_valueChangedCallback, true);
			}
		}

		private byte[] googleTangle = null;
		private byte[] appleTangle = null;
		public void SetupTangle(Func<byte[]> _googleTangle, Func<byte[]> _appleTangle){
			#if UNITY_EDITOR
				googleTangle = new byte[1];
				appleTangle = new byte[1];
				return;
			#endif

			#if UNITY_ANDROID
			googleTangle = _googleTangle();
			#endif

			#if UNITY_IOS
			appleTangle = _appleTangle();
			#endif
		}

		public override void Initialize() 
		{

			PurchasingService.GetProduct = this.GetProductBase;
			PurchasingService.BuyProduct = this.BuyProductByStringId;
			PurchasingService.RestorePurchases = this.RestorePurchases;

			if(productTable.Count == 0){
				return;
			}

			// If we have already connected to Purchasing ...
			if (IsInitialized())
			{
				// ... we are done here.
				return;
			}

			// Create a builder, first passing in a suite of Unity provided stores.
			var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

			foreach(var _product in productTable){
				builder.AddProduct(_product.Id.AsString, _product.ProductType.AsEnum, _product.StoreIds);
			}
			
			try{
				UnityPurchasing.Initialize(this, builder);
			}catch(Exception _exception){
				Debug.LogError("IAP Init Exception " + _exception.ToString());
			}
		}

		public void BuyProductByStringId(string _productId, Action<PurchasingResult> _callback){
			var _product = getProduct(_productId);
			BuyProduct(_product.IdType.AsEnum, _callback);
		}

		private Action<PurchasingResult> buyCallback;
		public void BuyProduct(PRODUCTTYPE _idType, Action<PurchasingResult> _callback)
		{	
			buyCallback = _callback;
			ProductModel<PRODUCTTYPE> _product = GetProduct(_idType);

			#if UNITY_EDITOR
			if(buyCallback != null){
				completePurchase(_product.Id.AsString);
				buyCallback(PurchasingResult.Complete);
				buyCallback = null;
			}
			return;
			#else

			// If the stores throw an unexpected exception, use try..catch to protect my logic here.
			try
			{
				// If Purchasing has been initialized ...
				if (IsInitialized())
				{
					// ... look up the Product reference with the general product identifier and the Purchasing system's products collection.
					Product product = m_StoreController.products.WithID(_product.Id.AsString);
					
					// If the look up found a product for this device's store and that product is ready to be sold ... 
					if (product != null && product.availableToPurchase)
					{
						m_StoreController.InitiatePurchase(product);
					}
					// Otherwise ...
					else if(buyCallback != null)
					{
						// ... report the product look-up failure situation  
						buyCallback(PurchasingResult.NotAvailable);
						buyCallback = null;
					}
				}
				// Otherwise ...
				else if(buyCallback != null)
				{
					// ... report the fact Purchasing has not succeeded initializing yet. Consider waiting longer or retrying initiailization.
					buyCallback(PurchasingResult.NotInitialized);
					buyCallback = null;
				}
			}
			// Complete the unexpected exception handling ...
			catch (Exception)
			{
				// ... by reporting any unexpected exception for later diagnosis.
				if(buyCallback != null){
					buyCallback(PurchasingResult.Unknown);
					buyCallback = null;
				}
			}
			#endif
		}

		// 애플로 출시할때는 이 코드를 추가해야하나봄??? 
		// Restore purchases previously made by this customer. Some platforms automatically restore purchases. Apple currently requires explicit purchase restoration for IAP.
		public void RestorePurchases(Action<bool, string> _callback)
		{
			// If Purchasing has not yet been set up ...
			if (!IsInitialized())
			{
				if(_callback != null){
					_callback(false, string.Empty);
				}
				// ... report the situation and stop restoring. Consider either waiting longer, or retrying initialization.
				Debug.Log("RestorePurchases FAIL. Not initialized.");
				return;
			}

			// If we are running on an Apple device ... 
			if (Application.platform == RuntimePlatform.IPhonePlayer || 
				Application.platform == RuntimePlatform.OSXPlayer)
			{
				//TODO: 추가필요
				// ... begin restoring purchases
				Debug.Log("RestorePurchases started ...");
				
				// Fetch the Apple store-specific subsystem.
				var apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();
				// Begin the asynchronous process of restoring purchases. Expect a confirmation response in the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
				apple.RestoreTransactions(_callback);
			}
			// Otherwise ...
			else
			{
				if(_callback != null){
					_callback(false, string.Empty);
				}

				// We are not running on an Apple device. No work is necessary to restore purchases.
				Debug.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
			}
		}




		private IStoreController m_StoreController;             // Reference to the Purchasing system.
		private IExtensionProvider m_StoreExtensionProvider;    // Reference to store-specific Purchasing subsystems.
		
		private bool IsInitialized()
		{
			// Only say we are initialized if both the Purchasing references are set.
			return m_StoreController != null && m_StoreExtensionProvider != null;
		}

		public ProductModel<PRODUCTTYPE> GetProduct(PRODUCTTYPE _idType){
			
			return productTable.FirstOrDefault<ProductModel<PRODUCTTYPE>>(_row=>Enum.Equals(_row.IdType.AsEnum, _idType));
		}

		private ProductModel<PRODUCTTYPE> getProduct(string _id){
			return productTable.FirstOrDefault(_row=>_row.Id.AsString == _id);
		}

		public ProductModelBase GetProductBase(string _id){
			return productTable.FirstOrDefault(_row=>_row.Id.AsString == _id);
		}

		//  
		// --- IStoreListener
		//
		
		public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
		{
			var _appleExtensions = extensions.GetExtension<IAppleExtensions>();
			Dictionary<string, string> _introductoryInfo = null;
			if(_appleExtensions != null){
				_introductoryInfo = _appleExtensions.GetIntroductoryPriceDictionary();
			}

			// Purchasing has succeeded initializing. Collect our Purchasing references.
			// Overall Purchasing system, configured with products for this application.
			m_StoreController = controller;
			// Store specific subsystem, for accessing device-specific store features.
			m_StoreExtensionProvider = extensions;
			
			SubscriptionState.AsEnum = SubscriptionStateType.Inactive;
			
			foreach(var _product in m_StoreController.products.all){
				var _model = this.productTable.FirstOrDefault(_row=>_row.Id.AsString == _product.definition.id);
				if(_model != null){
					_model.CurrencyCode.AsString = _product.metadata.isoCurrencyCode;
					_model.PriceString.AsString = _product.metadata.localizedPriceString;
					_model.Price.AsFloat = (float)_product.metadata.localizedPrice;
					_model.Title.AsString = _product.metadata.localizedTitle;
					
					if(_model.ProductType.AsEnum == ProductType.Subscription){
					#if UNITY_EDITOR
						if(_model.PurchaseCount.AsInt > 0){
                            this.SubscriptionActiveIDs.Add(_model.IdType.AsEnum);
							this.SubscriptionState.AsEnum = Purchasing.SubscriptionStateType.Active;
						}
					#else
					try{
						// Debug.Log("[pixaw] checkRecipt " + _product.definition.id);
						// Debug.Log("[pixaw] before PurchaseCount = " + _model.PurchaseCount.AsString);
						var _result = checkRecipt(_product.definition.id, _product.receipt);
						if(_result == PurchasingResult.Complete){
							// Debug.Log("[pixaw] checkRecipt PurchasingResult.Complete");
							string _introJson = (_introductoryInfo == null || !_introductoryInfo.ContainsKey(_product.definition.storeSpecificId)) ? null : _introductoryInfo[_product.definition.storeSpecificId];
							var _subscriptionManager = new SubscriptionManager(_product, _introJson);
							var _subscriptionInfo = _subscriptionManager.getSubscriptionInfo();
							
							// Debug.Log("[pixaw] _subscriptionInfo.isSubscribed() " + _subscriptionInfo.isSubscribed().ToString());
							// Debug.Log("[pixaw] _subscriptionInfo.isExpired() " + _subscriptionInfo.isExpired().ToString());
							// Debug.Log("[pixaw] _subscriptionInfo.getExpireDate() " + _subscriptionInfo.getExpireDate().ToString());

							if(_subscriptionInfo.isSubscribed() == UnityEngine.Purchasing.Result.False || _subscriptionInfo.isExpired() == UnityEngine.Purchasing.Result.True){
								_model.PurchaseCount.AsInt = 0;
								this.SubscriptionState.AsEnum = SubscriptionStateType.Inactive;
								// Debug.Log("[pixaw] purchasing count = 0");
							}else if(_subscriptionInfo.isSubscribed() == UnityEngine.Purchasing.Result.True && _subscriptionInfo.isExpired() == UnityEngine.Purchasing.Result.False){
								_model.PurchaseCount.AsInt = 1;
								SubscriptionInfo = _subscriptionInfo;
								this.SubscriptionActiveIDs.Add(_model.IdType.AsEnum);
								this.SubscriptionState.AsEnum = SubscriptionStateType.Active;
								// Debug.Log("[pixaw] purchasing count = 1 Active");
							}else{
								this.SubscriptionState.AsEnum = SubscriptionStateType.Inactive;
								_model.PurchaseCount.AsInt = 0;
							}
						}
					}catch(Exception){
						// Debug.Log("[pixaw] checkRecipt fail exception");
						this.SubscriptionState.AsEnum = SubscriptionStateType.Inactive;
						_model.PurchaseCount.AsInt = 0;
					}
					#endif
					}
				}
			}

			try{
				this.Save();
			}catch{

			}
		}
		
		
		public void OnInitializeFailed(InitializationFailureReason error)
		{
			BicUtil.Analytics.Analytics.Event("IAP_Init_Fail", new Dictionary<string, object>{
				{"reason", error.ToString()}
			});

			// Purchasing set-up has not succeeded. Check error for reason. Consider sharing this reason with the user.
			Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
		}
		
		
		public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args) 
		{
			var _result = checkRecipt(args.purchasedProduct.definition.id, args.purchasedProduct.receipt);
			if(_result == PurchasingResult.Complete)
			{
				completePurchase(args.purchasedProduct.definition.id);

			}else if(_result == PurchasingResult.Refunded){
				completeRefund(args.purchasedProduct.definition.id);
			}

			if(buyCallback != null){
                buyCallback(_result);
				buyCallback = null;
			}

			return PurchaseProcessingResult.Complete;
		}

        private void completePurchase(string _id)
        {
			ProductModel<PRODUCTTYPE> _productInfo = getProduct(_id);
			// Debug.Log("[pixaw] completePurchase " + _id + " / " + _productInfo.ProductType.AsString);
            if (_productInfo.ProductType.AsEnum == ProductType.Consumable)
            {
                _productInfo.PurchaseCount.AsInt += 1;
            }
            else if (_productInfo.ProductType.AsEnum == ProductType.NonConsumable)
            {
                _productInfo.PurchaseCount.AsInt = 1;
            }else if(_productInfo.ProductType.AsEnum == ProductType.Subscription){
				// Debug.Log("[pixaw] _productInfo.PurchaseCount.AsInt = 1");
				_productInfo.PurchaseCount.AsInt = 1;
				this.SubscriptionActiveIDs.Add(_productInfo.IdType.AsEnum);
                this.SubscriptionState.AsEnum = Purchasing.SubscriptionStateType.Active;
			}

			productTable.Save();

			BicUtil.Analytics.Analytics.Event("IAP_Success", new Dictionary<string, object>{
				{"id", _id},
				{"currency", _productInfo.CurrencyCode.AsString},
				{"revenue", _productInfo.Price.AsFloat},
				{"title", _productInfo.Title.AsString}
			});
        }

		public void Save(Action<BicDB.Result> _callback = null, object _parameter = null){
			productTable.Save(_callback, _parameter);
		}
		
		private void completeRefund(string _id)
        {
            ProductModel<PRODUCTTYPE> _productInfo = getProduct(_id);
            if (_productInfo.ProductType.AsEnum == ProductType.Consumable)
            {
                _productInfo.PurchaseCount.AsInt -= 1;
            }
            else if (_productInfo.ProductType.AsEnum == ProductType.NonConsumable)
            {
                _productInfo.PurchaseCount.AsInt = 0;
            }
            else if (_productInfo.ProductType.AsEnum == ProductType.Subscription)
            {
                _productInfo.PurchaseCount.AsInt = 0;
            }


			BicUtil.Analytics.Analytics.Event("IAP_Refund", new Dictionary<string, object>{
				{"id", _id}
			});

			productTable.Save();
        }

        public PurchasingResult checkRecipt(string _productId, string _recipt) {

			// #if UNITY_IOS || UNITY_STANDALONE_OSX
			// return PurchasingResult.Complete;
			// #endif
			bool _isValidPurchase = true; // Presume valid for platforms with no R.V.


			#if UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX  
			var validator = new CrossPlatformValidator(googleTangle, appleTangle, Application.identifier);  
			
			 try {
				// On Google Play, result has a single product ID.
				// On Apple stores, receipts contain multiple products.
				var result = validator.Validate(_recipt);
				// For informational purposes, we list the receipt(s)
				// Debug.Log("[pixaw] Receipt is valid. Contents:");
				// foreach (IPurchaseReceipt productReceipt in result) {
				// 	Debug.Log("[pixaw]" + productReceipt.productID);
				// 	Debug.Log("[pixaw]" + productReceipt.purchaseDate);
				// 	Debug.Log("[pixaw]" + productReceipt.transactionID);
				// }
			} catch (IAPSecurityException) {
				// Debug.Log("[pixaw] Invalid receipt, not unlocking content");
				_isValidPurchase = false;
			}
			// try  
			// {  
			// 	var result = validator.Validate(_recipt);  
			// 	foreach (IPurchaseReceipt productReceipt in result)  
			// 	{  
			// 		if (String.Equals(productReceipt.productID, _productId, StringComparison.Ordinal))  
			// 		{  
			// 			AppleReceipt apple = productReceipt as AppleReceipt;
			// 			apple.
			// 			GooglePlayReceipt google = productReceipt as GooglePlayReceipt;  
			// 			if (google != null)  
			// 			{  
			// 				Debug.Log(google.purchaseState.ToString());
			// 				switch (google.purchaseState)  
			// 				{  
			// 					case GooglePurchaseState.Purchased:  
			// 						return PurchasingResult.Complete;
			// 					case GooglePurchaseState.Cancelled:  
			// 					case GooglePurchaseState.Refunded:  
			// 						return PurchasingResult.Refunded;
			// 				}  
			// 			}else{
			// 				return PurchasingResult.Unknown;
			// 			}
			// 		}  
			// 	}  

			// 	return PurchasingResult.Unknown;
			// }  
			// catch (IAPSecurityException)  
			// {  
			// 	return PurchasingResult.Unknown;
			// }  
			#endif  

			if(_isValidPurchase == true){
				return PurchasingResult.Complete;
			}else{
				return PurchasingResult.Unknown;
			}
		}
		
		public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
		{
			var _product = getProduct(product.definition.id);

			BicUtil.Analytics.Analytics.Event("IAP_Fail", new Dictionary<string, object>{
				{"reason", failureReason.ToString()},
				{"id", product.definition.storeSpecificId}
			});
			
			if(buyCallback != null){
				buyCallback(PurchasingResult.Failed);
				buyCallback = null;
			}
			// A product purchase attempt did not succeed. Check failureReason for more detail. Consider sharing this reason with the user.
			Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}",product.definition.storeSpecificId, failureReason));
		}

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            var _product = getProduct(product.definition.id);
			
			if(buyCallback != null){
				buyCallback(PurchasingResult.Failed);
				buyCallback = null;
			}
			// A product purchase attempt did not succeed. Check failureReason for more detail. Consider sharing this reason with the user.
			Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1} , message {2}",product.definition.storeSpecificId, failureDescription.reason, failureDescription.message));
			
			BicUtil.Analytics.Analytics.Event("IAP_Fail_Detailed", new Dictionary<string, object>{
				{"reason", failureDescription.reason.ToString()},
				{"id", product.definition.storeSpecificId},
				{"message", failureDescription.message}
			});
        }

		public TableLoadData GetTableLoadData(){
			this.productTable.SetStorage(FileStorage.GetInstance());
			return new TableLoadData(this.productTable, new FileStorageParameter("purchase"), _result=>{
				if(_result.IsSuccess == true){
					isLoad = true;
					checkSubscribeMaybe();
					return true;
				}else{
					return false;
				}
			});
		}

        private void checkSubscribeMaybe()
        {

			// Debug.Log("[pixaw] checkSubscribeMaybe");

			this.subscriptionState.AsEnum = Purchasing.SubscriptionStateType.Inactive;
            for(int i = 0; i < this.productTable.Count; i++){
				var _product = this.productTable[i];
				// Debug.Log("[pixaw] product id = " + _product.Id.AsString);
				// Debug.Log("[pixaw] product type = " + _product.ProductType.AsString);
				// Debug.Log("[pixaw] product purchasingCount = " + _product.PurchaseCount.AsString);
				if(_product.ProductType.AsEnum == ProductType.Subscription && _product.PurchaseCount.AsInt > 0){
					this.SubscriptionPerhapsIDs.Add(_product.IdType.AsEnum);
					this.subscriptionState.AsEnum = Purchasing.SubscriptionStateType.Perhaps;
					// Debug.Log("[pixaw] subscriptionState is perhaps");
				}
			}
        }

        public void CheckIfSubscriptionIsActive(){
			ConfigurationBuilder builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
			// Get a reference to IAppleConfiguration during IAP initialization.
			IAppleConfiguration appleConfig = builder.Configure<IAppleConfiguration>();
			if (!string.IsNullOrEmpty (appleConfig.appReceipt)) {
				// Debug.Log (appleConfig.appReceipt);
	//            InstantiateDebugText (DebugInfoPanel, "APP Receipt Base64 " + appleConfig.appReceipt);
				var receiptData = System.Convert.FromBase64String (appleConfig.appReceipt);
	//            InstantiateDebugText (DebugInfoPanel, "receipt Data "+ receiptData);
				AppleReceipt receipt = new UnityEngine.Purchasing.Security.AppleValidator (appleTangle).Validate (receiptData);
				foreach (AppleInAppPurchaseReceipt productReceipt in receipt.inAppPurchaseReceipts) {
					// Debug.Log ("PRODUCTID: " + productReceipt.productID);
					// Debug.Log ("PURCHASE DATE: " + productReceipt.purchaseDate);
					// Debug.Log ("EXPIRATION DATE: " + productReceipt.subscriptionExpirationDate);
					// Debug.Log ("CANCELDATE DATE: " + productReceipt.cancellationDate);
					
				}
			}
        }

		private bool checkIfProductIsAvailableForSubscriptionManager(string receipt) {
			var receipt_wrapper = (Dictionary<string, object>)MiniJson.JsonDecode(receipt);
			if (!receipt_wrapper.ContainsKey("Store") || !receipt_wrapper.ContainsKey("Payload")) {
				// Debug.Log("The product receipt does not contain enough information");
				return false;
			}
			var store = (string)receipt_wrapper ["Store"];
			var payload = (string)receipt_wrapper ["Payload"];

			if (payload != null ) {
				switch (store) {
				case GooglePlay.Name:
					{
						var payload_wrapper = (Dictionary<string, object>)MiniJson.JsonDecode(payload);
						if (!payload_wrapper.ContainsKey("json")) {
							// Debug.Log("The product receipt does not contain enough information, the 'json' field is missing");
							return false;
						}
						var original_json_payload_wrapper = (Dictionary<string, object>)MiniJson.JsonDecode((string)payload_wrapper["json"]);
						if (original_json_payload_wrapper == null || !original_json_payload_wrapper.ContainsKey("developerPayload")) {
							// Debug.Log("The product receipt does not contain enough information, the 'developerPayload' field is missing");
							return false;
						}
						var developerPayloadJSON = (string)original_json_payload_wrapper["developerPayload"];
						var developerPayload_wrapper = (Dictionary<string, object>)MiniJson.JsonDecode(developerPayloadJSON);
						if (developerPayload_wrapper == null || !developerPayload_wrapper.ContainsKey("is_free_trial") || !developerPayload_wrapper.ContainsKey("has_introductory_price_trial")) {
							// Debug.Log("The product receipt does not contain enough information, the product is not purchased using 1.19 or later");
							return false;
						}
						return true;
					}
				case AppleAppStore.Name:
				case AmazonApps.Name:
				case MacAppStore.Name:
					{
						return true;
					}
				default:
					{
						return false;
					}
				}
			}
			return false;
		}

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            BicUtil.Analytics.Analytics.Event("IAP_Init_Fail", new Dictionary<string, object>{
				{"reason", error.ToString()},
				{"message", message}
			});

			// Purchasing set-up has not succeeded. Check error for reason. Consider sharing this reason with the user.
			Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
        }

    }

	public enum SubscriptionStateType{
		Inactive,
		Perhaps,
		Active
	}
}
#endif