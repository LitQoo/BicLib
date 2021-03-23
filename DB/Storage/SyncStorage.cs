#if BICUTIL_WWW
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using BicDB.Container;
using BicDB.Variable;
using BicUtil.Json;
using UnityEngine.Networking;
using System.Threading.Tasks;

namespace BicDB.Storage
{
	public class SyncStorage : MonoBehaviour, ITableStorage {
		#region Constant
		static public string LOAD_URL_KEY = "syncstorageLoadURL";
		static public string ENCRYPT_KEY = "syncstorageEncryptKey";
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
		private string getEncryptKey<T>(ITableContainer<T> _table) where T : IRecordContainer, new() {
			string _result = this.encryptKey;
			if (_table.Header.ContainsKey (ENCRYPT_KEY) == true) {
				_result = _table.Header[ENCRYPT_KEY].AsVariable.AsString;
			}

			return _result.PadRight(16, '_');
		}
		#endregion

		#region IStorage
		public string StorageType{get{ return "SyncStorage"; }}
		
		public void Save<T>(ITableContainer<T> _table, Action<Result> _callback = null, object _parameter = null) where T : IRecordContainer, new() {

			System.Text.StringBuilder _stringBuilder = new System.Text.StringBuilder();
			string _json = JsonConvertor.GetInstance().ToFormattedString(_table);
			FileStorageUtil.Write(_json, getFileName(_table.Name), getEncryptKey(_table));

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

			var _result = new Result ((int)ResultCode.Success);
			if(_param.Target != SyncStorageParameter.SyncTarget.WebStorageOnly){
				string _data = FileStorageUtil.ReadAndDecrypt(FileStorageUtil.GetPath(getFileName(_table.Name)), getEncryptKey(_table));
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

			string _url = (_table.Header[LOAD_URL_KEY] as IVariable).AsString;
			UnityWebRequest www = UnityWebRequest.Get(_url);
			yield return www.SendWebRequest();

			var _result = new Result ((int)ResultCode.Success);
			int _counter = 0;

			if (www.error != null || www.result != UnityWebRequest.Result.Success)
			{
				_result.Code = (int)ResultCode.ErrorNetwork;
				_result.Message = www.result.ToString();
			}
			else
			{
				string _json = www.downloadHandler.text;

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

        public Task<Result> LoadAsync<T>(ITableContainer<T> _table, object _parameter) where T : IRecordContainer, new()
        {
            throw new NotImplementedException();
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
#endif