using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BicDB.Variable;
using BicUtil.Ads;
using BicUtil.Translate;
using BicUtil.Tween;
using UnityEngine;
using UnityEngine.UI;

namespace BicUtil.LocalAds{
    public class LocalAdsController : MonoBehaviour, IAdsPlatform
    {
        [SerializeField]
        private UnityEngine.UI.Text title;
        [SerializeField]
        private UnityEngine.UI.Image icon;
        [SerializeField]
        private UnityEngine.UI.Image screenshot;


        [SerializeField]
        private UnityEngine.UI.Text noAdsMessage;
        [SerializeField]
        private UnityEngine.UI.Text noAdsButtonTitle;
        [SerializeField]
        private UnityEngine.UI.Text noAdsPrice;
        [SerializeField]
        private UnityEngine.UI.Text bigjamMessage;
        [SerializeField]
        private UnityEngine.UI.Text downloadText;

        [SerializeField]
        private UnityEngine.UI.Text appInfoReviewTitle;
        [SerializeField]
        private UnityEngine.UI.Text appInfoDownloadTitle;
        [SerializeField]
        private UnityEngine.UI.Text appInfoPriceTitle;
        [SerializeField]
        private UnityEngine.UI.Text appInfoReview;
        [SerializeField]
        private UnityEngine.UI.Text appInfoDownload;
        [SerializeField]
        private UnityEngine.UI.Text appInfoPrice;


        [SerializeField]
        private GameObject noAdsLayer;
        [SerializeField]
        private UnityEngine.UI.Text closeText;
        [SerializeField]
        private UnityEngine.UI.Button closeButton;
        [SerializeField]
        private List<LocalAdsData> AdsList;
        [SerializeField]
        private List<MaskableGraphic> BackgroundColorTargets;
        [SerializeField]
        private List<MaskableGraphic> ButtonColorTargets;
        [SerializeField]
        private RemoveAdsController removeAds;
        
        private IntVariable SelectedIndex = new IntVariable(0);
        private LocalAdsData selectedData{get=>this.AdsList[this.SelectedIndex.AsInt];}

        public string TermsURL => "https://bigjamgames.com/privacy_en.html";

        private Action purchaseAction;
        private Action closeAction = null;

        private RectTransform rectTransform = null;
        private bool isInit = false;

        #if UNITY_EDITOR
        private static LocalAdsController instance;

        
        [UnityEditor.MenuItem("BicLib/LocalAds/TestInterstitial", false, 500)]
        private static void TestInterstitial(){
            if(instance == null){
                return;
            }

            AdsManager.Instance.CallOnBeforePlayAdsCallback(AdsType.Interstital, AdsType.Interstital);
            instance.ShowInterstitial(AdsType.RewardBase, _result=>{
                AdsManager.Instance.CallOnAfterPlayAdsCallback(AdsType.Interstital, AdsType.Interstital, _result);
            });
        }

        [UnityEditor.MenuItem("BicLib/LocalAds/TestRV", false, 501)]
        private static void TestRV(){
            if(instance == null){
                return;
            }
            
            AdsManager.Instance.CallOnBeforePlayAdsCallback(AdsType.RewardBase, AdsType.RewardBase);
            instance.ShowRewardBased(AdsType.RewardBase, _result=>{
                AdsManager.Instance.CallOnAfterPlayAdsCallback(AdsType.RewardBase, AdsType.RewardBase, _result);
            });
        }
        #endif
        
        public void Init(){
            #if UNITY_EDITOR
            instance = this;
            #endif

            if(isInit == true){
                return;
            }

            isInit = true;

            this.noAdsMessage.text = LocalAdsService.Translator.Get("noads_pr");
            this.noAdsButtonTitle.text = LocalAdsService.Translator.Get("noads_title");
            this.bigjamMessage.text = LocalAdsService.Translator.Get("bigjam_message");
            
            this.appInfoPrice.text = LocalAdsService.Translator.Get("free");
            this.downloadText.text = LocalAdsService.Translator.Get("download_title");
            
            this.appInfoDownloadTitle.text = LocalAdsService.Translator.Get("download");
            this.appInfoPriceTitle.text = LocalAdsService.Translator.Get("price");
            this.appInfoReviewTitle.text = LocalAdsService.Translator.Get("review");

            SelectedIndex.Subscribe(selectAds, true);
        }

