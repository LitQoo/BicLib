#if BICUTIL_IAP
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BicUtil.MVCSystem;
using BicDB.Variable;
using BicUtil.Purchasing;
using BicUtil.Analytics;
using BicUtil.UI;

namespace BicUtil.LocalAds{
	public class RemoveAdsController : MonoBehaviour
	{
		#region inapp
		[SerializeField]
		private GameObject[] disableObjectOnPurchased;
		[SerializeField]
		private GameObject restoreButton;
		[SerializeField]
		private UnityEngine.UI.Text[] restoreTitles;
		[SerializeField]
		private UnityEngine.UI.Text[] priceTexts;
		[SerializeField]
		private UnityEngine.UI.Text[] noAdsButtonTitles;
		[SerializeField]
		private UnityEngine.UI.Text[] noAdsButtonMessages;
		[SerializeField]
		private CommonPopup commonPopup;
		[SerializeField]
		private LocalAdsController localAds;

		
		private string noAdsProductId = "";
		private ModelBinder binder = new ModelBinder();
		private BoolVariable isNoAds = null;
		private bool isPurchasedNoAds = false;

		public void Setup(string _noAdsProductId, BoolVariable _isNoAds){
			this.noAdsProductId = _noAdsProductId;
			binder.BindModelToController(PurchasingService.GetProduct(noAdsProductId).PriceString, setNoAdsOption, true);
			
			isNoAds = _isNoAds;

			if(isNoAds == null){
				binder.BindModelToController(PurchasingService.GetProduct(noAdsProductId).PurchaseCount, _count=>{
					this.enableObject(_count.AsInt > 0);
					isPurchasedNoAds = _count.AsInt > 0;
				}, true);
			}else{
				binder.BindModelToController(isNoAds, _isNoAds=>{enableObject(_isNoAds.AsBool);isPurchasedNoAds=_isNoAds.AsBool;}, true);
			}

			if(noAdsButtonTitles != null){
				foreach(var _title in noAdsButtonTitles){
					_title.text = LocalAdsService.Translator.Get("noads_title");
				}
			}
			
			if(this.noAdsButtonMessages != null){
				foreach(var _message in this.noAdsButtonMessages){
					_message.text = LocalAdsService.Translator.Get("noads_pr");
				}
			}


			if(this.restoreTitles != null){
				foreach(var _restoreTitle in this.restoreTitles){
					_restoreTitle.text = LocalAdsService.Translator.Get("restore");
				}
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
				Debug.Log("hideobject setactive " + (_isNoAds == false).ToString());
				_hideObject.SetActive(_isNoAds == false);
			}

			if(restoreButton != null && _isNoAds == true){	
				restoreButton.SetActive(false);
			}
		}

		private void OnDestroy() {
			binder.ClearBinding();
		}

		public void SetActive(bool _active){
			if(isPurchasedNoAds == false){
				this.gameObject.SetActive(_active);
			}else{
				this.gameObject.SetActive(false);
			}
		}

		public void BuyProduct(){
			if(PurchasingService.GetProduct(noAdsProductId).PurchaseCount.AsInt == 0){
				
				if(commonPopup != null){
					commonPopup.Dimmed("Loading..");
            		commonPopup.Open();
				}

				var _where = localAds != null && localAds.gameObject.activeSelf == true ? "localAds" : "other";
				
				PurchasingService.BuyProduct(noAdsProductId, _result=>{
					if(commonPopup != null){
						commonPopup.Close();
					}

					if (localAds != null && _result == PurchasingResult.Complete)
                    {
                        localAds.Close();
                    }

					if(_result == PurchasingResult.Complete){
						Analytics.Analytics.Event("LocalAds_purchase_success", new Dictionary<string, object>
						{
								{ "where", _where}
						});
					}
				});

				Analytics.Analytics.Event("LocalAds_purchase_try", new Dictionary<string, object>
				{
						{ "where", _where}
				});
			}else{
				Debug.LogWarning("이미 구매하였습니다");
			}
		}

		public void Restore(){
			PurchasingService.RestorePurchases((_result, _message)=>{});

			Analytics.Analytics.Event("LocalAds_restore", new Dictionary<string, object>
			{
					{ "where", "option" }
			});
		}
		#endregion
	}
}
#endif