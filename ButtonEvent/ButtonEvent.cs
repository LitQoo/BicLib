using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BicUtil.ButtonEvent{
	[System.AttributeUsage(System.AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
	sealed class SubscribeButtonAttribute : System.Attribute
	{
		readonly string eventName;
		
		public SubscribeButtonAttribute(string _eventName)
		{
			this.eventName = _eventName;
		}
		
		public string EventName
		{
			get { return eventName; }
		}
	}

	public class ButtonEvent{
		static private Dictionary<object, Dictionary<string, Action>> actionsCache = new Dictionary<object, Dictionary<string, Action>>();

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
			if(actionsCache.ContainsKey(_object) == false){
				actionsCache.Add(_object, new Dictionary<string, Action>());
			}

			if(actionsCache[_object].ContainsKey(_eventName) == false){
				actionsCache[_object].Add(_eventName, _action);
			}else{
				actionsCache[_object][_eventName] += _action;
			}
		}

		static public void SubscribeByAttribute(object _objectHavingButton, object _objectHavingEvent){
			var methods = _objectHavingEvent.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
			foreach(var _method in methods){
				SubscribeButtonAttribute _u = (SubscribeButtonAttribute)_method.GetCustomAttributes(typeof(SubscribeButtonAttribute), true).FirstOrDefault();
				if(_u != null){
					Action _action = ()=>_method.Invoke(_objectHavingEvent, BindingFlags.InvokeMethod, null, null, CultureInfo.CurrentCulture);
					addActionInCache(_objectHavingButton, _u.EventName, _action);
				}
			}
		}

		static public bool Notify(object _object, string _eventName){
			if(actionsCache.ContainsKey(_object) && actionsCache[_object].ContainsKey(_eventName)){
				actionsCache[_object][_eventName]();
				return true;
			}

			var methods = _object.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
			foreach(var _method in methods){
				SubscribeButtonAttribute _u = (SubscribeButtonAttribute)_method.GetCustomAttributes(typeof(SubscribeButtonAttribute), true).FirstOrDefault();
				if(_u != null && _u.EventName == _eventName){
					Action _action = ()=>_method.Invoke(_object, BindingFlags.InvokeMethod, null, null, CultureInfo.CurrentCulture);
					addActionInCache(_object, _eventName, _action);
					_action();
					return true;
				}
			}

			return false;
		}

		static public void ClearSubscription(object _object){
			actionsCache.Remove(_object);
		}
	}
}