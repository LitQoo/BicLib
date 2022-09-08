using System;
using System.Collections;
using System.Collections.Generic;
using BicDB.Core;
using BicDB.Variable;
using UnityEngine;
using UnityEngine.UI;

namespace BicUtil.UI{
    public class CheckInRewardPopup : MonoBehaviour
    {
        private const string DATE_SAVE_FORMAT = "yyyyMMddHHmmss";
        private const string DATE_CHECK_FORMAT = "yyyyMMdd";

        #region DI
        [SerializeField]
        private Color normalColor;
        [SerializeField]
        private Color todayColor;
        [SerializeField]
        private Color checkedColor;
        [SerializeField]
        private Text titleText;
        [SerializeField]
        private Text subtitleText;
        [SerializeField]
        private CheckInRewardIcon[] checkInRewardIcons;
        [SerializeField]
        private GameObject takeButton;
        [SerializeField]
        private Text todayRewardTitleText;
        [SerializeField]
        private Image todayRewardIcon;
        [SerializeField]
        private Text todayRewardText;
        [SerializeField]
        private GameObject doubleButton;
        [SerializeField]
        private Text doubleButtonText;
        #endregion

        #region Instant
        private IVariable lastCheckDate;
        private IVariable lastCheckCount;
        private Action<int, bool> takeRewardCallback;
        #endregion

        #region Logic
        public void Setup(Action<CheckInRewardIcon, int> _iconSetupFunc, string _doubleText){
            lastCheckDate = TableService.GetProperty("atDate", new StringVariable("19990101010001"));
            lastCheckCount = TableService.GetProperty("atCount", new IntVariable(0));

            if(string.IsNullOrEmpty(_doubleText) == true){
                doubleButton.SetActive(false);
            }else{
                doubleButtonText.text = _doubleText;
            }

            for(int i = 0; i < checkInRewardIcons.Length; i++){
                _iconSetupFunc(checkInRewardIcons[i], i);
            }
        }

        public bool ShouldOpen(){
            try{
                var _currentDate = DateTime.Now;
                var _lastDate = DateTime.ParseExact(lastCheckDate.AsString ,DATE_SAVE_FORMAT, null);

                if(_lastDate.CompareTo(_currentDate) > 0){
                    return false;
                }

                if(isEqualDate(_currentDate, _lastDate) == true){
                    return false;
                }
            }catch{
                return false;
            }

            return true;
        }

        public void Open(Action<int, bool> _takeRewardCallback){
            if(ShouldOpen() == false){
                throw new System.Exception("Check-in reward can't open");
            }

            takeRewardCallback = _takeRewardCallback;

            var _currentDate = DateTime.Now;
            var _lastDate = DateTime.ParseExact(lastCheckDate.AsString ,DATE_SAVE_FORMAT, null);
            var _lastTomorrow = _lastDate.AddDays(1);

            if(isEqualDate(_lastTomorrow, _currentDate) == true){
                lastCheckCount.AsInt++;
            }else{
                lastCheckCount.AsInt = 1;
            }

            for(int i = 0; i < checkInRewardIcons.Length; i++){
                var _icon = checkInRewardIcons[i];
                int _day = i + 1;
                _icon.EnableCheck(i < lastCheckCount.AsInt);

                if(_day == lastCheckCount.AsInt){
                    _icon.SetColor(todayColor);
                    todayRewardIcon.sprite = _icon.RewardIconImage.sprite;
                    todayRewardIcon.rectTransform.sizeDelta = _icon.RewardIconImage.rectTransform.sizeDelta;
                    todayRewardText.text = _icon.RewardString;
                }else if(_day < lastCheckCount.AsInt){
                    _icon.SetColor(checkedColor);
                }else{
                    _icon.SetColor(normalColor);
                }
            }

            lastCheckDate.AsString = _currentDate.ToString(DATE_SAVE_FORMAT);
            TableService.Save();

            this.gameObject.SetActive(true);
        }
        
        public void Take(){
            callback(false);
            resetIfFull();
            this.gameObject.SetActive(false);
        }

        public void TakeDouble(){
            callback(true);
            resetIfFull();
            this.gameObject.SetActive(false);
        }

        private void callback(bool _isDouble){
            var _dayIndex = lastCheckCount.AsInt - 1;
            takeRewardCallback(_dayIndex, _isDouble);
            Analytics.Analytics.Event("check_in_reward", new Dictionary<string, object> {
                {
                    "isDouble",
                    _isDouble
                },
                {
                    "dayIndex",
                    _dayIndex
                }
            });
        }

        private void resetIfFull(){
            if (lastCheckCount.AsInt >= checkInRewardIcons.Length)
            {
                reset(0);
            }
        }

        private bool isEqualDate(DateTime _date1, DateTime _date2){
            return _date1.ToString(DATE_CHECK_FORMAT) == _date2.ToString(DATE_CHECK_FORMAT);
        }

        private void reset(int _count){
            lastCheckCount.AsInt = _count;
            lastCheckDate.AsString = DateTime.Now.ToString(DATE_SAVE_FORMAT);
            TableService.Save();
        }
        #endregion
    }
}
