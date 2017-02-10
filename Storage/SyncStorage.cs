using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace BicDB.Storage
{
	public class SyncStorage : MonoBehaviour, IStorage {
		#region Static
		static public string LOAD_URL_KEY = "syncstorageLoadURL";
		#endregion

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
		public void Save<T>(ITable<T> _table, Action<bool> _callback = null, object _parameter = null) where T : IModel, new() {
			FileStorage.Write(JsonConvertor.ConvertTableToJsonString(_table), getFileName(_table.Name));
			if (_callback != null) {
				_callback(true);
			}
		}

		public void Load<T>(ITable<T> _table, Action<bool> _callback = null, object _parameter = null) where T : IModel, new() {
			SyncStorageParameter _param = _parameter as SyncStorageParameter;

			string _data = FileStorage.Read(getFileName(_table.Name));
			bool _isSuccess = true;

			if (_param.Target == SyncStorageParameter.SyncTarget.FileStorageOnly) {
				if(_callback != null) {
					_callback (_isSuccess);
				}

				return;
			}

			if (!string.IsNullOrEmpty (_data)) {
				try {
					JsonConvertor.ConvertJsonDictionaryToTable (_data, _table);
					_isSuccess = true;
				} catch (Exception) {
					_isSuccess = false;
				}
			}

			if (_isSuccess) {
				loadCallback = _callback;
				StartCoroutine (GetTextFromWWW (_table));
			} else if(_callback != null) {
				_callback (_isSuccess);
			}

		}

		private string getFileName(string _tableName){
			return FileStorage.FILE_NAME_PREFIX + _tableName;
		}

		private Action<bool> loadCallback = null;
		private IEnumerator GetTextFromWWW<T> (ITable<T> _table) where T : IModel, new()
		{
			if (!_table.ContainsHeader (LOAD_URL_KEY)) {
				throw new SystemException ("not found Header " + LOAD_URL_KEY);
			}

			WWW www = new WWW(_table.GetHeader(LOAD_URL_KEY).AsString);
			yield return www;

			bool _isSuccess = false;

			if (www.error != null)
			{
				_isSuccess = false;
			}
			else
			{
				_isSuccess = true;

				try {
					JsonConvertor.ConvertJsonDictionaryToTableThenUpdate(www.text, _table);
				} catch (Exception) {
					_isSuccess = false;
				}
			}

			if (loadCallback != null) {
				loadCallback (_isSuccess);
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
