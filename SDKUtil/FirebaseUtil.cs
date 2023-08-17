#if BICUTIL_ANALYTICS_FIREBASE
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using BicDB.Container;
using BicDB.Core;
using BicDB.Variable;
using BicUtil.Analytics;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BicUtil.SDKUtil
{
    public static class FirebaseUtil
    {
        #region FireBase
        static private async UniTask setRemoteConfigDefaultValueAsync(IRecordContainer _constants){
            if(_constants == null){
                Debug.Log("start constants is null");
                return;
            }
            
            var _default = new Dictionary<string, object>();
            foreach(var _value in _constants){
                switch(_value.Value.Type){
                    case BicDB.DataType.Int:
                        _default.Add(_value.Key, _value.Value.AsVariable.AsInt); 
                    break;
                    case BicDB.DataType.Float:
                        _default.Add(_value.Key, _value.Value.AsVariable.AsFloat); 
                    break;
                    case BicDB.DataType.Bool:
                        _default.Add(_value.Key, _value.Value.AsVariable.AsBool); 
                    break;
                    case BicDB.DataType.String:
                        _default.Add(_value.Key, _value.Value.AsVariable.AsString);
                    break; 
                    case BicDB.DataType.Enum:
                        _default.Add(_value.Key, _value.Value.AsVariable.AsString); 
                    break;
                    default:
                        try{
                            Debug.LogError("[Firebase] Remote config not supports  " + _value.Value.Type.ToString());
                            _default.Add(_value.Key, _value.Value.AsVariable.AsString); 
                        }catch(System.Exception _e){
                            Debug.LogError("[Firebase] remote config updateConstant error " + _value.Key);
                            Firebase.Crashlytics.Crashlytics.LogException(_e);
                        }
                    break;
                }
            }

            await Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(_default).AsUniTask();
        }

        static private void updateConstant(IRecordContainer _constants){
            if(_constants == null){
                return;
            }

            foreach(var _value in _constants){
                var _configValue = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue(_value.Key);
                var _stringValue = _configValue.StringValue;

                if(string.IsNullOrEmpty(_stringValue) == false){
                    try{
                        _value.Value.AsVariable.AsString = _stringValue;
                    }catch(System.Exception _e){
                        Debug.LogError("[Firebase] remote config updateConstant error " + _value.Key);
                        Firebase.Crashlytics.Crashlytics.LogException(_e);
                    }
                }
            }
            
            if(_constants.ContainsKey("ab_group") == true){

                lock(FirebaseAnalytics.LOCK_CHECK){
                    abTestName = _constants["ab_group"].AsVariable;
                }
                
                if(!string.IsNullOrEmpty(abTestName.AsString)){
                    Firebase.Analytics.FirebaseAnalytics.SetUserProperty("ABGroup", abTestName.AsString);
                }
            }else{
                #if UNITY_EDITOR
                Debug.LogError("[ABTest] Add 'ab_group' value in Constant");
                #endif
            }
        }
        
        static public Firebase.DependencyStatus Status = Firebase.DependencyStatus.UnavilableMissing;
        static private async UniTask<Firebase.DependencyStatus> checkAndFixDependenciesAsync(IRecordContainer _constants){
            FirebaseAnalytics.SetUserId(TableService.UserId);
            var _fbInitTask = Firebase.FirebaseApp.CheckAndFixDependenciesAsync().AsUniTask();
            var _result = await _fbInitTask;

            // .ContinueWithOnMainThread(async _task=>{
                if(_result == Firebase.DependencyStatus.Available){
                    try{
                        Status = Firebase.DependencyStatus.Available;

                        //await Task.Delay(3000);
                        Firebase.Analytics.FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
                        Firebase.Crashlytics.Crashlytics.IsCrashlyticsCollectionEnabled = true;
                        var _installVersion = TableService.GetStringProperty(TableService.PROP_FIELD_INSTALL_VERSION, Application.version);
                        FirebaseAnalytics.SetCustomKey("SetupVersion", _installVersion);
                        var _installDateHour = TableService.GetStringProperty(TableService.PROP_FIELD_INSTALL_DATEHOUR, DateTime.Now.ToString("yyMMddHH"));
                        var _installDateString = _installDateHour.Substring(0, 6);
                        FirebaseAnalytics.SetCustomKey("SetupDateHour", _installDateHour);
                        FirebaseAnalytics.SetCustomKey("SetupDate", _installDateString);
                        FirebaseAnalytics.SetCustomKey("SetupVersionNumber", GetVersionNumber(_installVersion).ToString());
                        FirebaseAnalytics.SetCustomKey("IsSetupNow", TableService.IsSetup.ToString());
                        FirebaseAnalytics.SetCustomKey("SetupDateLocal", TableService.InstallDateLocal);
                        FirebaseAnalytics.SetCustomKey("DaysAfterSetup", TableService.DaysAfterInstall.ToString());
                        FirebaseAnalytics.SetCustomKey("Session", TableService.SessionCount.ToString());

                         if(TableService.IsSetup == true){
                            Analytics.Analytics.Event("FirebaseInitOnInstall", new Dictionary<string, object> {
                                {
                                    "RealtimeSinceStartup",
                                    UnityEngine.Time.realtimeSinceStartup
                                }
                            });
                        }

                        Application.logMessageReceived += log;

                    }catch(System.Exception _error){
                        Debug.LogError("[Firebase] InitializationException property " + _error.ToString() + "/////" + _error.StackTrace);
                        Firebase.Crashlytics.Crashlytics.LogException(_error);
                    }

                    if(_result == Firebase.DependencyStatus.Available){
                        try{
                            if(_constants != null){
                                await remoteConfigAsync(_constants);
                            }
                        }catch(System.Exception _error){
                            Debug.LogError("[Firebase] InitializationException remoteConfigAsync " + _error.ToString() + "/" + _error.Message);
                            Firebase.Crashlytics.Crashlytics.LogException(_error);
                        }
                    }
                }else{
                    var _exception = new SystemException("Firebase Not Available " + _result.ToString());
                    Firebase.Crashlytics.Crashlytics.LogException(_exception);
                }
            // });

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

        static public async UniTask<BicDB.Result> InitFirebaseAsync(IRecordContainer _constants, float _timeout){
            try{
                var _initTask = checkAndFixDependenciesAsync(_constants);
                var _timeoutTask = UniTask.RunOnThreadPool(async ()=>{await UniTask.Delay(TimeSpan.FromSeconds(_timeout)); return new BicDB.Result(1);});
                var _result = await UniTask.WhenAny(_initTask, _timeoutTask);
            
                if(_result.winArgumentIndex == 0){
                    if (_result.result1 == Firebase.DependencyStatus.Available) {
                        return new BicDB.Result(0);
                    }else{
                        return new BicDB.Result(1);
                    }
                }else if(_result.winArgumentIndex == 1){
                    return new BicDB.Result(2);
                }else{
                    return new BicDB.Result(3);
                }
            }catch(System.Exception _e){
                Debug.LogError("[Firebase] Error InitFirebaseAsync " + _e.Message);
                Firebase.Crashlytics.Crashlytics.LogException(_e);
                return new BicDB.Result(3, "", 0, _e.Message);
            }
        }

        static private async UniTask remoteConfigAsync(IRecordContainer _constants){
            var _errorLine = 0;
            try{
            await setRemoteConfigDefaultValueAsync(_constants);
            
            #if UNITY_EDITOR
            if(tester != null){
                if(tester.UseTester == true){
                    tester.SetConstantsValues(_constants);
                    return;
                }else{
                    Debug.Log("[RemoteConfigTester] Use Sever Value");
                }
            }
            #endif


            _errorLine++;
            var _reloadTime = TimeSpan.FromHours(12);
            if (TableService.IsUpdate == true)
            {
                _reloadTime = TimeSpan.Zero;
            }

            _errorLine++;
            #if UNITY_EDITOR
            _reloadTime = TimeSpan.Zero;
            #endif


            _errorLine++;
            var _fetchTask = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.FetchAsync(_reloadTime).AsUniTask();
            
            _errorLine++;

            try{
                await _fetchTask;
            }catch(System.Exception _exception){

                var info = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.Info;
                if(info.LastFetchStatus != Firebase.RemoteConfig.LastFetchStatus.Success){
                    Debug.Log("FetchAsync fail" + info.LastFetchStatus + "/" + info.LastFetchFailureReason);
                    BicUtil.Analytics.Analytics.Event("FetchAsyncError", new Dictionary<string, object> {
                        {
                            "LastFetchStatus",
                            info.LastFetchStatus.ToString()
                        },
                        {
                            "LastFetchFailureReason",
                            info.LastFetchFailureReason.ToString()
                        }
                    });
                }
                
                Debug.LogError("[Firebase] remoteConfigAsync _fetchTask");
                throw _exception;
            }

            _errorLine++;
            
            FetchComplete(_fetchTask);


            _errorLine++;
            var _activateTask = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.ActivateAsync().AsUniTask();
            
            _errorLine++;
            try{
                var _activateResult = await _activateTask;


                if(_activateResult == false){
                    Debug.LogError("ActivateAsync error");
                    BicUtil.Analytics.Analytics.Event("ActivateAsyncError");
                }
            }catch(System.Exception _exception){
                Debug.LogError("[Firebase] remoteConfigAsync _activateTask");
                throw _exception;
            }


            _errorLine++;

            #if !UNITY_EDITOR
            sendActiveABTestEvent();
            #endif
            
            _errorLine++;
            try{
                updateConstant(_constants);
            }catch(System.Exception _exception){
                Debug.LogError("[Firebase] remoteConfigAsync updateConstant");
                throw _exception;
            }
            }catch(Firebase.FirebaseException _exception){
                Debug.LogError("Firebase remoteConfigAsync exception " + _exception.ErrorCode + "/" + _exception.Message + "/" + _errorLine);
                throw _exception;
            }
            // await _fetchedTask.ContinueWithOnMainThread(_resultTask=>{
            //     #if !UNITY_EDITOR
            //     sendActiveABTestEvent();
            //     #endif
                
            //     updateConstant(_constants);
            // });

            // var _asyncTask = Task.Run(async ()=>
            // {
                
                
            //     return new BicDB.Result(0);
            // });

            // var _timeoutTask = Task.Run(async ()=>{
            //     await Task.Delay(TimeSpan.FromSeconds(_timeout)); 
            //     return new BicDB.Result(1);
            // });

            //var _result = await _asyncTask; //Task.WhenAny(_asyncTask, _timeoutTask);
        }

        private static RemoteConfigTester tester = null;
        public static void SetRemoteConfigTester(RemoteConfigTester _tester){
            tester = _tester;
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
                        },
                        {
                            "RealtimeSinceStartup",
                            UnityEngine.Time.realtimeSinceStartup
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
                BicUtil.Analytics.Analytics.Event(_eventName, new Dictionary<string, object> {
                        {
                            "Result",
                            "Success"
                        },
                        {
                            "RealtimeSinceStartup",
                            UnityEngine.Time.realtimeSinceStartup
                        }
                });
            }
            catch (Exception _e)
            {
                Debug.LogError("send InitRemoteConfigOn" + SceneManager.GetActiveScene().name + " error");
                Firebase.Crashlytics.Crashlytics.LogException(_e);
            }
        }

        static private void FetchComplete(UniTask fetchTask)
         {
              if (fetchTask.Status == UniTaskStatus.Canceled)
              {
                  Debug.Log("Remoteconfig Fetch canceled.");
              }
              else if (fetchTask.Status == UniTaskStatus.Faulted)
              {
                  Debug.Log("Remoteconfig Fetch encountered an error.");
              }
              else if (fetchTask.Status == UniTaskStatus.Succeeded)
              {
                   DebugForEditor.Log("Fetch completed successfully!");
              }

              var info = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.Info;

              switch (info.LastFetchStatus)
              {
                  case Firebase.RemoteConfig.LastFetchStatus.Success:
                    //Firebase.RemoteConfig.FirebaseRemoteConfig.ActivateFetched();

                      DebugForEditor.Log(string.Format("Remoteconfig Remote data loaded and ready (last fetch time {0}).", info.FetchTime));
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
                                DebugForEditor.Log("Remoteconfig Fetch failed for unknown reason");
                                break;
                          case Firebase.RemoteConfig.FetchFailureReason.Throttled:
                                DebugForEditor.Log("Remoteconfig Fetch throttled until " + info.ThrottledEndTime);
                                break;
                      }
                     break;
                  case Firebase.RemoteConfig.LastFetchStatus.Pending:
                     DebugForEditor.Log("Remoteconfig Latest Fetch call still pending.");
                     break;
            }
        }

        static public void InitFirebase(){
            Firebase.Analytics.FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
            Firebase.Crashlytics.Crashlytics.IsCrashlyticsCollectionEnabled = true;
            Firebase.Analytics.FirebaseAnalytics.SetUserId(TableService.UserId);
            Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {

                var dependencyStatus = task.Result;
                FirebaseUtil.Status = task.Result;
                
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