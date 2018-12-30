using UnityEngine;
using System.Collections.Generic;
using System;

namespace BicUtil.ObjectPuller
{
    public class PrefabPuller<T> where T : class, IPullingObject{

		private List<T> objectList = new List<T>();
		private Action<T> usingFunction = null;
		private Action<T> pullingFunction = null;
		private UnityEngine.Object prefab;

		public PrefabPuller(int _readyObjectCount, string _prefabPath, Action<T> _pullingFunction = null, Action<T> _usingFunction = null) : this(_pullingFunction, _usingFunction){
			this.prefab = Resources.Load(_prefabPath);
			createReadyObject(_readyObjectCount);
		}

		public PrefabPuller(int _readyObjectCount, UnityEngine.Object _prefab, Action<T> _pullingFunction = null, Action<T> _usingFunction = null) : this(_pullingFunction, _usingFunction){
			this.prefab = _prefab;
			createReadyObject(_readyObjectCount);
		}

		public PrefabPuller(Action<T> _pullingFunction = null, Action<T> _usingFunction = null){
			usingFunction = _usingFunction;
			pullingFunction = _pullingFunction;
		}

		private void createReadyObject(int _readyObjectCount){
			for (int i = 0; i < _readyObjectCount; i++) {
				var _object = copyPrefab ();
				PullingObject (_object);
			}
		}

        private T copyPrefab(){
			GameObject _object = MonoBehaviour.Instantiate (prefab) as GameObject;
			return _object.GetComponent<T>();
		}

		public void PullingObject(T _object){
			_object.ReadyPulling ();

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
				_result = copyPrefab ();
			}

			_result.ReadyUsing ();

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