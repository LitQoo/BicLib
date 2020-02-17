using System.Runtime.CompilerServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BicUtil.Analytics
{
    public class Analytics : BicUtil.SingletonBase.SingletonBase<Analytics>
    {
        private List<IAnalyticsLib> libList = new List<IAnalyticsLib>();
        
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
        }

        private void addService(IAnalyticsLib _lib){
            libList.Add(_lib);
        }

        private void sendEvent(string _eventName, Dictionary<string, object> _eventData = null, int _count = 1){
            #if UNITY_EDITOR
                if(libList.Count <= 0){
                    Debug.LogError("[Analytics] Analytics Lib is Not Added");
                }
                return;
            #else

            for(int i = 0; i < libList.Count; i++){
                libList[i].Event(_eventName, _eventData, _count);
            }
            #endif
        }

        static public void Event(string _eventName, Dictionary<string, object> _eventData = null, int _count = 1){
            Instance.sendEvent(_eventName, _eventData, _count);
        }

    }

    public interface IAnalyticsLib{
        void Event(string _eventName, Dictionary<string, object> _eventData = null, int _count = 1);
    }
}