        private void Awake(){
            #if UNITY_EDITOR
            instance = this;
            #endif
            Init();
        }

        private void selectAds(IVariableReadOnly _index)
        {
            foreach(var _target in BackgroundColorTargets){
                _target.color = selectedData.BackgroundColor;
            }

            foreach(var _target in ButtonColorTargets){
                _target.color = selectedData.ButtonColor;
            }

            this.title.text = selectedData.Title;
            this.appInfoReview.text = selectedData.Review;
            this.appInfoDownload.text = selectedData.Donwload;
            this.icon.sprite = selectedData.Icon;
            this.screenshot.sprite = selectedData.Screenshot;
        }

        public void Down(){
            BicUtil.Analytics.Analytics.Event("LocalAds_Down", new(){{"AppId", selectedData.GooglePlayAppId}});
            BicUtil.PublishingUtil.PublishingUtil.OpenStore(selectedData.GooglePlayAppId, selectedData.AppStoreAppId, "LocalAds");
        }

        public void Next(){
            if(SelectedIndex.AsInt + 1 < this.AdsList.Count){
                SelectedIndex.AsInt++;
            }else{
                SelectedIndex.AsInt = 0;
            }
        }

        public void Prev(){
            if(SelectedIndex.AsInt >= 1){
                SelectedIndex.AsInt--;
            }else{
                SelectedIndex.AsInt = this.AdsList.Count - 1;
            }
        }

