using System;
using System.Collections.Generic;
using UnityEngine;

namespace BicDB.Storage
{
	public class SyncStorage : MonoBehaviour, IStorage {
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
			string _data = FileStorage.Read(getFileName(_table.Name));
			bool _isSuccess = true;

			if (!string.IsNullOrEmpty (_data)) {
				try {
					JsonConvertor.ConvertJsonDictionaryToTable (_data, _table);
					_isSuccess = true;
				} catch (Exception) {
					_isSuccess = false;
				}
			}

			if (_callback != null) {
				_callback (_isSuccess);
			}

		}

		private string getFileName(string _tableName){
			return FileStorage.FILE_NAME_PREFIX + _tableName;
		}
		#endregion
	}
}
