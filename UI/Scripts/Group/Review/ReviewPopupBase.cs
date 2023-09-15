using System;
using System.Collections.Generic;
using BicDB.Core;
using BicDB.Variable;
using BicUtil.Ads;
using BicUtil.Translate;
using BicUtil.Tween;
using UnityEngine;
using UnityEngine.UI;

namespace BicUtil.UI{
    public class ReviewPopupBase : MonoBehaviour, IAdsPlatform
    {
        #region DI
        [SerializeField]
        private Text messageText;
        [SerializeField]
        private Text leftButtonText;
        [SerializeField]
        private Text rightButtonText;
        [SerializeField]
        private Bigjam.Developer.DeveloperController developer;
        [SerializeField]
        private bool openAlwaysOnEditor = false;
        #endregion

        #region Instant
        private EnumVariable<Mode> mode = new EnumVariable<Mode>(Mode.Enjoy);
        private string appId = "";
        private bool isOpend = false;
        private IVariable reviewCounting;
        private IVariable isWroteReview;
        private int firstReviewSession = 1;
        private int showTerm = 1;
        private bool isFastReviewReqeust = false;
        private Translator translator;
        public Action OnClose = null;
        #endregion

        #region ClassInitialiszer

        public void Init()
        {

            translator = new Translator("ReviewTranslate", "ReviewTranslateText");
            this.gameObject.SetActive(false);

            #if UNITY_EDITOR
            BicTween.Delay(0.5f).SubscribeComplete(()=>{
                var _check = translator.Get("review_enjoy");
                if(string.IsNullOrEmpty(_check) == true){
                    Debug.LogError("Setup translate for review");
                }
            });
            #endif
        }
        #endregion

        #region Event
        private void Awake(){

        }

        public void OnClickedLeftButton(){
            switch(this.mode.AsEnum){
                case Mode.Enjoy:
                    mode.AsEnum = Mode.Feedback;
                    this.isWroteReview.AsBool = true;
                    reviewCounting.AsInt = -20;
                    TableService.Save();
                break;
                case Mode.Feedback:
                    Close();
                    BicUtil.Analytics.Analytics.Event("ReviewPopup", new Dictionary<string, object> {
                        {
                            "Result",
                            "Feedback_close"
                        }
                    });
                break;
                case Mode.Review:
                    Close();
                    BicUtil.Analytics.Analytics.Event("ReviewPopup", new Dictionary<string, object> {
                        {
                            "Result",
                            "Review_close"
                        }
                    });
                break;
            }
        }

        public void OnclickedRightButton(){
            switch(this.mode.AsEnum){
                case Mode.Enjoy:
                    mode.AsEnum = Mode.Review;
                break;
                case Mode.Feedback:
                    PublishingUtil.PublishingUtil.OpenFacebookPage();
                    Close();
                    BicUtil.Analytics.Analytics.Event("ReviewPopup", new Dictionary<string, object> {
                        {
                            "Result",
                            "Feedback"
                        }
                    });
                break;
                case Mode.Review:
                    openReviewAndClose();
                    break;
            }
        }

        private void openReviewAndClose()
        {
            PublishingUtil.PublishingUtil.OpenReview(this.appId, this.appId);
            this.isWroteReview.AsBool = true;
            TableService.Save();
            Close();
            BicUtil.Analytics.Analytics.Event("ReviewPopup", new Dictionary<string, object> {
                        {
                            "Result",
                            "Yes"
                        }
                    });
        }
        #endregion

        #region Logic
        public void Setup(int _firstReviewSession, int _showTerm, string _iosAppId, string _androidAppId, bool _isFastReviewRequest){
            #if UNITY_IOS
                appId = _iosAppId;
            #elif UNITY_ANDROID
                appId = _androidAppId;
            #endif

            if(string.IsNullOrEmpty(_iosAppId) == true || string.IsNullOrEmpty(_androidAppId)){
                Debug.LogError("[ReviewPopup] Not Setup appid");
            }
            
            firstReviewSession = _firstReviewSession;
            isWroteReview = TableService.GetProperty("isWriteReview", new BoolVariable(false));
            reviewCounting = TableService.GetProperty("reviewCount", new IntVariable(1));
            isFastReviewReqeust = _isFastReviewRequest;
            showTerm = _showTerm;

            mode.Subscribe(setMode);
        }

