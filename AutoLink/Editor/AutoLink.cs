using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BicUtil.AutoLink{
	public static class AutoLinkEditor {
		[MenuItem("GameObject/AutoLink", false, 0)]
		static void StartLinkSerializeField() {
			for(int i = 0; i < Selection.objects.Length; i++){
				autoLinkSerialField(Selection.objects[i]);
			}
		}

		static void autoLink(object _object){
			var _gameObject = _object as GameObject;
			var _components = _gameObject.GetComponents(typeof(MonoBehaviour));
			
			foreach(var _component in _components){
				var _fields = _component.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public| BindingFlags.Instance);
				foreach(var _field in _fields){
					var _attributes = _field.GetCustomAttributes(typeof(AutoLinkAttribute), true);
					foreach(AutoLinkAttribute _attribute in _attributes){
						var _findName = _attribute.ObjectName;
						if(_findName == ""){
							_findName = _field.Name;
						}

						var _childList = new List<GameObject>();
						getChildAll(_gameObject, _childList);
						for(int i = 0; i < _childList.Count; i++){
							var _child = _childList[i];
							if(_child.name.ToLower() == _findName.ToLower()){
								var _comp = _child.gameObject.GetComponent(_field.FieldType);
								if(_comp != null){
									_field.SetValue(_component, _comp);
								}
								break;
							}
						}
					}
				}
			}
		}

		static void autoLinkSerialField(object _object){
			var _gameObject = _object as GameObject;
			var _components = _gameObject.GetComponents(typeof(MonoBehaviour));

			foreach(var _component in _components){
				var _fields = _component.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public| BindingFlags.Instance);
				foreach(var _field in _fields){
					var _attributes = _field.GetCustomAttributes(typeof(SerializeField), true);
					if(_attributes.Length > 0)
                    {
                        var _childList = new List<GameObject>();
                        getChildAll(_gameObject, _childList);
                        setField(_component, _field, _childList);
                    }
                }
			}
		}

        private static void setField(Component _component, FieldInfo _field, List<GameObject> _childList)
        {
            for (int i = 0; i < _childList.Count; i++)
            {
                var _child = _childList[i];
                if (_child.name.ToLower() == _field.Name.ToLower())
                {
                    var _comp = _child.gameObject.GetComponent(_field.FieldType);
                    if (_comp != null)
                    {
                        _field.SetValue(_component, _comp);
                    }
                    break;
                }

                if (_field.FieldType.IsArray == true)
                {
                    setArrayField(_component, _field, _child);
                }
            }
        }

        private static void setArrayField(Component _component, FieldInfo _field, GameObject _child)
        {
            var _arrayName = _field.Name.ToLower().Replace("List", string.Empty);
            if (_child.name.ToLower().Contains(_arrayName.Substring(0, _arrayName.Length - 1)) == true)
            {
                var _comp = _child.gameObject.GetComponent(_field.FieldType.GetElementType());
                if (_comp != null)
                {
                    Array array = _field.GetValue(_component) as Array;
                    var _length = array.Length;
                    for (int j = 0; j < array.Length; j++)
                    {
                        if (array.GetValue(j) == null)
                        {
                            array.SetValue(_comp, j);
                            break;
                        }
                    }
                }
            }
        }

        static void getChildAll(GameObject _gameObject, List<GameObject> _list){
			List<GameObject> _result = new List<GameObject>();
			for(int i = 0; i < _gameObject.transform.childCount; i++){
				_list.Add(_gameObject.transform.GetChild(i).gameObject);
			}

			for(int i = 0; i < _gameObject.transform.childCount; i++){
				getChildAll(_gameObject.transform.GetChild(i).gameObject, _list);
			}
		}
	}
}