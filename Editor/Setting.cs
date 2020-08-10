using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace BicUtil.Core{
    
    public class Setting : EditorWindow {
        private DefineData[] defineList = new DefineData[]{
            new DefineData("Analytics", "Appsflyer", false, "BICUTIL_ANALYTICS_APPSFLYER"),
            new DefineData("Analytics", "Facebook", false, "BICUTIL_ANALYTICS_FB"),
            new DefineData("Analytics", "Unity", false, "BICUTIL_ANALYTICS_UNITY"),
            new DefineData("Analytics", "Firebase", false, "BICUTIL_ANALYTICS_FIREBASE"),

            new DefineData("Ads", "UnityAds", false, "BICUTIL_UNITYADS2"),
            new DefineData("Ads", "AppLovin", false, "BICUTIL_APPLOVIN"),
            new DefineData("Ads", "Admob", false, "BICUTIL_ADMOB"),
            
            new DefineData("BicTween", "UnityAnimation", false, "BICUTIL_UNITY_ANIMATOR"),
            new DefineData("BicTween", "Spine", false, "BICUTIL_SPINE"),

            new DefineData("ETC", "Unity WWW", false, "BICUTIL_WWW"),
            new DefineData("ETC", "Unity WWW TEXTURE", false, "BICUTIL_WWW_TEXTURE"),
            new DefineData("ETC", "Unity IAP", false, "BICUTIL_IAP"),
            new DefineData("ETC", "GoogleInAppReview", false, "BICUTIL_GIAR")
        };

        Vector2 scroll = new Vector2(0, 0);

        [MenuItem("Window/BicLib Setting")]
        private static void ShowWindow() {
            var window = GetWindow<Setting>("BicLib Setting");
            window.titleContent = new GUIContent("BicLib Setting");
            UnityEngine.Object.DontDestroyOnLoad(window);
        }
        
        private void OnGUI()
        {
            scroll = GUILayout.BeginScrollView(scroll);
            GUILayout.BeginVertical();
            GUILayout.Label("Define Symbol", EditorStyles.largeLabel);
            GUILayout.BeginHorizontal();
            defineSymbol(BuildTargetGroup.Unknown);
            defineSymbol(BuildTargetGroup.Standalone);
            defineSymbol(BuildTargetGroup.iOS);
            defineSymbol(BuildTargetGroup.Android);
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }

        private void defineSymbol(BuildTargetGroup _targetOS)
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            if(_targetOS != BuildTargetGroup.Unknown){
                GUILayout.Label(_targetOS.ToString());
            }else{
                GUILayout.Label("");
            }
            
            bool _isChangeSetting = false;
            var _defineSetting = PlayerSettings.GetScriptingDefineSymbolsForGroup(_targetOS);
            List<string> _defineList;

            if (_defineSetting.Contains(';') == true)
            {
                _defineList = _defineSetting.Split(';').ToList();
            }
            else if (_defineSetting.Length > 0)
            {
                _defineList = new List<string>();
                _defineList.Add(_defineSetting);
            }
            else
            {
                _defineList = new List<string>();
            }

            string _group = "";
            for (int i = 0; i < this.defineList.Length; i++)
            {
                var _data = this.defineList[i];
                if (_data.Group != _group)
                {
                    if (i != 0)
                    {
                        //GUILayout.EndHorizontal();
                        GUILayout.Space(10);
                    }
                    if(_targetOS != BuildTargetGroup.Unknown){
                        GUILayout.Label("", EditorStyles.boldLabel, GUILayout.Height(15));//_data.Group);
                    }else{
                        GUILayout.Label(_data.Group, EditorStyles.boldLabel, GUILayout.Height(15));
                    }
                    //GUILayout.BeginHorizontal();
                    _group = _data.Group;
                }

                if(_targetOS == BuildTargetGroup.Unknown){
                    GUILayout.Label(_data.Name, GUILayout.Height(15));
                    continue;
                }

                var _isEnabled = _defineList.Contains(_data.DefineName);
                _data.IsEnabled = GUILayout.Toggle(_isEnabled, "", GUILayout.Height(15));

                if (_data.IsEnabled != _isEnabled)
                {
                    _isChangeSetting = true;
                }

                if (_data.IsEnabled == true && _defineList.Contains(_data.DefineName) == false)
                {
                    _defineList.Add(_data.DefineName);
                }
            }

            //GUILayout.EndHorizontal();
            
            if(_targetOS != BuildTargetGroup.Unknown){
                for (int i = _defineList.Count - 1; i >= 0; i--)
                {
                    var _name = _defineList[i];
                    var _find = defineList.FirstOrDefault(_row => _row.DefineName == _name);
                    if (_find != null && _find.IsEnabled == false)
                    {
                        _defineList.Remove(_name);
                    }
                }

                if (_isChangeSetting == true)
                {
                    var _result = String.Join(";", _defineList);
                    Debug.Log("[BicLib Setting] Define Symbol "+ _targetOS.ToString() + " : " + _result);
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(_targetOS, _result);
                }
            }

            GUILayout.EndVertical();
        }

        private class DefineData{
            public string Group;
            public string Name;
            public bool IsEnabled;
            public string DefineName;

            public DefineData(string _group, string _name, bool _isEnabled, string _defineName){
                this.Group = _group;
                this.Name = _name;
                this.IsEnabled = _isEnabled;
                this.DefineName = _defineName;
            }
        }
    }
}