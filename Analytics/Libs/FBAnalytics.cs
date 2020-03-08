#if BICUTIL_ANALYTICS_FB
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Facebook.Unity;

namespace BicUtil.Analytics
{
    public class FBAnalytics : IAnalyticsLib{
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
            if(FB.IsInitialized == true){
                RetrySavedEvent();
                
                try{
                    FB.LogAppEvent(_name, _value, _eventData);
                    
                }catch{
                    Debug.Log("LogAppEvent Error " + _name);
                }
            }else{
                savedEvent.Add(new SavedEvent(_name, _eventData, _value));
                needRetryEvent = true;
            }
        }

        public void RetrySavedEvent(){
            if(needRetryEvent == true){
                try{
                    foreach(var _savedData in savedEvent){
                        Debug.Log("RetrySavedEvent FBLog " + _savedData.Name);
                        FB.LogAppEvent(_savedData.Name, _savedData.Value, _savedData.EventData);
                    }

                    savedEvent.Clear();
                }catch{
                    Debug.Log("RetrySavedEvent Error ");
                }
                needRetryEvent = false;
            }
        }
    }
}
#endif