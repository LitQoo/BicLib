using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BicUtil.EventNotifyer{
	[System.AttributeUsage(System.AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
	sealed class SubscribeEventAttribute : System.Attribute
	{
		readonly string eventName;
		
		public SubscribeEventAttribute(string _eventName)
		{
			this.eventName = _eventName;
		}
		
		public string EventName
		{
			get { return eventName; }
		}

		private Type targetType = null;
		public Type TargetType{
			get{
				return targetType;
			}

			set{
				targetType = value;
			}
		}

	}

	public class EventNotifyer{
		static private Dictionary<object, Dictionary<string, Action>> actionsCache = new Dictionary<object, Dictionary<string, Action>>();
		static private List<object> cachedSelfObjects = new List<object>();
		static public bool Notify(object[] _objects, string _eventName, bool _allowMultipleCall = false){
			bool _result = false;
			for(int i = 0; i <_objects.Length; i++){
				if(Notify(_objects[i], _eventName) == true){
					_result = true;

					if(_allowMultipleCall == false){
						return true;
					}
				}
			}

			return _result;
		}

		static private void addActionInCache(object _object, string _eventName, Action _action){
			if(_object == null){
				throw new SystemException("[EventNotifyer] target object is null, check to set object on editor");
			}
			
			if(actionsCache.ContainsKey(_object) == false){
				actionsCache.Add(_object, new Dictionary<string, Action>());
			}

			if(actionsCache[_object].ContainsKey(_eventName) == false){
				actionsCache[_object].Add(_eventName, _action);
			}else{
				actionsCache[_object][_eventName] += _action;
			}
		}

		static public void SubscribeByAttribute(object _objectHavingNotifyer, object _objectHavingMethod){
			var methods = _objectHavingMethod.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
			bool _isAdded = false;
			foreach(var _method in methods){
				var _attributes = _method.GetCustomAttributes(typeof(SubscribeEventAttribute), true);
				
				foreach(SubscribeEventAttribute _attribute in _attributes){
					if(_attribute != null && _attribute.TargetType != null){
						if((_attribute.TargetType == _objectHavingNotifyer.GetType() || _objectHavingNotifyer.GetType().IsSubclassOf(_attribute.TargetType))){
							Action _action = ()=>_method.Invoke(_objectHavingMethod, BindingFlags.InvokeMethod, null, null, CultureInfo.CurrentCulture);
							addActionInCache(_objectHavingNotifyer, _attribute.EventName, _action);
							_isAdded = true;
						}
					}
				}
			}

			if(_isAdded == false){
				throw new SystemException("[EventNotifyer] Do not added Event, check TargetType parameter\n Notifyer : " + _objectHavingNotifyer.GetType().ToString() + "\nHaving Method :" +_objectHavingMethod.GetType().ToString());
			}
		}

		static public bool Notify(object _object, string _eventName){
			
			bool _result = false;
			SubscribeBySelf(_object);

			if(actionsCache.ContainsKey(_object) && actionsCache[_object].ContainsKey(_eventName)){
				actionsCache[_object][_eventName].Invoke();
				_result = true;
			}

			return _result;
		}

		static public void SubscribeBySelf(object _object){
			if(cachedSelfObjects.Contains(_object) == false){
				var methods = _object.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
			
				foreach(var _method in methods){
					var _attributes = _method.GetCustomAttributes(typeof(SubscribeEventAttribute), true);
					foreach(SubscribeEventAttribute _buttonAttribute in _attributes){
						if(_buttonAttribute != null && _buttonAttribute.TargetType == null){
							Action _action = ()=>_method.Invoke(_object, BindingFlags.InvokeMethod, null, null, CultureInfo.CurrentCulture);
							addActionInCache(_object, _buttonAttribute.EventName, _action);
						}
					}
				}

				cachedSelfObjects.Add(_object);
			}
		}

		static public void ClearSubscription(object _object){
			actionsCache.Remove(_object);
			cachedSelfObjects.Remove(_object);
		}
	}
}