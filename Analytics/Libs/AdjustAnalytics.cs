#if BICUTIL_ANALYTICS_ADJUST
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Facebook.Unity;
using com.adjust.sdk;
using System;

namespace BicUtil.Analytics
{
    public class AdjustAnalytics : IAnalyticsLib
    {
        public string TermsURL => "https://www.adjust.com/terms/general-terms-and-conditions/";

        public void Event(string _eventName, Dictionary<string, object> _eventData = null, int _count = 1)
        {
            var _eventInfoList = eventInfoList.Where(_row=>_row.EventName == _eventName);
            
            for(int i = 0; i < _eventInfoList.Count(); i++ ){
                var _eventInfo = _eventInfoList.ElementAt(i);
                if(_eventInfo.ConditionFunc == null || _eventInfo.ConditionFunc(_eventData) == true){ 
                    AdjustEvent adjustEvent = new AdjustEvent(_eventInfo.EventToken);
                    Adjust.trackEvent(adjustEvent);
                }
            }
        }

        private static List<EventTokenInfo> eventInfoList;

        public static void SetEventToken(string _eventToken, string _eventName, Func<Dictionary<string, object>, bool> _conditionFunc = null){
            if(eventInfoList == null){
                eventInfoList = new List<EventTokenInfo>();
            }

            eventInfoList.Add(new EventTokenInfo(_eventToken, _eventName, _conditionFunc));
        }

        public void SetUserConsent(bool _isEnabled)
        {
            
        }
    }

    internal class EventTokenInfo{
        public string EventToken;
        public string EventName;
        public Func<Dictionary<string, object>, bool> ConditionFunc;
        
        public EventTokenInfo(string _eventToken, string _eventName, Func<Dictionary<string, object>, bool> _conditionFunc){
            this.EventToken = _eventToken;
            this.ConditionFunc = _conditionFunc;
            this.EventName = _eventName;
        }
    } 
}
#endif