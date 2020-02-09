#if BICUTIL_ANALYTICS_FIREBASE
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BicUtil.Analytics
{
    public class FirebaseAnalytics : IAnalyticsLib{
        private struct SavedEvent{
            public string Name;
            public Dictionary<string, object> EventData;
            public int Value;

            public SavedEvent(string _name, Dictionary<string, object> _eventData = null, int _value = 1){
                this.Name = _name;
                this.EventData = _eventData;
                this.Value = _value;
            }
        } 

        private bool needRetryEvent = false;
        private List<SavedEvent> savedEvent = new List<SavedEvent>();
        public void Event(string _name, Dictionary<string, object> _eventData = null, int _value = 1){
            UnityEngine.Debug.Log("firebase event 1");
            var _isAvailable = Firebase.DependencyStatus.UnavilableMissing;
            UnityEngine.Debug.Log("firebase event 2");
            try{
                UnityEngine.Debug.Log("firebase event 3");
                _isAvailable = Firebase.FirebaseApp.CheckDependencies();
                UnityEngine.Debug.Log("firebase event 4");
            }catch{
                UnityEngine.Debug.Log("firebase event 5");
                _isAvailable = Firebase.DependencyStatus.UnavilableMissing;
                UnityEngine.Debug.Log("firebase event 6");
            }
            
            UnityEngine.Debug.Log("firebase event 7");
            if(_isAvailable == Firebase.DependencyStatus.Available){
            UnityEngine.Debug.Log("firebase event 8");
                RetrySavedEvent();

                UnityEngine.Debug.Log("firebase event 9");
                sendEvent(_name, _value, _eventData);

                UnityEngine.Debug.Log("firebase event 10");
            }else{

                UnityEngine.Debug.Log("firebase event 11");
                savedEvent.Add(new SavedEvent(_name, _eventData, _value));
                needRetryEvent = true;

                UnityEngine.Debug.Log("firebase event 12");
            }

            UnityEngine.Debug.Log("firebase event 13");
        }

        public void RetrySavedEvent(){
            if(needRetryEvent == true){
                foreach(var _savedData in savedEvent){
                    sendEvent(_savedData.Name, _savedData.Value, _savedData.EventData);
                }

                savedEvent.Clear();
                needRetryEvent = false;
            }
        }

        private void sendEvent(string _eventName, int _value, Dictionary<string, object> _data){
            if(_data != null){
                List<Firebase.Analytics.Parameter> _params = new List<Firebase.Analytics.Parameter>();

                foreach(var _param in _data){

                    UnityEngine.Debug.Log("firebase sendEvent " + _param.Key);
                    _params.Add(new Firebase.Analytics.Parameter(_param.Key, _param.Value.ToString()));
                }

                Firebase.Analytics.FirebaseAnalytics.LogEvent(_eventName, _params.ToArray());
            }else{
                Firebase.Analytics.FirebaseAnalytics.LogEvent(_eventName);
            }
        }
    }
}
#endif