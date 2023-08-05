using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using BicDB;
using BicDB.Core;
using UnityEngine;

namespace BicUtil.Splash
{
    public abstract class SplashBaseAsync : MonoBehaviour
    {
        private async void Start(){
            OnSetup();
            await loadTableService();
        }

        private async Task loadTableService(){
            TableService.OnSetup += ()=>{
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
                    }
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
            
            if(_result.IsSuccess == true){
                await OnLoadedTableServiceAsync();
            }else{
                await OnFailedToLoadAsync(_result);
            }

        }

        protected abstract void OnSetup();
        protected abstract Task OnLoadedTableServiceAsync();
        protected abstract Task OnFailedToLoadAsync(Result _result);
    }
}