        private void setMode(IEnumVariable<Mode> _mode)
        {
            switch(_mode.AsEnum){
                case Mode.Enjoy:
                setEnjoy();
                break;
                case Mode.Feedback:
                setFeedback();
                break;
                case Mode.Review:
                setReview();
                break;
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(this.messageText.rectTransform);
        }

        public void Open(){
            if(string.IsNullOrEmpty(appId) == true){
                #if UNITY_EDITOR
                Debug.LogError("[ReviewPopup] Not Setup appid");
                #else
                throw new System.Exception("[ReviewPopup] Not Setup appid");
                #endif
            }

            isOpend = true;
            this.gameObject.SetActive(true);
            this.mode.AsEnum = Mode.Enjoy;
        }

        public bool ShouldOpen(bool _isCouting){
            if(isOpend == true){
                DebugForEditor.Log("[ReviewPopup] ShouldOpen false - isOpend true");
                return false;
            }

            #if UNITY_EDITOR
            if(openAlwaysOnEditor == true){
                DebugForEditor.Log("[ReviewPopup] ShouldOpen false - openAlwaysOnEditor true");
                return true;
            }
            #endif

            if(firstReviewSession > TableService.SessionCount){
                DebugForEditor.Log("[ReviewPopup] ShouldOpen false - firstReviewSession > TableService.SessionCount");
                return false;
            }

            if(isWroteReview.AsBool == true){
                DebugForEditor.Log("[ReviewPopup] ShouldOpen false - isWroteReview.AsBool == true");
                return false;
            }
            
            var _result = false;
            if(reviewCounting.AsInt > 0 && reviewCounting.AsInt % this.showTerm != 0){
                DebugForEditor.Log("[ReviewPopup] ShouldOpen false - reviewCounting.AsInt > 0 && reviewCounting.AsInt % this.showTerm != 0");
                _result = false;
            }else{
                DebugForEditor.Log("[ReviewPopup] ShouldOpen true");
                _result = true;
            }

            if(_isCouting == true){
                reviewCounting.AsInt++;
                TableService.Save();
            }

            return _result;
        }

        private void setEnjoy(){
            
            messageText.text = translator.Get("review_enjoy");
            leftButtonText.text = translator.Get("review_not_really");
            rightButtonText.text = translator.Get("review_yes");
            developer.PlayHadsUpDance();
        }

        private void setFeedback(){
            messageText.text = translator.Get("review_feedback");
            leftButtonText.text = translator.Get("review_no");
            rightButtonText.text = translator.Get("review_yes");
        }

        private void setReview(){

            if(isFastReviewReqeust == true){
                openReviewAndClose();
            }else{
                messageText.text = translator.Get("review_request");
                leftButtonText.text = translator.Get("review_later");
                rightButtonText.text = translator.Get("review_ok");
            }
        }

        public void Close(){
            developer.StopDance();
            this.gameObject.SetActive(false);
            if(OnClose != null){
                OnClose();
            }

            if(onCloseByAds != null){
                onCloseByAds(AdsResult.Finished);
                onCloseByAds = null;
            }
        }

        private enum Mode{
            Enjoy,
            Feedback,
            Review
        } 
        #endregion

        #region IAdsPlatform
        public string TermsURL => string.Empty;
        object[] adsTypesForShow = null;

        public IAdsBanner CreateBanner(object _adsPlacement, Color _backColor, Action<IAdsBanner> _onLoadBannerAction)
        {
            throw new NotImplementedException();
        }

        public bool IsReadyBanner(object _adsPlacement)
        {
            return false;
        }

        public void SetAdsPlacementForShow(object[] _adsPlacements){
            this.adsTypesForShow = _adsPlacements;
        }

        public bool IsReadyInterstitial(object _adsPlacement)
        {
            if(adsTypesForShow != null){
                bool _containKey = false;
                for(int i = 0; i < adsTypesForShow.Length; i++){
                    if(adsTypesForShow[i].Equals(_adsPlacement)){
                        _containKey = true;
                        break;
                    }
                }

                if(_containKey == false){
                    DebugForEditor.Log("[ReviewPopup] IsReadyInterstitial false - not contain " +_adsPlacement.ToString());
                    return false;
                }
            }

            var _result = this.ShouldOpen(true);
            DebugForEditor.Log("[ReviewPopup] IsReadyInterstitial - " + _result.ToString());

            return _result;
        }

        public bool IsReadyRewardBased(object _adsPlacement)
        {
            return false;
        }

        public void LoadInterstitial(object _adsPlacement)
        {
        }

        public void LoadRewardBased(object _adsPlacement)
        {

        }

        public void SetUserConsent(bool _isEnabled)
        {

        }

        public void Initialize(Action _callback)
        {
            _callback();
        }


        private Action<AdsResult> onCloseByAds = null;
        public void ShowInterstitial(object _adsPlacement, Action<AdsResult> _callback)
        {
            this.onCloseByAds = _callback;
            this.Open();
        }

        public void ShowRewardBased(object _adsPlacement, Action<AdsResult> _callback)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}