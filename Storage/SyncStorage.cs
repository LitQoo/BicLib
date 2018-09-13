using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using BicDB.Container;
using BicDB.Variable;
using BicUtil.Json;

namespace BicDB.Storage
{
	public class SyncStorage : MonoBehaviour, ITableStorage {
		#region Constant
		static public string LOAD_URL_KEY = "syncstorageLoadURL";
		#endregion

		public enum ResultCode
		{
			Success = 0,
			FailedConvertJson = 1,
			ErrorNetwork = 2
		}

		#region static
		private static ITableStorage instance = null;  
		private static GameObject container;  
		public static ITableStorage GetInstance()  
		{  
			if(instance == null)  
			{  
				container = new GameObject();  
				container.name = "BicDBSyncStorage";  
				instance = container.AddComponent(typeof(SyncStorage)) as ITableStorage;  
				DontDestroyOnLoad(container);
			}  

			return instance;  
		}  
		#endregion

		#region EncryptKey
		private string encryptKey = "bicdbbicdbbicdbd";
		public void SetEncryptKey(string _key){
			encryptKey = _key.PadRight(16, '_');
		}
		#endregion

		#region IStorage
		public string StorageType{get{ return "SyncStorage"; }}
		
		public void Save<T>(ITableContainer<T> _table, Action<Result> _callback = null, object _parameter = null) where T : IRecordContainer, new() {

			string _json = string.Empty;
			JsonConvertor.GetInstance().BuildFormattedString(_table, ref _json, null);
			FileStorage.Write(_json, getFileName(_table.Name), encryptKey);

			if (_callback != null) {
				_callback(new Result((int)ResultCode.Success));
			}
		}

		public void Push<T>(ITableContainer<T> _table, Action<Result> _callback) where T : IRecordContainer, new()
		{
			//TODO: 구현해야함.
			throw new NotImplementedException("");
		}

		public void Pull<T>(ITableContainer<T> _table, Action<Result> _callback, object _parameter) where T : IRecordContainer, new (){
			loadCallback = _callback;
			StartCoroutine (GetTextFromWWW (_table));
		}

		public void Load<T>(ITableContainer<T> _table, Action<Result> _callback = null, object _parameter = null) where T : IRecordContainer, new() {
			SyncStorageParameter _param = _parameter as SyncStorageParameter;

			string _data = FileStorage.Read(getFileName(_table.Name), encryptKey);
			var _result = new Result ((int)ResultCode.Success);
			int _counter = 0;

			if (!string.IsNullOrEmpty (_data)) {
				try {
					JsonConvertor.GetInstance().BuildTableContainer(_table, ref _data, ref _counter);
				} catch (Exception) {
					_result.Code = (int)ResultCode.FailedConvertJson;
					_result.Message = ResultCode.FailedConvertJson.ToString ();
				}
			}

			if (_param.Target == SyncStorageParameter.SyncTarget.FileStorageOnly) {
				if(_callback != null) {
					_callback (_result);
				}

				return;
			}


			if (_result.Code == (int)ResultCode.Success) {
				Pull(_table, _callback, _parameter);
			} else if(_callback != null) {
				_callback (_result);
			}


		}

		private string getFileName(string _tableName){
			return FileStorage.FILE_NAME_PREFIX + _tableName;
		}

		private Action<Result> loadCallback = null;
		private IEnumerator GetTextFromWWW<T> (ITableContainer<T> _table) where T : IRecordContainer, new()
		{
			if (!_table.Header.ContainsKey (LOAD_URL_KEY)) {
				throw new SystemException ("not found Header " + LOAD_URL_KEY);
			}

			WWW www = new WWW((_table.Header[LOAD_URL_KEY] as IVariable).AsString);
			yield return www;

			var _result = new Result ((int)ResultCode.Success);
			int _counter = 0;
			string _json = www.text;

			if (www.error != null)
			{
				_result.Code = (int)ResultCode.ErrorNetwork;
				_result.Message = www.error;
			}
			else
			{
				try {
					JsonConvertor.GetInstance().BuildTableContainer(_table, ref _json, ref _counter);
				} catch (Exception) {
					_result.Code = (int)ResultCode.FailedConvertJson;
					_result.Message = ResultCode.FailedConvertJson.ToString ();
				}
			}

			if (loadCallback != null) {
				loadCallback (_result);
			}
		}
		#endregion
	}


	public class SyncStorageParameter{
		public enum SyncMode
		{
			RemoveLocalIfNotFound,
			RemoveWebIfNotFound,
			RemoveLocalAndWebIfNotFound,
			All
		}

		public enum SyncTarget
		{
			WebStorageOnly,
			FileStorageOnly,
			All
		}

		public SyncMode Mode;
		public SyncTarget Target;

		public SyncStorageParameter(SyncMode _mode, SyncTarget _target){
			Mode = _mode;
			Target = _target;
		}
	}
}
