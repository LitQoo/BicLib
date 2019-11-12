#if BICUTIL_ANALYTICS_APPSFLYER
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BicUtil.Analytics
{
    public class AppsFlyerAnalytics : IAnalyticsLib{
        public void Event(string _name, Dictionary<string, object> _eventData = null, int _value = 1){
            var _afData = new Dictionary<string, string>();

            if(_eventData != null){
                foreach (var _item in _eventData)
                {
                    _afData.Add(_item.Key, _item.Value.ToString());
                }
            }

            AppsFlyer.trackRichEvent(_name, _afData);
        }
    }
}
#endif