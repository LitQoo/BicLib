#if BICUTIL_ANALYTICS_FB
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Facebook.Unity;

namespace BicUtil.Analytics
{
    public class FBAnalytics : IAnalyticsLib{
        #region Static
        private static bool isEnabled = true;
        public static void SetEnable(bool _isEnabled){
            isEnabled = _isEnabled;
        }
        #endregion
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

        public string TermsURL => "https://www.facebook.com/policy";

        public void Event(string _name, Dictionary<string, object> _eventData = null, int _value = 1){
            if(isEnabled == false){
                return;
            }
            
            lock(savedEvent){
                if(FB.IsInitialized == true){
                    RetrySavedEvent();

                    try{
                        eventWithLock(_name, _eventData, _value);
                        
                    }catch{
                        Debug.Log("LogAppEvent Error " + _name);
                    }
                }else{
                    if(savedEvent.Count < 100){
                        savedEvent.Add(new SavedEvent(_name, _eventData, _value));
                        needRetryEvent = true;
                    }
                }
            }
        }

        public void RetrySavedEvent(){
            if(needRetryEvent == true){
                try{
                    foreach(var _savedData in savedEvent){
                        Debug.Log("RetrySavedEvent FBLog " + _savedData.Name);
                        eventWithLock(_savedData.Name, _savedData.EventData, _savedData.Value);
                    }

                    savedEvent.Clear();
                }catch{
                    Debug.Log("RetrySavedEvent Error ");
                }

                needRetryEvent = false;
            }
        }

        private void eventWithLock(string _name, Dictionary<string, object> _eventData = null, int _value = 1){
            FB.LogAppEvent(_name, _value, _eventData);
        }

        public void SetUserConsent(bool _isEnabled){
            FB.Mobile.SetAutoLogAppEventsEnabled(_isEnabled);
        }
        
    }
}
#endif