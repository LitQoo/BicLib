using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using BicDB;
using BicDB.Core;
using UnityEngine;

namespace BicUtil.Splash
{
    public abstract class SplashBaseAsync : MonoBehaviour
    {
        private void Start(){
            OnSetup();
            loadTableService().Forget();
        }

        private async UniTaskVoid loadTableService(){
            TableService.OnSetup += (_result)=>{
                var _version = "0";
                try{
                    _version = Application.version;
                }catch{
                }

                BicUtil.Analytics.Analytics.Event("Install", new Dictionary<string, object>{
                    {"version", _version},
                    {
                        "RealtimeSinceStartup",
                        UnityEngine.Time.realtimeSinceStartup
                    },
                    {"result", _result.Code}
                });
            };

            TableService.OnUpdate += (_lastVersion, _currentVersion)=>{
                BicUtil.Analytics.Analytics.Event("Update", new Dictionary<string, object>{
                    {"from", _lastVersion},
                    {"to", _currentVersion}
                });

                Debug.Log("[Update] From:" + _lastVersion + " ,To:" + _currentVersion);
            };

            var _result = await TableService.LoadAsync();

            try{
                if(_result.IsSuccess == true){
                    await OnLoadedTableServiceAsync();
                    return;
                }
            }catch(System.Exception _e){
                Debug.LogError(_e.ToString());
                await callFailedToLoadAsync(new Result(9, "", 0, "OnLoadedTableServiceAsync error"), "SplashFailedAfterDestoryed1");
                return;
            }
            
            await callFailedToLoadAsync(_result, "SplashFailedAfterDestoryed2");
        }

        private async UniTask callFailedToLoadAsync(Result _result, string _eventName){
            try{
                if(_isDestoryed == true || ReferenceEquals(this.gameObject, null) || this.gameObject == null){
                    BicUtil.Analytics.Analytics.Event("SplashFailedAfterDestoryed2");
                }else{
                    await OnFailedToLoadAsync(_result);
                }
            }catch(System.Exception _e){
                Debug.LogError("SplashFailedAfterDestoryed3 "+_e.ToString());
                BicUtil.Analytics.Analytics.Event("SplashFailedAfterDestoryed3");
            }
        }

        bool _isDestoryed = false;
        private void OnDestroy() {
            _isDestoryed = true;
        }

        protected abstract void OnSetup();
        protected abstract UniTask OnLoadedTableServiceAsync();
        protected abstract UniTask OnFailedToLoadAsync(Result _result);
    }

}