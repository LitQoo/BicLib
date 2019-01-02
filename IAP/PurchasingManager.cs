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

namespace BicUtil.Purchasing{
	public enum PurchasingResult{
		Complete,
		Refunded,
		NotInitialized,
		NotAvailable,
		Failed,
		Unknown
	}

	public class PurchasingManager<PRODUCTTYPE> : SingletonBase<PurchasingManager<PRODUCTTYPE>>, IStoreListener where PRODUCTTYPE : struct {
		// private List<ProductInfo> products = new List<ProductInfo>();
		public TableContainer<ProductModel<PRODUCTTYPE>> ProductTable = new TableContainer<ProductModel<PRODUCTTYPE>>("Puma");
		public bool isLoadedProductTable = false;
		public void AddProduct(PRODUCTTYPE _idType, string _id, ProductType _productType, int _value){
			if(isLoadedProductTable == false){
				ProductTable.SetStorage(FileStorage.GetInstance());
				ProductTable.Load(null, new FileStorageParameter("purchase"));
				isLoadedProductTable = true; 
			}

			var _product = GetProduct(_idType);

			if(_product == null){
				_product = new ProductModel<PRODUCTTYPE>(_idType, _id, _productType, _value);
				ProductTable.Add(_product);
			}

			_product.ProductType.AsEnum = _productType;
		}


		public override void Initialize() 
		{
			if(ProductTable.Count == 0){
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

			
			foreach(var _product in ProductTable){
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
			#endif

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
			catch (Exception e)
			{
				// ... by reporting any unexpected exception for later diagnosis.
				if(buyCallback != null){
					buyCallback(PurchasingResult.Unknown);
					buyCallback = null;
				}
			}
		}

		// 애플로 출시할때는 이 코드를 추가해야하나봄??? 
		// Restore purchases previously made by this customer. Some platforms automatically restore purchases. Apple currently requires explicit purchase restoration for IAP.
		public void RestorePurchases()
		{
			// If Purchasing has not yet been set up ...
			if (!IsInitialized())
			{
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
				apple.RestoreTransactions((result) => {
					// The first phase of restoration. If no more responses are received on ProcessPurchase then no purchases are available to be restored.
					Debug.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");
				});
			}
			// Otherwise ...
			else
			{
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
			
			return ProductTable.FirstOrDefault<ProductModel<PRODUCTTYPE>>(_row=>Enum.Equals(_row.IdType.AsEnum, _idType));
		}

		private ProductModel<PRODUCTTYPE> getProduct(string _id){
			return ProductTable.FirstOrDefault(_row=>_row.Id.AsString == _id);
		}
		
		
		//  
		// --- IStoreListener
		//
		
		public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
		{
			// Purchasing has succeeded initializing. Collect our Purchasing references.
			Debug.Log("OnInitialized: PASS");
			
			// Overall Purchasing system, configured with products for this application.
			m_StoreController = controller;
			// Store specific subsystem, for accessing device-specific store features.
			m_StoreExtensionProvider = extensions;

			foreach(var _product in m_StoreController.products.all){
				Debug.Log("is availeble : " + _product.availableToPurchase.ToString() + "/" + _product.hasReceipt.ToString());

				try{
					if(_product.hasReceipt){
						if(buyCallback != null){
							var _result = checkRecipt(_product.definition.id, _product.receipt);
							if(_result == PurchasingResult.Complete){
								completePurchase(_product.definition.id);
							}else if(_result == PurchasingResult.Refunded){
								completeRefund(_product.definition.id);
							}
						}
					}
				}catch(Exception e){
					Debug.Log("not support product " + _product.definition.id);
				}
			}

			Debug.Log("OnInitialized: Finished");
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
                _productInfo.Value.AsInt += 1;
            }
            else if (_productInfo.ProductType.AsEnum == ProductType.NonConsumable)
            {
                _productInfo.Value.AsInt = 1;
            }

			ProductTable.Save();
        }
		
		private void completeRefund(string _id)
        {
            ProductModel<PRODUCTTYPE> _productInfo = getProduct(_id);
            if (_productInfo.ProductType.AsEnum == ProductType.Consumable)
            {
                _productInfo.Value.AsInt -= 1;
            }
            else if (_productInfo.ProductType.AsEnum == ProductType.NonConsumable)
            {
                _productInfo.Value.AsInt = 0;
            }

			ProductTable.Save();
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
	}
}
#endif