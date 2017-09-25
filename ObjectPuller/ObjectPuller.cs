using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace BicUtil.ObjectPuller
{
	public class ObjectPuller : MonoBehaviour 
	{
		#region LinkingObject
		[SerializeField]
		private int pullingSize = 10;
		[SerializeField]
		private string objectName;
		#endregion

		private List<IPullingObject> objectList = new List<IPullingObject>();
		private Func<IPullingObject> createFunction = null;
		private Action<IPullingObject> usingFunction = null;
		private Action<IPullingObject> pullingFunction = null;

		public void PullingObject(IPullingObject _object){
			_object.ReadyPulling ();

			if (pullingFunction != null) {
				pullingFunction (_object);
			}

			objectList.Add (_object);
		}

		public IPullingObject GetObject(){
			IPullingObject _result = null;
			if (objectList.Count > 0) {
				_result = objectList [0];
				objectList.RemoveAt (0);
			} else {
				_result = createFunction ();
			}

			_result.ReadyUsing ();

			if (usingFunction != null) {
				usingFunction (_result);
			}

			return _result;
		}

		public T GetObject<T>() where T : class{
			return GetObject() as T;
		}

		public void Init(Func<IPullingObject> _createFunction, Action<IPullingObject> _pullingFunction = null, Action<IPullingObject> _usingFunction = null){
			createFunction = _createFunction;
			usingFunction = _usingFunction;
			pullingFunction = _pullingFunction;

			for (int i = 0; i < pullingSize; i++) {
				var _object = createFunction ();
				PullingObject (_object);
			}
		}
	}

	public interface IPullingObject
	{
		void ReadyPulling();
		void ReadyUsing();
	}
}