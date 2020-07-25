using System;
using System.Collections;
using System.Collections.Generic;
using BicDB.Core;
using BicDB.Variable;
using BicUtil.Translate;
using UnityEngine;
using UnityEngine.UI;

namespace BicUtil.UI{
    public class ReviewPopup : MonoBehaviour
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
        #endregion

        #region Instant
        private EnumVariable<Mode> mode = new EnumVariable<Mode>(Mode.Enjoy);
        private string appId = "";
        private bool isOpend = false;
        private IVariable reviewCounting;
        private IVariable isWroteReview;
        private int firstReviewSession = 1;
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
                break;
            }
        }
        #endregion

        #region Logic
        public void Setup(int _firstReviewSession, string _iosAppId, string _androidAppId){
            #if UNITY_IOS
                appId = _iosAppId;
            #elif UNITY_ANDROID
                appId = _androidAppId;
            #endif
            
            firstReviewSession = _firstReviewSession;
            isWroteReview = TableService.GetProperty("isWriteReview", new BoolVariable(false));
            reviewCounting = TableService.GetProperty("reviewCount", new IntVariable(1));

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
            
            messageText.text = TranslateManager.Instance.GetText("review_enjoy");
            leftButtonText.text = TranslateManager.Instance.GetText("review_not_really");
            rightButtonText.text = TranslateManager.Instance.GetText("review_yes");
        }

        private void setFeedback(){
            messageText.text = TranslateManager.Instance.GetText("review_feedback");
            leftButtonText.text = TranslateManager.Instance.GetText("review_no");
            rightButtonText.text = TranslateManager.Instance.GetText("review_yes");
        }

        private void setReview(){
            developer.PlayHadsUpDance();
            messageText.text = TranslateManager.Instance.GetText("review_request");
            leftButtonText.text = TranslateManager.Instance.GetText("review_later");
            rightButtonText.text = TranslateManager.Instance.GetText("review_ok");
        }

        public void Close(){
            developer.StopDance();
            this.gameObject.SetActive(false);
        }

        private enum Mode{
            Enjoy,
            Feedback,
            Review
        } 
        #endregion
    }
}