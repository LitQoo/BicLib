using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BicUtil.AutoLink{
	public static class AutoLinkEditor {
		[MenuItem("GameObject/AutoLink", false, 0)]
		static void StartLink() {
			for(int i = 0; i < Selection.objects.Length; i++){
				autoLink(Selection.objects[i]);
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
						var _childList = new List<GameObject>();
						getChildAll(_gameObject, _childList);
						for(int i = 0; i < _childList.Count; i++){
							var _child = _childList[i];
							if(_child.name == _attribute.ObjectName){
								_field.SetValue(_component, _child.gameObject.GetComponent(_field.FieldType));
								break;
							}
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