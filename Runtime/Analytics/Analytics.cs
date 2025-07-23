using System.Runtime.CompilerServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace BicUtil.Analytics
{
    public class Analytics : BicUtil.SingletonBase.SingletonBase<Analytics>
    {
        private List<IAnalyticsLib> libList = new List<IAnalyticsLib>();
        private bool isEnable = false;

        public override void Initialize()
        {
            #if BICUTIL_ANALYTICS_APPSFLYER
            this.addService(new AppsFlyerAnalytics());
            #endif

            #if BICUTIL_ANALYTICS_FB
            this.addService(new FBAnalytics());
            #endif

            #if BICUTIL_ANALYTICS_FIREBASE
            this.addService(new FirebaseAnalytics());
            #endif

            #if BICUTIL_ANALYTICS_UNITY
            this.addService(new UnityAnalytics());
            #endif

            #if BICUTIL_ANALYTICS_ADJUST
            this.addService(new AdjustAnalytics());
            #endif
        }

        private void addService(IAnalyticsLib _lib){
            libList.Add(_lib);
            isEnable = true;
        }

        private void sendEvent(string _eventName, Dictionary<string, object> _eventData = null, int _count = 1){
            
            #if UNITY_EDITOR
                if(isEnable == false){
                    Debug.LogError("[Analytics] Analytics Lib is Not Added");
                }
                return;
            #else

            if(isEnable == false){
                return;
            }

            for(int i = 0; i < libList.Count; i++){
                libList[i].Event(_eventName, _eventData, _count);
            }
            #endif
        }

        static public void Event(string _eventName, Dictionary<string, object> _eventData = null, int _count = 1){
            #if UNITY_EDITOR
                if(_eventData == null){
                    Debug.Log("[Event] " + _eventName + "\n value : " + _count);
                }else{
                    var _param = _eventData.Select(_info=>"{"+_info.Key + " : " + _info.Value.ToString()+"}");
                    Debug.Log("[Event] " + _eventName + "\n param : \n" + string.Join("\n", _param) + "\n value : " + _count);
                }
            #endif
            
            try{
                Instance.sendEvent(_eventName, _eventData, _count);
            }catch(Exception e){
                Debug.LogWarning("[Analytics Error] " + e.ToString());
            }
        }

        public List<string> GetTerms(){
            var _result = new List<string>();
            for(int i = 0; i < libList.Count; i++){
                _result.Add(libList[i].TermsURL);
            }

            return _result;
        }

        public void SetUserConsent(bool _isEnabled){
            for(int i = 0; i < libList.Count; i++){
                libList[i].SetUserConsent(_isEnabled);
            }
        }


        public void logException(System.Exception _exception){
            for(int i = 0; i < libList.Count; i++){
                libList[i].LogException(_exception);
            }
        }

        public void setCustomData(string _key, string _value){
            for(int i = 0; i < libList.Count; i++){
                libList[i].SetCustomData(_key, _value);
            }
        }

        static public void SetCustomData(string _key, string _value){
            Instance.setCustomData(_key, _value);
        }

        static public void LogException(System.Exception _exception){
            Instance.logException(_exception);
        }
    }

    public interface IAnalyticsLib{
        void Event(string _eventName, Dictionary<string, object> _eventData = null, int _count = 1);
        string TermsURL{get;}
        void SetUserConsent(bool _isEnabled);
        void SetCustomData(string _key, string _value);
        void LogException(System.Exception _exception);
    }
}
