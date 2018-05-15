using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using BicUtil.SingletonBase;
using UnityEngine.Purchasing.Security;

namespace BicUtil.Purchasing{
	public enum PurchasingResult{
		Complete,
		Refunded,
		NotInitialized,
		NotAvailable,
		Failed,
		Unknown
	}

	public struct ProductInfo{
		public string Id;
		public IDs IdsForStore;
		public ProductType Type;
		public Action<PurchasingResult> Callback;

		public ProductInfo(string _id, ProductType _type, Action<PurchasingResult> _callback){
			Id = _id;
			IdsForStore = new IDs(){{ _id, AppleAppStore.Name },{ _id,  GooglePlay.Name },};
			Type = _type;
			Callback = _callback;
		}
	}

	public class PurchasingManager : SingletonBase<PurchasingManager>, IStoreListener {
		private List<ProductInfo> products = new List<ProductInfo>();

		public void AddProduct(string _id, ProductType _type, Action<PurchasingResult> _callback){
			products.Add(new ProductInfo(_id, _type, _callback));
		}


		public void Initialize() 
		{
			if(products.Count == 0){
				throw new Exception("No Products");
			}

			// If we have already connected to Purchasing ...
			if (IsInitialized())
			{
				// ... we are done here.
				return;
			}



			// Create a builder, first passing in a suite of Unity provided stores.
			var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

			
			foreach(var _product in products){
				builder.AddProduct(_product.Id, _product.Type, _product.IdsForStore);
			}
			
			UnityPurchasing.Initialize(this, builder);
		}

		public void BuyProduct(string productId)
		{
			ProductInfo _product = getProductInfo(productId);

			#if UNITY_EDITOR
			_product.Callback(PurchasingResult.Complete);
			return;
			#endif

			// If the stores throw an unexpected exception, use try..catch to protect my logic here.
			try
			{
				// If Purchasing has been initialized ...
				if (IsInitialized())
				{
					// ... look up the Product reference with the general product identifier and the Purchasing system's products collection.
					Product product = m_StoreController.products.WithID(productId);
					
					// If the look up found a product for this device's store and that product is ready to be sold ... 
					if (product != null && product.availableToPurchase)
					{
						m_StoreController.InitiatePurchase(product);
					}
					// Otherwise ...
					else
					{
						// ... report the product look-up failure situation  
						_product.Callback(PurchasingResult.NotAvailable);
					}
				}
				// Otherwise ...
				else
				{
					// ... report the fact Purchasing has not succeeded initializing yet. Consider waiting longer or retrying initiailization.
					_product.Callback(PurchasingResult.NotInitialized);
				}
			}
			// Complete the unexpected exception handling ...
			catch (Exception e)
			{
				// ... by reporting any unexpected exception for later diagnosis.
				_product.Callback(PurchasingResult.Unknown);
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

		private ProductInfo getProductInfo(string _id){
			foreach(var _product in products){
				if(string.Equals(_id, _product.Id, StringComparison.Ordinal)){
					return _product;
				}
			}

			throw new Exception("Product " + _id + " Not Found");
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
				Debug.Log("is availeble : " + _product.availableToPurchase.ToString());

				try{
					ProductInfo _productInfo = getProductInfo(_product.definition.id);
					_productInfo.Callback(checkRecipt(_product.definition.id, _product.receipt));
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

			ProductInfo _productInfo = getProductInfo(args.purchasedProduct.definition.id);
			
			_productInfo.Callback(checkRecipt(args.purchasedProduct.definition.id, args.purchasedProduct.receipt));

			return PurchaseProcessingResult.Complete;
		}


		public PurchasingResult checkRecipt(string _productId, string _recipt) {

			#if UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX  
			var validator = new CrossPlatformValidator(GooglePlayTangle.Data(), AppleTangle.Data(), Application.identifier);  
			
			try  
			{  
				var result = validator.Validate(_recipt);  
				foreach (IPurchaseReceipt productReceipt in result)  
				{  

					Debug.Log(_productId + "/" + _recipt + "/");
					if (String.Equals(productReceipt.productID, _productId, StringComparison.Ordinal))  
					{  
						GooglePlayReceipt google = productReceipt as GooglePlayReceipt;  
						if (google != null)  
						{  
							Debug.Log(google.purchaseState.ToString());
							switch (google.purchaseState)  
							{  
								case GooglePurchaseState.Purchased:  
									return PurchasingResult.Complete;
								case GooglePurchaseState.Cancelled:  
								case GooglePurchaseState.Refunded:  
									return PurchasingResult.Refunded;
							}  
						}else{
							return PurchasingResult.Unknown;
						}
					}  
				}  

				return PurchasingResult.Unknown;
			}  
			catch (IAPSecurityException)  
			{  
				return PurchasingResult.Unknown;
			}  
			#endif  
		}

		
		
		public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
		{
			var _product = getProductInfo(product.definition.id);

			_product.Callback(PurchasingResult.Failed);
			// A product purchase attempt did not succeed. Check failureReason for more detail. Consider sharing this reason with the user.
			Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}",product.definition.storeSpecificId, failureReason));
		}
	}
}