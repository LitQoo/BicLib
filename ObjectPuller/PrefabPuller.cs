using UnityEngine;
using System.Collections.Generic;
using System;

namespace BicUtil.ObjectPuller
{
    public class PrefabPuller<T> where T : class{

		private List<T> objectList = new List<T>();
		private Action<T> usingFunction = null;
		private Action<T> pullingFunction = null;
		private UnityEngine.Object prefab;

		public PrefabPuller(int _readyObjectCount, string _prefabPath, Action<T> _pullingFunction = null, Action<T> _usingFunction = null) : this(_pullingFunction, _usingFunction){
			this.prefab = Resources.Load(_prefabPath);
			this.createFunc = copyPrefab;
			createReadyObject(_readyObjectCount);
		}

		public PrefabPuller(int _readyObjectCount, UnityEngine.Object _prefab, Action<T> _pullingFunction = null, Action<T> _usingFunction = null) : this(_pullingFunction, _usingFunction){
			this.prefab = _prefab;
			this.createFunc = copyPrefab;
			createReadyObject(_readyObjectCount);
		}

		public PrefabPuller(int _readyObjectCount, Func<T> _createFunc, Action<T> _pullingFunction = null, Action<T> _usingFunction = null) : this(_pullingFunction, _usingFunction){
			this.createFunc = _createFunc;
			createReadyObject(_readyObjectCount);
		}

		public PrefabPuller(Action<T> _pullingFunction = null, Action<T> _usingFunction = null){
			usingFunction = _usingFunction;
			pullingFunction = _pullingFunction;
		}

		private void createReadyObject(int _readyObjectCount){
			for (int i = 0; i < _readyObjectCount; i++) {
				var _object = createFunc ();
				PullingObject (_object);
			}
		}

        private T copyPrefab(){
			GameObject _object = MonoBehaviour.Instantiate (prefab) as GameObject;
			return _object.GetComponent<T>();
		}

		private Func<T> createFunc = null;

		public void PullingObject(T _object){
			if(_object == null){
				return;
			}
			

			var _pullingObject = _object as IPullingObject;
			if(_pullingObject != null){
				_pullingObject.ReadyPulling ();
			}

			if (pullingFunction != null) {
				pullingFunction (_object);
			}

			objectList.Add (_object);
		}

		public T GetObject(){
			T _result = null;
			if (objectList.Count > 0) {
				_result = objectList [0];
				objectList.RemoveAt (0);
			} else {
				_result = createFunc ();
			}

			var _pullingObject = _result as IPullingObject;
			if(_pullingObject != null){
				_pullingObject.ReadyUsing ();
			}

			if (usingFunction != null) {
				usingFunction (_result);
			}

			return _result;
		}
		
		public void Clear(){
			objectList.Clear();
		}
	}
}