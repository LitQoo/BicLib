using System.Collections;
using UnityEngine;
using System.IO;
using BicDB.Storage;
using System;
using BicDB.Container;
using BicUtil.Json;

namespace BicDB.Storage{

	public class ResourceStorage : ITableStorage {
		static public string FILE_NAME_PREFIX = "bdb_"; 

		public enum ResultCode
		{
			Success = 0,
			FailedConvertJson = 1
		}
		#region static
		static private ITableStorage instance = null;
		static public ITableStorage GetInstance(){
			if (instance == null) {
				instance = new ResourceStorage();
			}

			return instance;
		}
		#endregion

		#region IStorage
		public void Save<T>(ITableContainer<T> _table, Action<Result> _callback = null, object _parameter = null) where T : IRecordContainer, new() {
			if (_callback != null) {
				_callback(new Result((int)ResultCode.Success));
			}
		}

		public void Pull<T>(ITableContainer<T> _table, Action<Result> _callback, object _parameter) where T : IRecordContainer, new ()
		{
			loadByResourceFile(_table, _callback, _parameter);
		}

		public void Load<T>(ITableContainer<T> _table, Action<Result> _callback = null, object _parameter = null) where T : IRecordContainer, new() {
			_table.Clear();
			loadByResourceFile(_table, _callback, _parameter);
		}

		private void loadByResourceFile<T>(ITableContainer<T> _table, Action<Result> _callback = null, object _parameter = null) where T : IRecordContainer, new() {

			string _filePath = "BicDB/" + getFileName(_table.Name) + ".json";

			if (_parameter != null) {
				ResourceStorageParameter _param = _parameter as ResourceStorageParameter;
				_filePath = _param.Path;

			}

			string _data = Read(_filePath);
			var _result = new Result ((int)ResultCode.Success);
			int _counter = 0;

			if (!string.IsNullOrEmpty(_data)) {
				try{
					JsonConvertor.GetInstance().BuildTableContainer(_table, ref _data, ref _counter);
				}catch(SystemException){
					_result.Code = (int)ResultCode.FailedConvertJson;
					_result.Message = ResultCode.FailedConvertJson.ToString ();
				}
			}

			if (_callback != null) {
				_callback(_result);
			}
		}

		private string getFileName(string _tableName){
			return FILE_NAME_PREFIX + _tableName;
		}
		#endregion

		#region ResourceControl
		public string Read(string _filePath){
			#if UNITY_EDITOR
			string tPath = Application.dataPath + "/Resources/" + _filePath;

			if (File.Exists(tPath))
			{
				FileStream tFile = new FileStream (tPath, FileMode.Open, FileAccess.Read);
				StreamReader tStream = new StreamReader(tFile);
				string tStr = string.Empty;
				while(!tStream.EndOfStream){
					tStr += tStream.ReadLine ();
				}
				tStream.Close();
				tFile.Close();
				return tStr;
			}
			else
			{
				Debug.Log("not found json file Resources/" + _filePath);
				return null;
			}
			#else
			TextAsset tText = Resources.Load<TextAsset>(_filePath);
			return tText.text;
			#endif 
		}
		#endregion
	}

	public class ResourceStorageParameter{
		public string Path;

		public ResourceStorageParameter(string _path){
			Path = _path;
		}
	}
}