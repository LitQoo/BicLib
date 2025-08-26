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
using System.Linq;

namespace BicUtil.SDKUtil
{
    public enum FirebaseUtilState
    {
        Ready,
        Pending,
        Complete,
        CompleteWithTimeOver,
        TimeOver,
    }
    public static class FirebaseUtil
    {
        #region FireBase
        static private int maxRetries = 3;
        static private int retryDelayMs = 1000;

        static public FirebaseUtilState State = FirebaseUtilState.Ready;
        static public Func<bool> ConfirmUpdateRemoteConfig = null;
        
        static public Action OnFetchedRemoteConfig = null;
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
                            BicUtil.Analytics.Analytics.LogException(_e);
                        }
                    break;
                }
            }

            await Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(_default);
            await UniTask.SwitchToMainThread();
        }

        static private void updateConstant(IRecordContainer _constants){
            if(_constants == null){
                return;
            }

            if(ConfirmUpdateRemoteConfig != null && ConfirmUpdateRemoteConfig() == false){
                Analytics.Analytics.Event("IgnoreUpdateRemoteConfig", new Dictionary<string, object> {{"state", State.ToString()}});
                return;
            }

            DebugForEditor.Log("[RemoteConfig] UPDATE CONSTANTS ----");
            foreach(var _value in _constants){
                var _configValue = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue(_value.Key);
                var _stringValue = _configValue.StringValue;

                if(string.IsNullOrEmpty(_stringValue) == false){
                    try{
                        _value.Value.AsVariable.AsString = _stringValue;
                        DebugForEditor.Log(_value.Key + " : " + _stringValue);
                    }catch(System.Exception _e){
                        Debug.LogError("[Firebase] remote config updateConstant error " + _value.Key);
                        BicUtil.Analytics.Analytics.LogException(_e);
                    }
                }
            }
            DebugForEditor.Log("[RemoteConfig] ---- Finihed ----");
            
            if(_constants.ContainsKey("ab_group") == true){

                lock(FirebaseAnalytics.LOCK_CHECK){
                    abTestName = _constants["ab_group"].AsVariable;
                }
                
                if(!string.IsNullOrEmpty(abTestName.AsString)){
                    FirebaseAnalytics.SetCustomKey("ABGroup", abTestName.AsString);
                }

                if(abTestName.AsString.ToLower() != "default" && abTestName.AsString.ToLower() != "none"){
                    FirebaseAnalytics.SetCustomKey("LastABGroup", abTestName.AsString);
                }
            }else{
                #if UNITY_EDITOR
                Debug.LogError("[ABTest] Add 'ab_group' value in Constant");
                #endif
            }
        }

        static public void SetFirebaseInitRetries(int _maxRetries, int _retryDelayMs){
            maxRetries = _maxRetries;
            retryDelayMs = _retryDelayMs;
        }

        static private async UniTask<Firebase.DependencyStatus> checkAndFixDependenciesRetryAsync()
        {
            Firebase.DependencyStatus dependencyStatus = Firebase.DependencyStatus.UnavailableDisabled;

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    // 의존성 확인 및 해결
                    dependencyStatus = await Firebase.FirebaseApp.CheckAndFixDependenciesAsync();

                    if (dependencyStatus == Firebase.DependencyStatus.Available)
                    {
                        Analytics.Analytics.Event("DoneCheckAndFixDependencies", new Dictionary<string, object> {
                            {
                                "RealtimeSinceStartup",
                                (int)UnityEngine.Time.realtimeSinceStartup
                            },
                            {
                                "retryCount",
                                i
                            }
                        });

                        return dependencyStatus;
                    }
                    else
                    {
                        Debug.LogError($"Could not resolve Firebase dependencies: {dependencyStatus}");
                        await UniTask.Delay(retryDelayMs);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Firebase initialization failed: {e.Message}");
                    await UniTask.Delay(retryDelayMs);
                }
            }

            return dependencyStatus;
        }
        
        static public Firebase.DependencyStatus Status = Firebase.DependencyStatus.UnavilableMissing;
        static private async UniTask<Firebase.DependencyStatus> checkAndFixDependenciesAsync(IRecordContainer _constants){
            State = FirebaseUtilState.Pending;

            FirebaseAnalytics.SetUserId(TableService.UserId);
            Firebase.Analytics.FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
            Firebase.Crashlytics.Crashlytics.IsCrashlyticsCollectionEnabled = true;

            var _fbInitTask = checkAndFixDependenciesRetryAsync();
            var _result = await _fbInitTask;

            await UniTask.SwitchToMainThread();
            
            // .ContinueWithOnMainThread(async _task=>{
                if(_result == Firebase.DependencyStatus.Available){
                    try{
                        try{FirebaseAnalytics.SetCustomKey("Session", TableService.SessionCount.ToString());}catch{}
                        try{FirebaseAnalytics.SetCustomKey("Language", Application.systemLanguage.ToString());}catch{}
                        try{
                        var _installVersion = TableService.GetStringProperty(TableService.PROP_FIELD_INSTALL_VERSION, Application.version);
                        FirebaseAnalytics.SetCustomKey("SetupVersion", _installVersion);
                        FirebaseAnalytics.SetCustomKey("SetupVersionNumber", GetVersionNumber(_installVersion).ToString());
                        }catch{}
                        try{var _installDateHour = TableService.GetStringProperty(TableService.PROP_FIELD_INSTALL_DATEHOUR, DateTime.Now.ToString("yyMMddHH"));
                        var _installDateString = _installDateHour.Substring(0, 6);
                        FirebaseAnalytics.SetCustomKey("SetupDateHour", _installDateHour);
                        FirebaseAnalytics.SetCustomKey("SetupDate", _installDateString);
                        }catch{}
                        try{FirebaseAnalytics.SetCustomKey("IsSetupNow", TableService.IsSetup.ToString());}catch{}
                        try{FirebaseAnalytics.SetCustomKey("SetupDateLocal", TableService.InstallDateLocal);}catch{}
                        try{FirebaseAnalytics.SetCustomKey("DaysAfterSetup", TableService.DaysAfterInstall.ToString());}catch{}
                        
                        if(TableService.IsSetup == true){
                            Analytics.Analytics.Event("FirebaseInitOnInstall", new Dictionary<string, object> {
                                {
                                    "RealtimeSinceStartup",
                                    (int)UnityEngine.Time.realtimeSinceStartup
                                }
                            });
                        }
                    
                        Status = Firebase.DependencyStatus.Available;
                        Application.logMessageReceived += log;

                    }catch(System.Exception _error){
                        Debug.LogError("[Firebase] InitializationException property " + _error.ToString() + "/////" + _error.StackTrace);
                        BicUtil.Analytics.Analytics.LogException(_error);
                    }

                    try{
                        if(_constants != null){
                            await remoteConfigAsync(_constants);
                        }
                    }catch(AggregateException _ae){

                        Debug.LogError("[Firebase] InitializationException remoteConfigAsync ae " + _ae.ToString() + "/" + _ae.Message);

                        foreach(var _x in _ae.InnerExceptions){
                             Debug.LogError("[Firebase] AggregateException " + _x.ToString() + "/" + _x.Message + "/" + _x.Source + "/" + _x.HelpLink+ "/"  + _x.HResult+ "/"  + _x.IsOperationCanceledException().ToString()+ "/"  + _x.StackTrace );

                            if(_x.InnerException != null){
                                Debug.LogError("inner " + _x.InnerException.Message + "/" + _x.InnerException.StackTrace + "/" + _x.InnerException.Source);
                            }

                            if(_x is Firebase.FirebaseException){
                                var _e = _x as Firebase.FirebaseException;
                                Debug.LogError("[Firebase.FirebaseException] " + _e.Message + "/" + _e.ErrorCode + "/" + _e.StackTrace);
                            }
                        }

                        BicUtil.Analytics.Analytics.LogException(_ae);

                    }catch(System.Exception _error){
                        Debug.LogError("[Firebase] InitializationException remoteConfigAsync " + _error.ToString() + "/" + _error.Message);
                        if(_error.InnerException != null){
                            Debug.LogError(_error.InnerException.Message + "/" + _error.InnerException.StackTrace + "/" + _error.InnerException.Source);
                            if(_error.InnerException.InnerException != null){
                                Debug.LogError(_error.InnerException.InnerException.Message + "/" + _error.InnerException.InnerException.StackTrace + "/" + _error.InnerException.InnerException.Source);
                            }
                        }

                        BicUtil.Analytics.Analytics.LogException(_error);
                    }
                }else{
                    var _exception = new Exception("Firebase Not Available " + _result.ToString() + " isSetup = " + TableService.IsSetup.ToString());
                    _exception.Source = _result.ToString();
                    BicUtil.Analytics.Analytics.LogException(_exception);
                    BicUtil.Analytics.Analytics.Event("FirebaseNotAvailable", new Dictionary<string, object> {
                        {
                            "isSetup",
                            TableService.IsSetup
                        },
                        {
                            "result",
                            _result.ToString()
                        }
                    });
                }
            // });

            if(State == FirebaseUtilState.TimeOver){
                State = FirebaseUtilState.CompleteWithTimeOver;
            }else{
                State = FirebaseUtilState.Complete;
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

        static private async UniTask<BicDB.Result> timeoutAsync(float _time){
            await UniTask.WaitForSeconds(_time); 
            if(State == FirebaseUtilState.Pending){
                State = FirebaseUtilState.TimeOver;
                BicUtil.Analytics.Analytics.Event("FirebaseInitTimeOver");
            }
            await UniTask.DelayFrame(2);
            return new BicDB.Result(1);
        }

        static public async UniTask<BicDB.Result> InitFirebaseAsync(IRecordContainer _constants, float _timeout){
            try{
                var _initTask = await checkAndFixDependenciesAsync(_constants);
                // var _result = await UniTask.WhenAny(_initTask, timeoutAsync(_timeout));
                await UniTask.SwitchToMainThread();

                 if (_initTask == Firebase.DependencyStatus.Available) {
                    return new BicDB.Result(0);
                }else{
                    BicUtil.Analytics.Analytics.Event("FirebaseInitError");
                    return new BicDB.Result(1);
                }


                // if(_result.winArgumentIndex == 0){
                //     if (_result.result1 == Firebase.DependencyStatus.Available) {
                //         return new BicDB.Result(0);
                //     }else{
                //         BicUtil.Analytics.Analytics.Event("FirebaseInitError");
                //         return new BicDB.Result(1);
                //     }
                // }else if(_result.winArgumentIndex == 1){
                //     return new BicDB.Result(2);
                // }else{
                //     return new BicDB.Result(3);
                // }
            }catch(System.Exception _e){
                Debug.LogError("[Firebase] Error InitFirebaseAsync " + _e.Message);
                BicUtil.Analytics.Analytics.Event("FirebaseInitException", new Dictionary<string, object> {
                    {
                        "isSetup",
                        TableService.IsSetup
                    }
                });
                BicUtil.Analytics.Analytics.LogException(_e);
                return new BicDB.Result(3, "", 0, _e.Message);
            }
        }

        static private async UniTask remoteConfigAsync(IRecordContainer _constants){
            var _errorLine = 0;
            try{
            await setRemoteConfigDefaultValueAsync(_constants);
            
            #if UNITY_EDITOR
            if(tester != null){
                if(testDelay > 0f){
                    await UniTask.WaitForSeconds(testDelay);
                }

                if(ConfirmUpdateRemoteConfig != null && ConfirmUpdateRemoteConfig() == false){
                    Analytics.Analytics.Event("IgnoreUpdateRemoteConfig", new Dictionary<string, object> {{"state", State.ToString()}});
                    return;
                }

                SetConstantsValuesTester(_constants);

                if(OnFetchedRemoteConfig != null){
                    OnFetchedRemoteConfig();
                }
                return;
            }else{
                Debug.Log("[RemoteConfigTester] Use Sever Value");
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
            try{
                #if UNITY_EDITOR
                await Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.FetchAsync(_reloadTime);
                await Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.ActivateAsync();
                #else

                await FetchRemoteConfigWithRetryAsync();
                
                #endif
                _errorLine++;
            }catch(System.Exception _exception){
                try{
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
                }catch{
                    BicUtil.Analytics.Analytics.Event("FetchAsyncError", new Dictionary<string, object> {
                        {
                            "LastFetchStatus",
                            "not connect"
                        },
                        {
                            "LastFetchFailureReason",
                            "not connect"
                        }
                    });
                }

                Debug.LogError("[Firebase] remoteConfigAsync _fetchTask");
                throw _exception;
            }

            await UniTask.SwitchToMainThread();

            _errorLine++;

            if(OnFetchedRemoteConfig != null){
                OnFetchedRemoteConfig();
            }
            
            _errorLine++;
            try{
                updateConstant(_constants);
            }catch(System.Exception _exception){
                Debug.LogError("[Firebase] remoteConfigAsync updateConstant");
                throw _exception;
            }
            }catch(Firebase.FirebaseException _exception){
                Debug.LogError("Firebase remoteConfigAsync exception " + _exception.ErrorCode + "/" + _exception.Message + "/" + _errorLine + "/" + (_exception.InnerException != null ? _exception.InnerException.ToString() : ""));
                throw _exception;
            }

            #if !UNITY_EDITOR
            sendActiveABTestEvent();
            #endif


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

        static private async UniTask FetchRemoteConfigWithRetryAsync()
        {
               var remoteConfig = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance;

               // 설정된 최대 횟수만큼 재시도를 시도합니다.
               for (int attempt = 1; attempt <= maxRetries; attempt++)
               {
                   try
                   {
                       // 데이터 가져오기 및 활성화 시도
                       bool isCompleted = await remoteConfig.FetchAndActivateAsync().ContinueWith(task => task.IsCompleted);

                       if (isCompleted)
                       {
                            Analytics.Analytics.Event("DoneFetchAndActivateAsync", new Dictionary<string, object> {
                                {
                                    "RealtimeSinceStartup",
                                    (int)UnityEngine.Time.realtimeSinceStartup
                                },
                                {
                                    "Retry",
                                    (attempt - 1)
                                }
                            });
                           return; 
                       }
                   }
                   catch (Exception e)
                   {
                       // 에러가 발생하면 로그를 남깁니다.
                       Debug.LogWarning($"Attempt {attempt} failed: {e.Message}");

                       // 마지막 시도가 아니라면, 설정된 시간만큼 기다린 후 다음 시도를 진행합니다.
                       if (attempt < maxRetries)
                       {
                           Debug.Log($"Retrying in {retryDelayMs} ms...");
                           await UniTask.Delay(retryDelayMs);
                       }
                   }
               }

               // 모든 재시도가 실패한 경우 최종 에러 로그를 남깁니다.
               Debug.LogError("❌ Failed to fetch Remote Config after all retries.");
               Analytics.Analytics.Event("FirebaseFetchRemoteConfigFail", new Dictionary<string, object> {
                   {
                       "RealtimeSinceStartup",
                       (int)UnityEngine.Time.realtimeSinceStartup
                   }
               });
       }

        private static TestGroup tester = null;
        private static float testDelay = 0f; 

        public static void SetRemoteConfigTester(RemoteConfigTester _tester){
            #if UNITY_EDITOR
            if(_tester.UseTester == true){
                if(_tester.GroupIndex < 0){
                    tester = new TestGroup();
                    testDelay = _tester.UpdateDelay;
                    tester.values = new List<TestData>();
                    return;
                }
                
                tester = _tester.Groups[_tester.GroupIndex];
                testDelay = _tester.UpdateDelay;
                var _data = new TestData();
                _data.Key = "ab_group";
                _data.Value = tester.Name;
                tester.values.Add(_data);
            }
            #endif
        }

        private static void sendActiveABTestEvent()
        {
            var _eventName = "";
            if (TableService.IsSetup == true)
            {
                _eventName = "InstallConstantOn";
                BicUtil.Analytics.Analytics.Event("InstallConstant", new Dictionary<string, object> {
                        {
                            "Result",
                            "Success"
                        },
                        {
                            "RealtimeSinceStartup",
                            (int)UnityEngine.Time.realtimeSinceStartup
                        }
                    });
            }
            else
            {
                _eventName = "UpdateConstantOn";
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
                            (int)UnityEngine.Time.realtimeSinceStartup
                        }
                });
            }
            catch (Exception _e)
            {
                Debug.LogError("send InitRemoteConfigOn" + SceneManager.GetActiveScene().name + " error");
                BicUtil.Analytics.Analytics.LogException(_e);
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

        [Obsolete]
        static public void InitFirebase(){
            State = FirebaseUtilState.Pending;
            Firebase.Analytics.FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
            Firebase.Crashlytics.Crashlytics.IsCrashlyticsCollectionEnabled = true;
            Firebase.Analytics.FirebaseAnalytics.SetUserId(TableService.UserId);
            Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {

                var dependencyStatus = task.Result;
                FirebaseUtil.Status = task.Result;
                
                if (dependencyStatus == Firebase.DependencyStatus.Available) {
                    Application.logMessageReceived += log;
                }

                State = FirebaseUtilState.Complete;
            });
        }

        static public string GetRemoteConfigValue(string _key){
            #if UNITY_EDITOR
            if(tester != null){
                var _configValue = tester.values.FirstOrDefault(_row=>_row.Key == _key);
                if(_configValue != null){
                    return _configValue.Value;
                }else{
                    Debug.LogError("Not Found value for key " + _key);
                }
            }
            #endif

            return Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue(_key).StringValue;
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

        public static void SetConstantsValuesTester(IRecordContainer _constants)
        {
            if(_constants == null){
                Debug.LogError("[RemoteConfigTester] constants is null");
                return;
            }


            var _testGroup = tester;
            Debug.Log("[RemoteConfigTester] setup test value group " + _testGroup.Name + " / value count is " + _testGroup.values.Count);
            foreach(var _testData in _testGroup.values){
                if(_testData.Value != "_DEFAULT_"){
                    try{
                        _constants[_testData.Key].AsVariable.AsString = _testData.Value;
                        Debug.Log("[RemoteConfigTester] " + _testData.Key + " = " + _testData.Value);
                    }catch(System.Exception _e){
                        Debug.LogError("[RemoteConfigTester] updateConstant error " + _testData.Key);
                        throw _e;
                    }
                }else{
                    Debug.Log("[RemoteConfigTester] " + _testData.Key + " use default value");
                }
            }
        }
        #endregion

    }
}
#endif