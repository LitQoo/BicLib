using System;
using BicDB.Variable;
using BicUtil.ServerTime;
using UnityEngine;

namespace BicUtil.TimeRefill{
    public class TimeRefillManager : MonoBehaviour
    {
        #region Field
        [NonSerialized]
        public int MaxCount = 5;
        [NonSerialized]
        public int RefillTime = 0;
        public int LeftRefillSecond{
            get{
                return (int)((1f - FillRate.AsFloat) * RefillTime);
            }
        }
        public string LeftRefillTime{
            get{
                if(CurrentCount.AsInt >= MaxCount){
                    return string.Empty;
                }

                var _time = this.LeftRefillSecond;
                var _sec = _time % 60;
                var _min = (_time / 60) % 60;
                var _hour = (_time / 60) / 60;

                if(_hour > 0){
                    return $"{_hour}:{_min:D2}:{_sec:D2}";
                }else{
                    return $"{_min:D2}:{_sec:D2}";
                }
            }
        }

        public bool IsMax{
            get{
                if(CurrentCount.AsInt >= MaxCount){
                    return true;
                }

                return false;
            }
        }

        public FloatVariable FillRate = new FloatVariable();
        public IVariable LastTimestamp{get;set;}
        public IVariable CurrentCount{get;set;}
        private Action onRefillCallback = null; 
        #endregion

        #region LifeCycle
        private void Update()
        {
            if(RefillTime > 0){
                refillCheck();
            }
        }

        private void OnDestroy() {
            if(this.CurrentCount != null){
                this.CurrentCount.Unsubscribe(resetTimestamp);
            }    
        }
        #endregion

        #region Logic
        public void Setup(IVariable _targetCount, int _max, int _refillTimeSec, IVariable _lastTimestamp, Action _onRefillCallback){
            onRefillCallback = _onRefillCallback;
            this.MaxCount = _max;
            this.RefillTime = _refillTimeSec;
            this.CurrentCount = _targetCount;
            this.LastTimestamp = _lastTimestamp;
            this.onRefillCallback = _onRefillCallback;

            _targetCount.Subscribe(resetTimestamp);
        }

        private void resetTimestamp(IVariable obj)
        {
            if(this.FillRate.AsFloat == 0){
                this.LastTimestamp.AsInt = (int)ServerTimeManager.Instance.TimestampUnix;
                if(onRefillCallback != null){
                    onRefillCallback();
                }
            }
        }

        private void refillCheck(){
            if(CurrentCount.AsInt >= MaxCount){
                FillRate.AsFloat = 0.0f;
                return;
            }

            float _refillValue = Mathf.Min((float)((int)ServerTimeManager.Instance.TimestampUnix - this.LastTimestamp.AsInt) / (float)RefillTime, MaxCount);
            int _refillCount = Mathf.FloorToInt(_refillValue);
            FillRate.AsFloat = ((float)(((int)(_refillValue*1000))%1000))/1000f;

            if(_refillCount > 0){
                this.CurrentCount.AsInt = Mathf.Min(this.CurrentCount.AsInt + _refillCount, MaxCount);
                this.LastTimestamp.AsInt = (int)ServerTimeManager.Instance.TimestampUnix;
                if(onRefillCallback != null){
                    onRefillCallback();
                }
            }
        }
        #endregion
    }
}