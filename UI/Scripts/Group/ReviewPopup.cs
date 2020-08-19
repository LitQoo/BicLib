using System;
using System.Collections;
using System.Collections.Generic;
using BicDB.Core;
using BicDB.Variable;
using BicUtil.ClassInitializer;
using BicUtil.Translate;
using BicUtil.UIFlow;
using UnityEngine;
using UnityEngine.UI;

namespace BicUtil.UI{
    public class ReviewPopup : MonoBehaviour, IClassInitializerObject, IUIFlowObject
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
        private bool isFastReviewReqeust = false;
        private Func<string, string> translator;
        #endregion

        #region ClassInitialiszer
        public void Deinitialize()
        {
            UIFlow.UIFlow.Instance.UnregisterUI(this);	
        }

        public void Initialize()
        {
            this.gameObject.SetActive(false);
            UIFlow.UIFlow.Instance.RegisterUI(this);
        }
        #endregion

        #region IUIFlowObject
        public OnCloseUIResult OnClosedUI(IUIFlowObject _fromUI, Action _finishCallback, object _parameter)
        {
            return OnCloseUIResult.DoNotWait;
        }

        public void OnOpenedUI(IUIFlowObject _fromUI, object _paramter)
        {
            Open();
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
        public void Setup(int _firstReviewSession, string _iosAppId, string _androidAppId, bool _isFastReviewRequest){
            #if UNITY_IOS
                appId = _iosAppId;
            #elif UNITY_ANDROID
                appId = _androidAppId;
            #endif
            
            firstReviewSession = _firstReviewSession;
            isWroteReview = TableService.GetProperty("isWriteReview", new BoolVariable(false));
            reviewCounting = TableService.GetProperty("reviewCount", new IntVariable(1));
            isFastReviewReqeust = _isFastReviewRequest;

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
            if(translator == null){
                translator = TranslateManager.Instance.GetText;
            }

            if(string.IsNullOrEmpty(appId) == true){
                throw new System.Exception("[ReviewPopup] Not Setup appid");
            }

            isOpend = true;
            this.gameObject.SetActive(true);
            this.mode.AsEnum = Mode.Enjoy;
        }

        public bool ShouldOpen(int _term, bool _isCouting){
            if(isOpend == true){
                return false;
            }

            #if UNITY_EDITOR
            if(openAlwaysOnEditor == true){
                return true;
            }
            #endif

            if(firstReviewSession > TableService.SessionCount){
                return false;
            }

            if(isWroteReview.AsBool == true){
                return false;
            }
            
            var _result = false;
            if(reviewCounting.AsInt > 0 && reviewCounting.AsInt % _term != 0){
                _result = false;
            }else{
                _result = true;
            }

            if(_isCouting == true){
                reviewCounting.AsInt++;
                TableService.Save();
            }

            return _result;
        }

        private void setEnjoy(){
            
            messageText.text = translator("review_enjoy");
            leftButtonText.text = translator("review_not_really");
            rightButtonText.text = translator("review_yes");
        }

        private void setFeedback(){
            messageText.text = translator("review_feedback");
            leftButtonText.text = translator("review_no");
            rightButtonText.text = translator("review_yes");
        }

        private void setReview(){

            if(isFastReviewReqeust == true){
                openReviewAndClose();
            }else{
                developer.PlayHadsUpDance();
                messageText.text = translator("review_request");
                leftButtonText.text = translator("review_later");
                rightButtonText.text = translator("review_ok");
            }
        }

        public void Close(){
            developer.StopDance();
            this.gameObject.SetActive(false);
        }

        public void SetTranslator(Func<string, string> _translator){
            translator = _translator;
        }

        private enum Mode{
            Enjoy,
            Feedback,
            Review
        } 
        #endregion
    }
}