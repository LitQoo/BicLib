using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace BicDB.Storage
{
	public class SyncStorage : MonoBehaviour, IStorage {
		#region Constant
		static public string LOAD_URL_KEY = "syncstorageLoadURL";
		static public string ENCRYPT_KEY = "ecky";
		#endregion

		public enum ResultCode
		{
			Success = 0,
			FailedConvertJson = 1,
			ErrorNetwork = 2
		}

		#region static
		private static IStorage instance = null;  
		private static GameObject container;  
		public static IStorage GetInstance()  
		{  
			if(instance == null)  
			{  
				container = new GameObject();  
				container.name = "BicDBSyncStorage";  
				instance = container.AddComponent(typeof(SyncStorage)) as IStorage;  
				DontDestroyOnLoad(container);
			}  

			return instance;  
		}  
		#endregion

		#region IStorage
		public void Save<T>(ITable<T> _table, Action<Result> _callback = null, object _parameter = null) where T : IModel, new() {
			string _encKey = FileStorage.GetDefaultEncryptKey();
			if (_encKey == string.Empty || _table.Header.ContainsKey(ENCRYPT_KEY)) {
				_encKey = _table.Header[ENCRYPT_KEY].AsString.PadRight(16, '_');
			}

			FileStorage.Write(JsonConvertor.ConvertTableToJsonString(_table), getFileName(_table.Name), _encKey);
			if (_callback != null) {
				_callback(new Result((int)ResultCode.Success));
			}
		}

		public void Load<T>(ITable<T> _table, Action<Result> _callback = null, object _parameter = null) where T : IModel, new() {
			SyncStorageParameter _param = _parameter as SyncStorageParameter;
			string _encKey = FileStorage.GetDefaultEncryptKey();
			if (_encKey == string.Empty || _table.Header.ContainsKey(ENCRYPT_KEY)) {
				_encKey = _table.Header[ENCRYPT_KEY].AsString.PadRight(16, '_');
			}

			string _data = FileStorage.Read(getFileName(_table.Name), _encKey);
			var _result = new Result ((int)ResultCode.Success);

			if (!string.IsNullOrEmpty (_data)) {
				try {
					JsonConvertor.ConvertJsonDictionaryToTable (_data, _table);
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
				loadCallback = _callback;
				StartCoroutine (GetTextFromWWW (_table));
			} else if(_callback != null) {
				_callback (_result);
			}

		}

		private string getFileName(string _tableName){
			return FileStorage.FILE_NAME_PREFIX + _tableName;
		}

		private Action<Result> loadCallback = null;
		private IEnumerator GetTextFromWWW<T> (ITable<T> _table) where T : IModel, new()
		{
			if (!_table.Header.ContainsKey (LOAD_URL_KEY)) {
				throw new SystemException ("not found Header " + LOAD_URL_KEY);
			}

			WWW www = new WWW(_table.Header[LOAD_URL_KEY].AsString);
			yield return www;

			var _result = new Result ((int)ResultCode.Success);

			if (www.error != null)
			{
				_result.Code = (int)ResultCode.ErrorNetwork;
				_result.Message = www.error;
			}
			else
			{
				try {
					JsonConvertor.ConvertJsonDictionaryToTableThenUpdate(www.text, _table);
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
