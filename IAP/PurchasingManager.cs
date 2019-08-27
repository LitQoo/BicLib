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

namespace BicUtil.Purchasing{
	public class PurchasingManager<PRODUCTTYPE> : SingletonBase<PurchasingManager<PRODUCTTYPE>>, IStoreListener, IPurchasingManager<PRODUCTTYPE> where PRODUCTTYPE : struct {
		public TableContainer<ProductModel<PRODUCTTYPE>> productTable = new TableContainer<ProductModel<PRODUCTTYPE>>("Puma");
		public SubscriptionInfo SubscriptionInfo = null;
		private BoolVariable isSubscribed = new BoolVariable(false);
		public BoolVariable IsSubscribed{get{return isSubscribed;}}
		private bool isLoad = false;
		public void AddProduct(PRODUCTTYPE _idType, string _id, ProductType _productType, int _amount, string _defaultCurrentCode, string _defaultPriceString, float _defaultPrice, string _title, Action<IVariable> _valueChangedCallback){
			if(isLoad == false){
				throw new SystemException("Not Load PurchasingManager");
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


		public override void Initialize() 
		{
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
			
			UnityPurchasing.Initialize(this, builder);
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
		public void RestorePurchases(Action<bool> _callback)
		{
			// If Purchasing has not yet been set up ...
			if (!IsInitialized())
			{
				if(_callback != null){
					_callback(false);
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
					_callback(false);
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
		
		
		//  
		// --- IStoreListener
		//
		
		public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
		{
			Dictionary<string, string> _introductoryInfo = extensions.GetExtension<IAppleExtensions>().GetIntroductoryPriceDictionary();
			// Purchasing has succeeded initializing. Collect our Purchasing references.
			// Overall Purchasing system, configured with products for this application.
			m_StoreController = controller;
			// Store specific subsystem, for accessing device-specific store features.
			m_StoreExtensionProvider = extensions;

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
							IsSubscribed.AsBool = true;
						}
					#else
					try{
						var _result = checkRecipt(_product.definition.id, _product.receipt);
						if(_result == PurchasingResult.Complete){
							string _introJson = (_introductoryInfo == null || !_introductoryInfo.ContainsKey(_product.definition.storeSpecificId)) ? null : _introductoryInfo[_product.definition.storeSpecificId];
							var _subscriptionManager = new SubscriptionManager(_product, _introJson);
							var _subscriptionInfo = _subscriptionManager.getSubscriptionInfo();
							
							if(_subscriptionInfo.isSubscribed() == UnityEngine.Purchasing.Result.False || _subscriptionInfo.isExpired() == UnityEngine.Purchasing.Result.True){
								_model.PurchaseCount.AsInt = 0;
							}else if(_subscriptionInfo.isSubscribed() == UnityEngine.Purchasing.Result.True && _subscriptionInfo.isExpired() == UnityEngine.Purchasing.Result.False){
								_model.PurchaseCount.AsInt = 1;
								SubscriptionInfo = _subscriptionInfo;
								IsSubscribed.AsBool = true;
							}
						}
					}catch(Exception){
						_model.PurchaseCount.AsInt = 0;
					}
					#endif
					}
				}
				
				// try{
				// 	if(_product.hasReceipt){
				// 		//FIXME: 여긴 절대 실행되지 않는 구간인듯?
				// 		if(buyCallback != null){
				// 			var _result = checkRecipt(_product.definition.id, _product.receipt);
				// 			if(_result == PurchasingResult.Complete){
				// 				completePurchase(_product.definition.id);
				// 			}else if(_result == PurchasingResult.Refunded){
				// 				completeRefund(_product.definition.id);
				// 			}
				// 		}
				// 	}
				// }catch(Exception){
				// 	Debug.Log("not support product " + _product.definition.id);
				// }
			}
		}
		
		
		public void OnInitializeFailed(InitializationFailureReason error)
		{
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
            if (_productInfo.ProductType.AsEnum == ProductType.Consumable)
            {
                _productInfo.PurchaseCount.AsInt += 1;
            }
            else if (_productInfo.ProductType.AsEnum == ProductType.NonConsumable)
            {
                _productInfo.PurchaseCount.AsInt = 1;
            }else if(_productInfo.ProductType.AsEnum == ProductType.Subscription){
				_productInfo.PurchaseCount.AsInt = 1;
				IsSubscribed.AsBool = true;
			}



			productTable.Save();
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

			productTable.Save();
        }

        public PurchasingResult checkRecipt(string _productId, string _recipt) {

			// #if UNITY_IOS || UNITY_STANDALONE_OSX
			// return PurchasingResult.Complete;
			// #endif
			bool _isValidPurchase = true; // Presume valid for platforms with no R.V.


			#if UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX  
			var validator = new CrossPlatformValidator(GooglePlayTangle.Data(), AppleTangle.Data(), Application.identifier);  
			
			 try {
				// On Google Play, result has a single product ID.
				// On Apple stores, receipts contain multiple products.
				var result = validator.Validate(_recipt);
				// For informational purposes, we list the receipt(s)
				Debug.Log("Receipt is valid. Contents:");
				foreach (IPurchaseReceipt productReceipt in result) {
					Debug.Log(productReceipt.productID);
					Debug.Log(productReceipt.purchaseDate);
					Debug.Log(productReceipt.transactionID);
				}
			} catch (IAPSecurityException) {
				Debug.Log("Invalid receipt, not unlocking content");
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
			
			if(buyCallback != null){
				buyCallback(PurchasingResult.Failed);
				buyCallback = null;
			}
			// A product purchase attempt did not succeed. Check failureReason for more detail. Consider sharing this reason with the user.
			Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}",product.definition.storeSpecificId, failureReason));
		}

		public TableLoadData GetTableLoadData(){
			this.productTable.SetStorage(FileStorage.GetInstance());
			return new TableLoadData(this.productTable, new FileStorageParameter("purchase"), _result=>{
				if(_result.IsSuccess == true){
					isLoad = true;
					return true;
				}else{
					return false;
				}
			});
		}

		public void CheckIfSubscriptionIsActive(){
			ConfigurationBuilder builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
			// Get a reference to IAppleConfiguration during IAP initialization.
			IAppleConfiguration appleConfig = builder.Configure<IAppleConfiguration>();
			if (!string.IsNullOrEmpty (appleConfig.appReceipt)) {
				Debug.Log (appleConfig.appReceipt);
	//            InstantiateDebugText (DebugInfoPanel, "APP Receipt Base64 " + appleConfig.appReceipt);
				var receiptData = System.Convert.FromBase64String (appleConfig.appReceipt);
	//            InstantiateDebugText (DebugInfoPanel, "receipt Data "+ receiptData);
				AppleReceipt receipt = new AppleValidator (AppleTangle.Data ()).Validate (receiptData);
				foreach (AppleInAppPurchaseReceipt productReceipt in receipt.inAppPurchaseReceipts) {
					Debug.Log ("PRODUCTID: " + productReceipt.productID);
					Debug.Log ("PURCHASE DATE: " + productReceipt.purchaseDate);
					Debug.Log ("EXPIRATION DATE: " + productReceipt.subscriptionExpirationDate);
					Debug.Log ("CANCELDATE DATE: " + productReceipt.cancellationDate);
					
				}
			}
        }
    }
}
#endif