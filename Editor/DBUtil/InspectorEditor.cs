using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BicDB;
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
                GUILayout.Label("Play Mode Only", EditorStyles.largeLabel);
                
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
                inspectObject(_keyValue.Value, _keyValue.Key, 0);
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }

        private Dictionary<string, bool> foldoutState = new Dictionary<string, bool>();
        private void inspectObject(object _object, string _id, int _depth)
        {
            if(_object == null){
                GUILayout.Label(_id + " is null", EditorStyles.boldLabel);

                return;    
            }

            if(_object is ITrackedListContainer){
                if(foldoutState.ContainsKey(_id) == false){
                    foldoutState[_id] = false;
                }

                GUILayout.BeginVertical(GUI.skin.GetStyle("HelpBox"));
                foldoutState[_id] = EditorGUILayout.Foldout(foldoutState[_id], _id);
                if(foldoutState[_id] == true){
                    var _list = _object as ITrackedListContainer;
                    for(int i = 0; i < _list.ObjectCount; i++){
                        inspectObject(_list.GetObject(i), _id + "." + i, _depth+1);
                    }
                }

                GUILayout.EndVertical();
                return;
            }

            Type myType = _object.GetType();
            var _fieldList = myType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var _propList = myType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if(foldoutState.ContainsKey(_id) == false){
                foldoutState[_id] = false;
            }

            foldoutState[_id] = EditorGUILayout.Foldout(foldoutState[_id], _id);
            
            
            //GUILayout.Label(_id, EditorStyles.boldLabel);

            if(foldoutState[_id] == true){
                for (int i = 0; i < _fieldList.Length; i++)
                {
                    inspectField(_object, _fieldList[i], _depth);
                }

                for(int i = 0; i < _propList.Length; i++){
                    try{
                    inspectProperty(_object, _propList[i], _depth);
                    }catch{}
                }
            }
        }

        private void inspectProperty(object _object, PropertyInfo _propInfo, int _depth)
        {
            var _field = _propInfo.GetValue(_object);
            var _name = _propInfo.Name.ToString();


            if(_name.Contains("<") == true){
                return;
            }

            GUILayout.BeginHorizontal();


            GUILayout.Space(20);
            if (_field is IVariable)
            {
                inspectVariable(_field as IVariable, _name);
            }else if(_field is IRecordContainer){
                inspectRecord(_field as IRecordContainer, _name, _depth + 1);
            }else if(_field is ITrackedListContainer){
                inspectObject(_field, _name, _depth+1);
            }

            GUILayout.EndHorizontal();
        }

        private void inspectField(object _object, FieldInfo _fieldInfo, int _depth)
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
                inspectRecord(_field as IRecordContainer, _name, _depth + 1);
            }else if(_field is IDataBase){
                inspectObject(_field, _name, _depth+1);
            }

            GUILayout.EndHorizontal();
        }

        private void inspectRecord(IRecordContainer _record, string _name, int _depth){
            
            GUILayout.BeginVertical(GUI.skin.GetStyle("HelpBox"));
            inspectObject(_record, _name, _depth + 1);
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