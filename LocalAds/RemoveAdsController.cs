using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BicUtil.MVCSystem;
using BicDB.Variable;
using BicUtil.Purchasing;
using BicUtil.Analytics;

namespace BicUtil.LocalAds{
	public class RemoveAdsController : MonoBehaviour
	{
		#region inapp
		[SerializeField]
		private GameObject[] disableObjectOnPurchased;
		[SerializeField]
		private GameObject restoreButton;
		[SerializeField]
		private UnityEngine.UI.Text[] priceTexts;
		[SerializeField]
		private UnityEngine.UI.Text[] noAdsButtonTitles;
		[SerializeField]
		private UnityEngine.UI.Text[] noAdsButtonMessages;
		
		private string noAdsProductId = "";
		private ModelBinder binder = new ModelBinder();
		private BoolVariable isNoAds = null;

		public void Setup(string _noAdsProductId, BoolVariable _isNoAds){
			this.noAdsProductId = _noAdsProductId;
			binder.BindModelToController(PurchasingService.GetProduct(noAdsProductId).PriceString, setNoAdsOption, true);
			
			isNoAds = _isNoAds;

			if(isNoAds == null){
				binder.BindModelToController(PurchasingService.GetProduct(noAdsProductId).PurchaseCount, _count=>{
					this.enableObject(_count.AsInt > 0);
				}, true);
			}else{
				binder.BindModelToController(isNoAds, _isNoAds=>enableObject(_isNoAds.AsBool), true);
			}

			foreach(var _title in noAdsButtonTitles){
				_title.text = LocalAdsService.Translator.Get("noads_title");
			}
			

			foreach(var _message in this.noAdsButtonMessages){
            	_message.text = LocalAdsService.Translator.Get("noads_pr");
			}


			#if UNITY_ANDROID
			if(restoreButton != null){
				restoreButton.gameObject.SetActive(false);
			}
			#endif
		}

		private void setNoAdsOption(IVariableReadOnly _price)
		{
			foreach(var _priceText in this.priceTexts){
				_priceText.text = _price.AsString;
			}
		}

		private void enableObject(bool _isNoAds)
		{
			#if UNITY_ANDROID
			if(restoreButton != null){
				restoreButton.gameObject.SetActive(false);
			}
			#endif

			foreach(var _hideObject in disableObjectOnPurchased){
				_hideObject.SetActive(_isNoAds == false);
			}

			if(restoreButton != null && _isNoAds == true){	
				restoreButton.SetActive(false);
			}
		}

		private void OnDestroy() {
			binder.ClearBinding();
		}

		public void BuyProduct(){
			if(PurchasingService.GetProduct(noAdsProductId).PurchaseCount.AsInt == 0){
				
				PurchasingService.BuyProduct(noAdsProductId, _result=>{

				});

				Analytics.Analytics.Event("startToBuyNoAds", new Dictionary<string, object>
				{
						{ "where", "option" }
				});
			}
		}

		public void Restore(){
			PurchasingService.RestorePurchases((_result, _message)=>{});

			Analytics.Analytics.Event("RestoreNoAds", new Dictionary<string, object>
			{
					{ "where", "option" }
			});
		} 
		#endregion
	}
}