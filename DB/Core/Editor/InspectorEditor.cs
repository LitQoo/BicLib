using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BicDB.Container;
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
        Vector2 scroll = new Vector2(0, 0);
        private void OnGUI()
        {
            if(GUILayout.Button("Reload") == true){
                Repaint();
            }

            if(EditorApplication.isPlaying == false){
                GUILayout.Label("Editor Mode Only", EditorStyles.largeLabel);
                
                return;
            }
            var _inspector = (TableService.Inspector as Inspector);

            if(_inspector == null){
                GUILayout.Label("inspector is null", EditorStyles.largeLabel);
                return;
            }


            scroll = GUILayout.BeginScrollView(scroll);
            GUILayout.BeginVertical();

            foreach(var _keyValue in _inspector.TrackingList)
            {
                inspectObject(_keyValue.Value, _keyValue.Key);
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }

        private void inspectObject(object _object, string _id)
        {
            Type myType = _object.GetType();
            var _fieldList = myType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            GUILayout.Label(_id, EditorStyles.boldLabel);

            for (int i = 0; i < _fieldList.Length; i++)
            {
                inspectField(_object, _fieldList[i]);
            }
        }

        private void inspectField(object _object, FieldInfo _fieldInfo)
        {

            var _field = _fieldInfo.GetValue(_object);
            var _name = _fieldInfo.Name.ToString();

            if(_name.Contains("<") == true){
                return;
            }

            GUILayout.BeginHorizontal();


            GUILayout.Space(20);
            if (_field is IVariable)
            {
                inspectVariable(_field as IVariable, _name);
            }else if(_field is IRecordContainer){
                inspectRecord(_field as IRecordContainer, _name);
            }

            GUILayout.EndHorizontal();
        }

        private void inspectRecord(IRecordContainer _record, string _name){
            
            GUILayout.BeginVertical();
            inspectObject(_record, _name);
            GUILayout.EndVertical();
        }

        private void inspectVariable(IVariable _variable, string _name)
        {
            GUILayout.Label(_variable.Type.ToString(), EditorStyles.largeLabel, GUILayout.MinWidth(50));
            GUILayout.Label(_name, EditorStyles.largeLabel, GUILayout.MinWidth(100));
            GUILayout.Label(_variable.AsString, EditorStyles.largeLabel);

            input = GUILayout.TextField(input);

            if (GUILayout.Button("Set", GUILayout.MaxWidth(30)) == true)
            {
                _variable.AsString = input;
                input = "";
            }
        }
    }


}