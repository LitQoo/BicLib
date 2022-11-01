#if BICUTIL_ANALYTICS_UNITY
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

namespace BicUtil.Analytics
{
    public class UnityAnalytics : IAnalyticsLib{
        private object lockObject = new object();

        public string TermsURL => "https://unity3d.com/fr/legal/privacy-policy";

        public void Event(string _name, Dictionary<string, object> _eventData = null, int _value = 1){
            lock(lockObject){
                if(_eventData == null){
                    UnityEngine.Analytics.Analytics.CustomEvent(_name);
                }else{
                    UnityEngine.Analytics.Analytics.CustomEvent(_name, _eventData);
                }
            }
        }


        public void SetUserConsent(bool _isEnabled){
        }

        public void SetCustomData(string _key, string _value){
            UnityEngine.CrashReportHandler.CrashReportHandler.SetUserMetadata(_key, _value);
        }
    }
}
#endif