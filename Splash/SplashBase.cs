using System.Collections;
using System.Collections.Generic;
using BicDB;
using BicDB.Core;
using UnityEngine;

namespace BicUtil.Splash
{
    public abstract class SplashBase : MonoBehaviour
    {
        private void Awake(){
            loadTableService();
        }

        private void loadTableService(){
            TableService.OnSetup += ()=>{
                var _version = "0";
                try{
                    _version = Application.version;
                }catch{

                }

                BicUtil.Analytics.Analytics.Event("GH_Install", new Dictionary<string, object>{
                    {"version", _version}
                });

                Debug.Log("[Install] Version:" + _version);
            };

            TableService.OnUpdate += (_lastVersion, _currentVersion)=>{
                BicUtil.Analytics.Analytics.Event("Update", new Dictionary<string, object>{
                    {"from", _lastVersion},
                    {"to", _currentVersion}
                });

                Debug.Log("[Update] From:" + _lastVersion + " ,To:" + _currentVersion);
            };

            TableService.Load(OnLoadedTableService);
        }

        public void OnLoadedTableService(Result _result){
            if(_result.IsSuccess == true){
                OnLoadedTableService();
            }else{
                OnFailedToLoad(_result);
            }
        }

        protected abstract void OnLoadedTableService();
        protected abstract void OnFailedToLoad(Result _result);
    }
}