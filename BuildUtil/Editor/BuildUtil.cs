using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.iOS.Xcode;
using System.IO;

namespace BicLib.BuildUtil{
    [InitializeOnLoad]
    static public class BuildUtil{

        [MenuItem("BicLib/Increase BuildNumber", false, 100)]
        static void IncreaseBuildNumber(){
            int _buildNumber = PlayerSettings.Android.bundleVersionCode + 1;
            Debug.Log("[BuildUtil] buildNumber " + _buildNumber);
            PlayerSettings.Android.bundleVersionCode = _buildNumber;
            PlayerSettings.iOS.buildNumber = _buildNumber.ToString();
            AssetDatabase.SaveAssets();
        }


        public static string GetBuildString(){
            int _iosNumber = 0;
            try{
                _iosNumber = int.Parse(PlayerSettings.iOS.buildNumber);
            }catch{

            }

            return "v" + PlayerSettings.bundleVersion + "(" + Mathf.Max(PlayerSettings.Android.bundleVersionCode, _iosNumber) + ")";
        }

        static void IncrementVersion(int majorIncr, int minorIncr, int buildIncr)
        {
            var _oldVersion = PlayerSettings.bundleVersion;
            var _oldNumber = PlayerSettings.Android.bundleVersionCode;

            string[] lines = PlayerSettings.bundleVersion.Split('.');

            int MajorVersion = int.Parse(lines[0]) + majorIncr;

            int MinorVersion = int.Parse(lines[1]) + minorIncr;
            int Build = int.Parse(lines[2]) + buildIncr;

            if(majorIncr > 0){
                MinorVersion = 0;
                Build = 0;
            }

            if(minorIncr > 0){
                Build = 0;
            }

            PlayerSettings.bundleVersion = MajorVersion.ToString("0") + "." +
                                            MinorVersion.ToString("0") + "." +
                                            Build.ToString("0");
            
            IncreaseBuildNumber();

            Debug.LogWarning("Version "+_oldVersion + "("+_oldNumber+") to " + PlayerSettings.bundleVersion + "(" + PlayerSettings.Android.bundleVersionCode +")");

        }


        [MenuItem("BicLib/Increase v0.0.+", false, 101)]
        private static void IncreaseLast()
        {
            IncrementVersion(0, 0, 1);
        }

        [MenuItem("BicLib/Increase v0.+.0", false, 102)]
        private static void IncreaseMinor()
        {
            IncrementVersion(0, 1, 0);
        }

        [MenuItem("BicLib/Increase v+.0.0", false, 103)]
        private static void IncreaseMajor()
        {
            IncrementVersion(1, 0, 0);
        }
    }

    public class BuildUtiliOS
    {
        private string pbxProjectPath;
        private string plistPath;
        private PBXProject pbxProject;
        private PlistDocument plistObj;

        public BuildUtiliOS(string _pathToBuiltProject){
            this.pbxProjectPath = PBXProject.GetPBXProjectPath(_pathToBuiltProject);
            this.pbxProject = new PBXProject();
            pbxProject.ReadFromFile(pbxProjectPath);

            this.plistPath = _pathToBuiltProject + "/Info.plist";
            this.plistObj = new PlistDocument();
            plistObj.ReadFromString(File.ReadAllText(plistPath));

        }

        public void SaveAll(){
            SavePbxProject();
            SaveInfoPlist();
        }

        public void SaveInfoPlist(){
            File.WriteAllText(plistPath, plistObj.WriteToString());
        }
        public void SavePbxProject(){
            pbxProject.WriteToFile(pbxProjectPath);
        }

        public void EnableBitCode(bool _isEnabled){
            Debug.Log("[BuildUtil] EnableBitCode " + _isEnabled.ToString());
            string[] targetGuids = new string[2] { pbxProject.GetUnityMainTargetGuid(), pbxProject.GetUnityFrameworkTargetGuid() };
            pbxProject.SetBuildProperty(targetGuids, "ENABLE_BITCODE", _isEnabled == true ? "YES" : "NO");
        }

        public void AddFrameworkToMainTarget(string _framework){
            var _targetGuid = pbxProject.GetUnityMainTargetGuid();
            
            pbxProject.AddFrameworkToProject(_targetGuid, _framework, true);
        }

        public void SetAttMessage(string _message = "This identifier will be used to deliver personalized ads to you."){
            Debug.Log("[BuildUtil] SetAttMessage " + _message);
            plistObj.root.SetString("NSUserTrackingUsageDescription", _message);
        }

        public void SetNoneExemptEncryption(){
            Debug.Log("[BuildUtil] ITSAppUsesNonExemptEncryption false");
            plistObj.root.SetBoolean("ITSAppUsesNonExemptEncryption", false);
        }

        public void RemoveUIApplicationExitsOnSuspend(){
            Debug.Log("[BuildUtil] RemoveUIApplicationExitsOnSuspend");

            string exitsOnSuspendKey = "UIApplicationExitsOnSuspend";
            if(plistObj.root.values.ContainsKey(exitsOnSuspendKey))
            {
               plistObj.root.values.Remove(exitsOnSuspendKey);
            }
        }

        public void EditRequiredDeviceCapabilitiesOnlyArmv7()
        {
            Debug.Log("[BuildUtil] EditRequiredDeviceCapabilitiesOnlyArmv7");
            
            var _uiCap = plistObj.root.values["UIRequiredDeviceCapabilities"].AsArray();
            removeString(_uiCap, "arm64");
            addString(_uiCap, "armv7");
        }

        private void removeString(PlistElementArray _uiCap, string _value)
        {
            for (int i = _uiCap.values.Count - 1; i >= 0; i--)
            {
                if (_uiCap.values[i].AsString() == _value)
                {
                    _uiCap.values.RemoveAt(i);
                }
            }
        }

        private void addString(PlistElementArray _uiCap, string _value)
        {
            var _find = false;
            for (int i = 0; i < _uiCap.values.Count; i++)
            {
                if (_uiCap.values[i].AsString() == _value)
                {
                    _find = true;
                    break;
                }
            }

            if (_find == false)
            {
                _uiCap.AddString(_value);
            }
        }
    }
}