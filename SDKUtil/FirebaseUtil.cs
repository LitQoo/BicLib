#if BICUTIL_ANALYTICS_FIREBASE
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BicDB.Container;
using BicDB.Core;
using BicDB.Variable;
using BicUtil.Analytics;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BicUtil.SDKUtil
{
    public static class FirebaseUtil
    {
        #region FireBase
        static private void setRemoteConfigDefaultValue(IRecordContainer _constants){
            Debug.Log("setRemoteConfigDefaultValue");
            var _default = new Dictionary<string, object>();
            foreach(var _value in _constants){
                _default.Add(_value.Key, _value.Value.AsVariable.AsString);
            }

            Firebase.RemoteConfig.FirebaseRemoteConfig.SetDefaults(_default);
        }

        static private void updateConstant(IRecordContainer _constants){
            Debug.Log("start Update Constant");
            foreach(var _value in _constants){
                var _stringValue = Firebase.RemoteConfig.FirebaseRemoteConfig.GetValue(_value.Key).StringValue;
                if(string.IsNullOrEmpty(_stringValue) == false){
                    try{
                        _value.Value.AsVariable.AsString = _stringValue;
                    }catch{
                        Debug.Log("updateConstant error " + _value.Key);
                    }
                }
            }
            
            if(_constants.ContainsKey("ab_group") == true){

                lock(FirebaseAnalytics.LOCK_CHECK){
                    abTestName = _constants["ab_group"].AsVariable;
                }
                
                if(!string.IsNullOrEmpty(abTestName.AsString) && abTestName.AsString.ToLower() != "none"){
                    Firebase.Analytics.FirebaseAnalytics.SetUserProperty("ABGroup", _constants["ab_group"].AsVariable.AsString);
                }
            }else{
                #if UNITY_EDITOR
                Debug.LogError("[ABTest] Add 'ab_group' value in Constant");
                #endif
            }
        }

        static private async Task<Firebase.DependencyStatus> checkAndFixDependenciesAsync(IRecordContainer _constants){
            Firebase.Analytics.FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
            
            var _result = await Firebase.FirebaseApp.CheckAndFixDependenciesAsync();

            if(_result == Firebase.DependencyStatus.Available){
                Application.logMessageReceived += log;
            }

            Debug.Log("firebase init complete "+ _result.ToString());
            // BicUtil.Analytics.Analytics.Event("FirebaseInit", new Dictionary<string, object> {
            //     {
            //         "Result",
            //         _result.ToString()
            //     }
            // });

            if(_result == Firebase.DependencyStatus.Available){
                try{
                    Firebase.Analytics.FirebaseAnalytics.SetUserProperty("SetupVersion", TableService.GetStringProperty(TableService.PROP_FIELD_INSTALL_VERSION, Application.version));
                    Firebase.Analytics.FirebaseAnalytics.SetUserProperty("SetupDateHour", TableService.GetStringProperty(TableService.PROP_FIELD_INSTALL_DATEHOUR, DateTime.UtcNow.ToString("yyMMddHH")));
                    Firebase.Analytics.FirebaseAnalytics.SetUserProperty("SetupVersionNumber", GetVersionNumber(TableService.GetStringProperty(TableService.PROP_FIELD_INSTALL_VERSION, Application.version)).ToString());
                    Firebase.Analytics.FirebaseAnalytics.SetUserProperty("IsSetupNow", TableService.IsSetup.ToString());
                    Firebase.Analytics.FirebaseAnalytics.SetUserId(TableService.UserId);
                }catch(System.Exception _error){
                    Debug.Log("[Firebase] InitializationException property " + _error.Message);
                }
                
                try{
                    await remoteConfigAsync(_constants, 2f);
                }catch(System.Exception _error){
                    Debug.Log("[Firebase] InitializationException " + _error.Message);
                }
            }

            return _result;
        }

        public static int GetVersionNumber(string _versionString){
            try{
                if(_versionString.Contains(".") == true){
                    var _versions = _versionString.Split('.');
                    int _result = 0;
                    _result += int.Parse(_versions[0]) * 10000;
                    _result += int.Parse(_versions[1]) * 100;

                    if(_versions.Length >=3){
                        _result += int.Parse(_versions[2]);
                    }

                    return _result;
                }else{
                    return int.Parse(_versionString) * 10000;	
                }
            }catch{
                return 0;
            }
        }

        static public async Task<BicDB.Result> InitFirebaseAsync(IRecordContainer _constants, float _timeout){
            try{
                var _initTask = checkAndFixDependenciesAsync(_constants);
                var _timeoutTask = Task.Run(async ()=>{await Task.Delay(TimeSpan.FromSeconds(_timeout)); return new BicDB.Result(1);});
                var _result = await Task.WhenAny(_initTask, _timeoutTask);

                if(_result == _initTask){
                    if (_initTask.Result == Firebase.DependencyStatus.Available) {
                        Debug.Log("firebase init available");
                        return new BicDB.Result(0);
                    }else{
                        Debug.Log("firebase init not available");
                        return new BicDB.Result(1);
                    }
                }else if(_result == _timeoutTask){
                    Debug.Log("firebase init timeout");
                    return new BicDB.Result(2);
                }else{
                    Debug.Log("firebase init known");
                    return new BicDB.Result(3);
                }
            }catch(System.Exception _e){
                Debug.Log("firebase init exception " + _e.Message);
                return new BicDB.Result(3, "", 0, _e.Message);
            }
        }

        static private async Task remoteConfigAsync(IRecordContainer _constants, float _timeout){
            setRemoteConfigDefaultValue(_constants);
            
            Debug.Log("FirebaseRemoteConfig Start");

            var _asyncTask = Task.Run(async ()=>
            {
                var _reloadTime = TimeSpan.FromHours(12);
                if (TableService.IsUpdate == true)
                {
                    _reloadTime = TimeSpan.Zero;
                }

                #if UNITY_EDITOR
                Debug.Log("FirebaseRemoteConfig FetchAsync EditorMode");
                _reloadTime = TimeSpan.Zero;
                #endif

                await Task.Delay(20);

                var _fetchTask = Firebase.RemoteConfig.FirebaseRemoteConfig.FetchAsync(_reloadTime);
                await _fetchTask.ContinueWith(FetchComplete);
                var _isFetched = Firebase.RemoteConfig.FirebaseRemoteConfig.ActivateFetched();

                await Task.Delay(20);

                #if !UNITY_EDITOR
                //sendActiveABTestEvent();
                //await Task.Delay(20);
                updateConstant(_constants);
                #endif

                return new BicDB.Result(0);
            });

            var _timeoutTask = Task.Run(async ()=>{
                await Task.Delay(TimeSpan.FromSeconds(_timeout)); 
                return new BicDB.Result(1);
            });

            var _result = await Task.WhenAny(_asyncTask, _timeoutTask);
            Debug.Log("remoteConfigAsync result = " + _result.Result.ToString());
        }

        private static void sendActiveABTestEvent()
        {
            var _eventName = "";
            if (TableService.IsSetup == true)
            {
                _eventName = "InitRemoteConfigOn";
                BicUtil.Analytics.Analytics.Event("InitRemoteConfigNewUser", new Dictionary<string, object> {
                        {
                            "Result",
                            "Success"
                        }
                    });
            }
            else
            {
                _eventName = "OldUserRemoteConfigOn";
            }

            try
            {
                _eventName = _eventName + SceneManager.GetActiveScene().name;
                Debug.Log(_eventName);
                BicUtil.Analytics.Analytics.Event(_eventName, new Dictionary<string, object> {
                        {
                            "Result",
                            "Success"
                        }
                });
            }
            catch (Exception _e)
            {
                Debug.Log("send InitRemoteConfigOn error");
                Firebase.Crashlytics.Crashlytics.LogException(_e);
            }
        }

        static private void FetchComplete(Task fetchTask)
         {
              if (fetchTask.IsCanceled)
              {
                  Debug.Log("Fetch canceled.");
              }
              else if (fetchTask.IsFaulted)
              {
                  Debug.Log("Fetch encountered an error.");
              }
              else if (fetchTask.IsCompleted)
              {
                  Debug.Log("Fetch completed successfully!");
              }

              var info = Firebase.RemoteConfig.FirebaseRemoteConfig.Info;

              switch (info.LastFetchStatus)
              {
                  case Firebase.RemoteConfig.LastFetchStatus.Success:
                    //Firebase.RemoteConfig.FirebaseRemoteConfig.ActivateFetched();

                      Debug.Log(string.Format("Remote data loaded and ready (last fetch time {0}).", info.FetchTime));
                    //   string stop = Firebase.RemoteConfig.FirebaseRemoteConfig.GetValue("stops").StringValue;
                    //   Debug.Log("Value: " + (string.IsNullOrEmpty(stop) ? "NA" : stop));

                      // Also tried this way, but then it doesn't enter the IF block
                      /*if (FirebaseRemoteConfig.ActivateFetched())
                      { 
                           Debug.Log(string.Format("Remote data loaded and ready (last fetch time {0}).", info.FetchTime));

                           string stop = FirebaseRemoteConfig.GetValue("stops").StringValue;
                           Debug.Log("Value: " + (string.IsNullOrEmpty(stop) ? "NA" : stop));
                      }*/
                      break;
                  case Firebase.RemoteConfig.LastFetchStatus.Failure:
                      switch (info.LastFetchFailureReason)
                      {
                          case Firebase.RemoteConfig.FetchFailureReason.Error:
                                Debug.Log("Fetch failed for unknown reason");
                                break;
                          case Firebase.RemoteConfig.FetchFailureReason.Throttled:
                                Debug.Log("Fetch throttled until " + info.ThrottledEndTime);
                                break;
                      }
                     break;
                  case Firebase.RemoteConfig.LastFetchStatus.Pending:
                     Debug.Log("Latest Fetch call still pending.");
                     break;
            }
        }

        static public void InitFirebase(){
            Firebase.Analytics.FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
            Firebase.Analytics.FirebaseAnalytics.SetUserId(TableService.UserId);
            Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
                var dependencyStatus = task.Result;
                if (dependencyStatus == Firebase.DependencyStatus.Available) {
                    Application.logMessageReceived += log;
                }
            });
        }

        private static void log(string _condition, string _stackTrace, LogType _type)
        {
            lock(FirebaseAnalytics.LOCK_CHECK){
                Firebase.Crashlytics.Crashlytics.Log(_condition + "\n[stack]" + _stackTrace + "\n[type]" + _type.ToString());
            }
        }

        static private int sendABTestEventCount = 0;
        static private bool isSendABTestEvent = false;
        static private IVariable abTestName = null;
        
        static public void SendABTestEvent(){
            lock(FirebaseAnalytics.LOCK_CHECK){
                if(isSendABTestEvent == false && abTestName != null){
                    var _abGroup = abTestName.AsString;
                    sendABTestEventCount++;
                    if((!string.IsNullOrEmpty(_abGroup) && _abGroup.ToLower() != "none") || sendABTestEventCount > 3){
                        isSendABTestEvent = true;
                        BicUtil.Analytics.Analytics.Event("ABGroup", new Dictionary<string, object> {
                            {
                                "GroupName",
                                _abGroup
                            },
                            {
                                "SessionCount",
                                TableService.SessionCount
                            },
                        });
                    }
                }
            }
        }
        #endregion

    }
}
#endif