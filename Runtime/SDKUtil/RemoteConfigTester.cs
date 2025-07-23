using System.Collections;
using System.Collections.Generic;
using BicDB.Container;
using UnityEngine;

namespace BicUtil.SDKUtil
{
    public class RemoteConfigTester : MonoBehaviour
    {
        [SerializeField]
        public float UpdateDelay = 0f;
        [SerializeField]
        public bool UseTester = false;
        [SerializeField]
        public int GroupIndex = -1;
        [SerializeField]
        public List<TestGroup> Groups;
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