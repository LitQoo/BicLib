using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BicDB;
using BicUtil.SingletonBase;
using BicDB.Container;
using System;

namespace BicDB.Storage {
	public class ScriptableStorage : MonoBehaviourHardBase<ScriptableStorage> , ITableStorage {

		public enum ResultCode
		{
			Success = 0,
			NeedParam = 1,
			FailedLoad = 2
		}

		#region IStorage
		public string StorageType{get{ return "ScriptableStorage"; }}
		
		public void Save<T> (ITableContainer<T> _table, System.Action<Result> _callback, object _parameter = null) where T : IRecordContainer, new ()
		{
			throw new System.NotImplementedException ();
		}

		public void Push<T>(ITableContainer<T> _table, Action<Result> _callback) where T : IRecordContainer, new()
		{
			Save(_table, _callback);
		}

		public void Load<T> (ITableContainer<T> _table, System.Action<Result> _callback, object _parameter) where T : IRecordContainer, new ()
		{
			var _param = _parameter as ScriptableStorageParameter;

			if (_param == null && _callback != null) {
				_callback (new Result ((int)ResultCode.NeedParam, string.Empty));
				return;
			}

			var _scriptableData = Resources.Load<ScriptableDataBase<T>> (_param.Path);

			if (_scriptableData == null && _callback != null) {
				_callback (new Result ((int)ResultCode.FailedLoad, _param.Path));
				return;
			}
				
			for (int i = 0; i < _scriptableData.List.Count; i++) {
				var _record = _scriptableData.List [i] as ScriptableRecordContainer;
				if (_record == null) {
					throw new System.Exception ("Need to use ScriptableRecordContainer");
				}

				_record.ApplyInspectorValue ();

				_table.Add (_scriptableData.List[i]);
			}

			if (_callback != null) {
				_callback(new Result ((int)ResultCode.Success, _param.Path));
			}
		}

		public void Pull<T> (ITableContainer<T> _table, System.Action<Result> _callback, object _parameter) where T : IRecordContainer, new ()
		{
			throw new System.NotImplementedException ();
		}
		#endregion
	}

	public class ScriptableStorageParameter{
		public string Path;

		public ScriptableStorageParameter(string _path){
			Path = _path;
		}
	}
}