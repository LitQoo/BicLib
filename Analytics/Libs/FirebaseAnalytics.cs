
#if BICUTIL_ANALYTICS_FIREBASE
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BicUtil.Analytics
{
    public class FirebaseAnalytics : IAnalyticsLib{

        static public object LOCK_CHECK{get;} = new object();
        public string TermsURL => "https://policies.google.com/privacy/update";
        
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
            lock(LOCK_CHECK){
                if(BicUtil.SDKUtil.FirebaseUtil.Status == Firebase.DependencyStatus.Available){
                    retrySavedEvent();
                    sendEvent(_name, _value, _eventData);
                }else{
                    if(savedEvent.Count < 100){
                        savedEvent.Add(new SavedEvent(_name, _eventData, _value));
                        needRetryEvent = true;
                    }
                }
            }
        }

        private void retrySavedEvent(){
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
                _params.Add(new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterValue, _value));

                foreach(var _param in _data){
                    _params.Add(new Firebase.Analytics.Parameter(_param.Key, _param.Value.ToString()));
                }

                Firebase.Analytics.FirebaseAnalytics.LogEvent(_eventName, _params.ToArray());
            }else{
                Firebase.Analytics.FirebaseAnalytics.LogEvent(_eventName);
            }
        }

        public void SetUserConsent(bool _isEnabled){
            //Firebase.Analytics.FirebaseAnalytics.SetAnalyticsCollectionEnabled(_isEnabled);
        }
    }
}
#endif