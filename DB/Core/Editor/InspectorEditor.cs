using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BicDB.Core;
using BicDB.Variable;
using UnityEditor;
using UnityEngine;

namespace BicUtil.Core{
    
    public class InspectorEditor : EditorWindow
    {
        
        [MenuItem("Window/BicLib/Inspector")]
        private static void ShowWindow() {
            var window = GetWindow<InspectorEditor>("BicDB Inspector");
            window.titleContent = new GUIContent("BicDB Inspector");
            UnityEngine.Object.DontDestroyOnLoad(window);
        }


        string input = "";
        private void OnGUI()
        {
            if(GUILayout.Button("Reload") == true){
                Repaint();
            }

            if(EditorApplication.isPlaying == false){
                GUILayout.Label("Editor Mode Only", EditorStyles.largeLabel);
                
                return;
            }

            GUILayout.BeginVertical();
            var _inspector = (TableService.Inspector as Inspector);

            if(_inspector == null){
                GUILayout.Label("inspector is null", EditorStyles.largeLabel);
                return;
            }


            foreach(var _keyValue in _inspector.TrackingList){
                var _object = _keyValue.Value;

                Type myType = _object.GetType();
                var _fieldList = myType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                
                GUILayout.Label(_keyValue.Key, EditorStyles.boldLabel);
                
                for (int i =0 ; i < _fieldList.Length ; i++)
                {
                    
                    GUILayout.BeginHorizontal();
                    // Display name and type of the concerned member.
                    var _fieldType = _fieldList[i].FieldType.ToString();
                    if(_fieldType.Contains(".")){
                        var _split = _fieldType.Split('.');
                        _fieldType = _split[_split.Length - 1];
                    }
                    GUILayout.Label(_fieldType, EditorStyles.largeLabel, GUILayout.MinWidth(100));
                    GUILayout.Label(_fieldList[i].Name.ToString(), EditorStyles.largeLabel, GUILayout.MinWidth(100));

                    var _field = _fieldList[i].GetValue(_object);

                    if(_field is IVariable){
                        var _variable = (_field as IVariable);
                        GUILayout.Label(_variable.AsString, EditorStyles.largeLabel);
                        input = GUILayout.TextField(input);

                        if(GUILayout.Button("Set", GUILayout.MaxWidth(30)) == true){
                            _variable.AsString = input; 
                            input = "";
                        }
                    } 
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();

            // GUILayout.BeginVertical();
            // GUILayout.Label("Define Symbol", EditorStyles.largeLabel);
            // GUILayout.BeginHorizontal();
            // GUILayout.Label("", EditorStyles.boldLabel, GUILayout.Height(15));

            // GUILayout.EndHorizontal();
            // GUILayout.EndVertical();
        }
    }


}