        public void Open(int _enableCloseDealy, bool _noAdsEnable)
        {
            clearBackKey();
            selectAds();
            BicUtil.Analytics.Analytics.Event("LocalAds_Open", new() { { "CloseDelay", _enableCloseDealy }, {"AppId", selectedData.GooglePlayAppId}});


            closeButton.interactable = false;
            
            #if UNITY_EDITOR
            closeButton.interactable = true;
            #endif
            closeText.text = _enableCloseDealy.ToString();
            BicTween.Interval(1f, _enableCloseDealy).SubscribeRepeat((_tween, _count) =>
            {
                closeText.text = (_enableCloseDealy - _count).ToString();
            }).SubscribeComplete(() =>
            {
                closeText.text = "X";
                closeButton.interactable = true;
            });

            this.gameObject.SetActive(true);
            
            if(removeAds != null){
                this.removeAds.SetActive(_noAdsEnable && this.enableNoAdsLayer);
            }else if(noAdsLayer != null){
                this.noAdsLayer.SetActive(_noAdsEnable && this.enableNoAdsLayer);
            }

            if(rectTransform == null){
                rectTransform = this.gameObject.GetComponent<RectTransform>();
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        }

        private void clearBackKey()
        {
            if(BicUtil.UIFlow.UIFlow.IsCreated == true){
                DebugForEditor.Log("disable uiflow backkey");
                BicUtil.UIFlow.UIFlow.Instance.BackupBackKeyAction();
            }

            if(BicUtil.PageService.PageManager.IsCreated == true){
                if(BicUtil.PageService.PageManager.Instance.CurrentController != null){
                    DebugForEditor.Log("disable pagemanager backkey");
                    BicUtil.PageService.PageManager.Instance.CurrentController.SetActiveBackKey(false);
                }
            }
        }

        private void restoreBackKey()
        {
            if(BicUtil.UIFlow.UIFlow.IsCreated == true){
                DebugForEditor.Log("restore uiflow backkey");
                BicUtil.UIFlow.UIFlow.Instance.RestoreBackKeyAction();
            }

            if(BicUtil.PageService.PageManager.IsCreated == true){
                if(BicUtil.PageService.PageManager.Instance.CurrentController != null){
                    DebugForEditor.Log("restore pagemanager backkey");
                    BicUtil.PageService.PageManager.Instance.CurrentController.SetActiveBackKey(true);
                }
            }
        }

        private void selectAds()
        {
            if(forcedViewCount > 0){
                this.SelectedIndex.AsInt = forcedViewAdsIndex;
                forcedViewAdsIndex++;
                forcedViewCount--;
                Debug.Log("selectAds first "+ forcedViewAdsIndex + "/" + forcedViewCount);
                return;
            }
            
            var _selectedAds = this.AdsList.Where(_row =>
            {
                if (_row.GooglePlayAppId == Application.identifier)
                {
                    return false;
                }

                if (string.IsNullOrEmpty(_row.ServiceLanguage) == true)
                {
                    return true;
                }

                if (_row.ServiceLanguage.ToLower().Contains(Application.systemLanguage.ToString().ToLower()))
                {
                    return true;
                }

                return false;
            }).ToList().Shuffle().First();

            this.SelectedIndex.AsInt = this.AdsList.IndexOf(_selectedAds);
        }
        

        public void Close(){
            restoreBackKey();
            this.gameObject.SetActive(false);
            closeText.text = "X";
            if(closeAction != null){
                closeAction();
                closeAction = null;
            }

        }

        public void PurchaseNoAds(){
            if(purchaseAction != null){
                BicUtil.Analytics.Analytics.Event("LocalAds_purchase_action", new(){{"AppId", selectedData.GooglePlayAppId}});
                purchaseAction();
            }else if(removeAds != null){
                removeAds.BuyProduct();
            }
        }

        public void SetPurchaseAction(Action _purchaseAction){
            this.purchaseAction = _purchaseAction;
        }

        public void SetNoAdsPrice(string _noAdsPrice){
            this.noAdsPrice.text = _noAdsPrice;
        }

        public void SetNoAdsPrice(IVariableReadOnly _noAdsPrice){
            this.noAdsPrice.text = _noAdsPrice.AsString;
        }

        public float interstitialRate = 1f;
        public int interstitialShowingTime = 5;
        public bool enableNoAdsLayer = false;
        public float rvRate = 1f;
        public int rvShowingTime = 15;

        public void Setup(float _interstitialRate, int _interstitialShowingTime, float _rvRate, int _rvShowingTime, bool _enableNoAdsLayer){
            
            #if UNITY_EDITOR
            instance = this;
            #endif

            this.interstitialRate = _interstitialRate;
            this.interstitialShowingTime = _interstitialShowingTime;
            this.rvRate = _rvRate;
            this.rvShowingTime = _rvShowingTime;
            this.enableNoAdsLayer = _enableNoAdsLayer;
        }

        public void EnableNoAdsLayer(bool _enabled){
            this.enableNoAdsLayer = _enabled;
        }

		public void EnableNoAdsLayer(IVariableReadOnly _isNoAds){
			this.EnableNoAdsLayer(_isNoAds.AsBool == false);
		}

        #region IAdsPlatform
        public void LoadInterstitial(object _adsPlacement)
        {
            
        }

        public bool IsReadyInterstitial(object _adsPlacement)
        {
            if(forcedViewCount > 0){
                return true;
            }

            if(UnityEngine.Random.Range(0f, 1f) <= interstitialRate){
                return true;
            }

            return false;
        }

        public void ShowInterstitial(object _adsPlacement, Action<AdsResult> _callback)
        {
            closeAction = ()=>{
                if(_callback != null){
                    _callback(AdsResult.Finished);
                }
            };

            this.Open(interstitialShowingTime, enableNoAdsLayer);

        }

        public void LoadRewardBased(object _adsPlacement)
        {
            
        }

        public bool IsReadyRewardBased(object _adsPlacement)
        {
            var _rate = UnityEngine.Random.Range(0f, 1f);
            
            if(_rate <= rvRate){
                return true;
            }

            return false;
        }

        
        public void ShowRewardBased(object _adsPlacement, Action<AdsResult> _callback)
        {

            closeAction = ()=>{
                if(_callback != null){
                    _callback(AdsResult.Finished);
                }
            };
            
            this.Open(rvShowingTime, false);
        }

        public bool IsReadyBanner(object _adsPlacement)
        {
            return false;
        }

        public IAdsBanner CreateBanner(object _adsPlacement, Color _backColor, Action<IAdsBanner> _onLoadBannerAction)
        {
            return null;
        }

        public void SetUserConsent(bool _isEnabled)
        {
            
        }

        public void Initialize(Action _callback)
        {
            
        }

        private int forcedViewCount = -1;
        private int forcedViewAdsIndex = 0;
        public void SetForcedViewCount(int _count){
            forcedViewCount = _count;
        }

        public void SetupRemoveAds(string _noAdsProductId, BoolVariable _isNoAds){
            removeAds.Setup(_noAdsProductId, _isNoAds);
        }
        #endregion
    }

    public static class LocalAdsService{
        public static Translator Translator{get; private set;} = new Translator("LocalAdsTranslate", "LocalAdsTranslateText");
    }
}
