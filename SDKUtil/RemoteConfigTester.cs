using System.Collections;
using System.Collections.Generic;
using BicDB.Container;
using UnityEngine;

namespace BicUtil.SDKUtil
{
    public class RemoteConfigTester : MonoBehaviour
    {
        [SerializeField]
        public bool UseTester = false;
        [SerializeField]
        public int GroupIndex = -1;
        [SerializeField]
        public List<TestGroup> Groups;

        private void Awake() {
            Debug.Log("awake remoteconfigteseter");   
        }
        
        public void SetConstantsValues(IRecordContainer _constants)
        {
            if(_constants == null){
                Debug.LogError("[RemoteConfigTester] constants is null");
                return;
            }

            if(this.GroupIndex < 0){
                Debug.Log("[RemoteConfigTester] user default value");
            }

            if(this.Groups.Count <= this.GroupIndex){
                Debug.LogError("[RemoteConfigTester] error groupIndex " + this.GroupIndex + " is not support");
                return;
            }

            var _testGroup = this.Groups[this.GroupIndex];

            Debug.Log("[RemoteConfigTester] setup test value group " + _testGroup.Name);
            foreach(var _testData in _testGroup.values){
                if(string.IsNullOrEmpty(_testData.Value) == false){
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
    }

    [System.Serializable]
    public class TestGroup{
        public string Name;
        public List<TestData> values;
    }

    [System.Serializable]
    public class TestData{
        public string Key;
        public string Value;
    }
}