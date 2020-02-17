#if BICUTIL_ANALYTICS_FIREBASE
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BicDB.Container;
using BicDB.Core;
using BicUtil.Analytics;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BicUtil.SDKUtil
{
    public static class FirebaseUtil
    {
        #region FireBase
        static public void Log(string _message){
            UnityEngine.Debug.Log(_message);
            Firebase.Crashlytics.Crashlytics.Log(_message);
        }

        static private void setRemoteConfigDefaultValue(IRecordContainer _constants){
            Log("setRemoteConfigDefaultValue");
            var _default = new Dictionary<string, object>();
            foreach(var _value in _constants){
                _default.Add(_value.Key, _value.Value.AsVariable.AsString);
            }

            Firebase.RemoteConfig.FirebaseRemoteConfig.SetDefaults(_default);
        }

        static private void updateConstant(IRecordContainer _constants){
            Log("updateConstant");
            foreach(var _value in _constants){
                var _stringValue = Firebase.RemoteConfig.FirebaseRemoteConfig.GetValue(_value.Key).StringValue;
                if(string.IsNullOrEmpty(_stringValue) == false){
                    _value.Value.AsVariable.AsString = _stringValue;
                    Log("updateConstant " + _value.Key + "="+ _value.Value.AsVariable.AsString);
                }
            }

            if(_constants.ContainsKey("ab_group") == true){
                BicUtil.Analytics.Analytics.Instance.Event("ABGroup", new Dictionary<string, object> {
                    {
                        "GroupName",
                        _constants["ab_group"].AsVariable.AsString
                    }
                });
            }else{
                #if UNITY_EDITOR
                Debug.LogError("[ABTest] Add 'ab_group' value in Constant");
                #endif
            }
        }

        static private async Task<Firebase.DependencyStatus> checkAndFixDependenciesAsync(IRecordContainer _constants){
            Firebase.Analytics.FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
            Firebase.Analytics.FirebaseAnalytics.SetUserId(TableService.UserId);
            var _result = await Firebase.FirebaseApp.CheckAndFixDependenciesAsync();
            Log("firebase init complete "+ _result.ToString());
            BicUtil.Analytics.Analytics.Instance.Event("FirebaseInit", new Dictionary<string, object> {
                {
                    "Result",
                    _result.ToString()
                }
            });

            if (_result == Firebase.DependencyStatus.Available) {
                
                var _eventName = "";
                if(TableService.IsSetup == true){
                    _eventName = "InitRemoteConfigOn";
                }else{
                    _eventName = "OldUserRemoteConfigOn";
                }

                try{
                    _eventName = _eventName + SceneManager.GetActiveScene().name;
                    Debug.Log(_eventName);
                    BicUtil.Analytics.Analytics.Instance.Event(_eventName, new Dictionary<string, object> {
                        {
                            "Result",
                            "Success"
                        }
                    });
                }catch(Exception _e){
                    Log("send InitRemoteConfigOn error");
                    Firebase.Crashlytics.Crashlytics.LogException(_e);
                }
            }

            if(_result == Firebase.DependencyStatus.Available){
                await remoteConfigAsync(_constants, 2f);
            }

            return _result;
        }

        static public async Task<BicDB.Result> InitFirebaseAsync(IRecordContainer _constants, float _timeout){
            Firebase.Analytics.FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
            Firebase.Analytics.FirebaseAnalytics.SetUserId(TableService.UserId);
            var _initTask = checkAndFixDependenciesAsync(_constants);
            var _timeoutTask = Task.Run(async ()=>{await Task.Delay(TimeSpan.FromSeconds(_timeout)); return new BicDB.Result(1);});
            var _result = await Task.WhenAny(_initTask, _timeoutTask);

            if(_result == _initTask){
                if (_initTask.Result == Firebase.DependencyStatus.Available) {
                    return new BicDB.Result(0);
                }else{
                    return new BicDB.Result(1);
                }
            }else if(_result == _timeoutTask){
                Log("firebase init timeout");
                return new BicDB.Result(2);
            }else{
                Log("firebase init known");
                return new BicDB.Result(3);
            }
        }

        static private async Task remoteConfigAsync(IRecordContainer _constants, float _timeout){
            setRemoteConfigDefaultValue(_constants);
            
            Debug.Log("FirebaseRemoteConfig Start");

            var _asyncTask = Task.Run(async ()=>{
                Debug.Log("FirebaseRemoteConfig FetchAsync");
                var _reloadTime = TimeSpan.FromDays(1);
                #if UNITY_EDITOR
                    Debug.Log("FirebaseRemoteConfig FetchAsync EditorMode");
                    _reloadTime = TimeSpan.Zero;
                #endif
                var _fetchTask = Firebase.RemoteConfig.FirebaseRemoteConfig.FetchAsync(_reloadTime); 
                await _fetchTask.ContinueWith(FetchComplete);
                Debug.Log("FirebaseRemoteConfig ActivateFetched");
                var _isFetched = Firebase.RemoteConfig.FirebaseRemoteConfig.ActivateFetched(); 
                Debug.Log("FirebaseRemoteConfig updateConstant " + _isFetched.ToString());
                updateConstant(_constants);
                return new BicDB.Result(0);
            });

            var _timeoutTask = Task.Run(async ()=>{
                await Task.Delay(TimeSpan.FromSeconds(_timeout)); 
                return new BicDB.Result(1);
            });

            await Task.WhenAny(_asyncTask, _timeoutTask);
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
                BicUtil.Analytics.Analytics.Instance.Event("FirebaseInit", new Dictionary<string, object> {
                    {
                        "Result",
                        "Success"
                    }
                });
            } else {
                BicUtil.Analytics.Analytics.Instance.Event("FirebaseInit", new Dictionary<string, object> {
                    {
                        "Result",
                        "Fail"
                    }
                });
            }
            });
        }
        #endregion

    }
}
#endif