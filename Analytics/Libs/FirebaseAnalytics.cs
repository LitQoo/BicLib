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

        private bool isCheckInit = false;
        private bool isInit = false;
        private bool needRetryEvent = false;
        private List<SavedEvent> savedEvent = new List<SavedEvent>();
        public void Event(string _name, Dictionary<string, object> _eventData = null, int _value = 1){
            if(isCheckInit == false){
                checkInit();
            }

            if(isInit == true){
                RetrySavedEvent();
                sendEvent(_name, _value, _eventData);
            }else{
                savedEvent.Add(new SavedEvent(_name, _eventData, _value));
                needRetryEvent = true;
            }
        }

        private void checkInit()
        {
            isCheckInit = true;
            Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(_task=>{
                var dependencyStatus = _task.Result;
                if (dependencyStatus == Firebase.DependencyStatus.Available) {
                    isInit = true;
                }else{
                    isInit = false;
                }
            });
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
            
            List<Firebase.Analytics.Parameter> _params = new List<Firebase.Analytics.Parameter>();

            foreach(var _param in _data){
                _params.Add(new Firebase.Analytics.Parameter(_param.Key, _param.Value.ToString()));
            }
           
            Firebase.Analytics.FirebaseAnalytics.LogEvent(_eventName, _params.ToArray());
        }
    }
}
#endif