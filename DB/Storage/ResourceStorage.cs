using System.Runtime.Versioning;
using System.Collections;
using UnityEngine;
using System.IO;
using BicDB.Storage;
using System;
using BicDB.Container;
using BicUtil.Json;
using System.Threading.Tasks;

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
		public string StorageType{get{ return "ResourceStorage"; }}
		
		public void Save<T>(ITableContainer<T> _table, Action<Result> _callback = null, object _parameter = null) where T : IRecordContainer, new() {
			if (_callback != null) {
				_callback(new Result((int)ResultCode.Success, string.Empty));
			}
		}

		public void Push<T>(ITableContainer<T> _table, Action<Result> _callback) where T : IRecordContainer, new()
		{
			Save(_table, _callback);
		}

		public void Pull<T>(ITableContainer<T> _table, Action<Result> _callback, object _parameter) where T : IRecordContainer, new ()
		{
			loadByResourceFile(_table, _callback, _parameter);
		}

		public void Load<T>(ITableContainer<T> _table, Action<Result> _callback = null, object _parameter = null) where T : IRecordContainer, new() {
			_table.Clear();
			loadByResourceFile(_table, _callback, _parameter);
		}

		public Task<Result> LoadAsync<T>(ITableContainer<T> _table, object _parameter) where T : IRecordContainer, new()
		{
			_table.Clear();
			return loadByResourceFileAsync(_table, _parameter);
		}

		private async Task<Result> loadByResourceFileAsync<T>(ITableContainer<T> _table, object _parameter = null) where T : IRecordContainer, new() {

			string _filePath = "BicDB/" + getFileName(_table.Name) + ".json";

			if (_parameter != null) {
				ResourceStorageParameter _param = _parameter as ResourceStorageParameter;
				_filePath = _param.Path;

			}

			string _data = await ReadAssetAsync(_filePath);
			var _result = new Result ((int)ResultCode.Success, _filePath);
			int _counter = 0;

			if (!string.IsNullOrEmpty(_data)) {
				try{
					JsonConvertor.GetInstance().BuildTableContainer(_table, ref _data, ref _counter);
				}catch(Exception e){
					_result.Code = (int)ResultCode.FailedConvertJson;
					_result.Message = "FailedConvertJson " + e.Message + "/" + e.ToString();
				}
			}

			return _result;
		}

		private void loadByResourceFile<T>(ITableContainer<T> _table, Action<Result> _callback = null, object _parameter = null) where T : IRecordContainer, new() {

			string _filePath = "BicDB/" + getFileName(_table.Name) + ".json";

			if (_parameter != null) {
				ResourceStorageParameter _param = _parameter as ResourceStorageParameter;
				_filePath = _param.Path;

			}

			string _data = ReadAsset(_filePath);
			var _result = new Result ((int)ResultCode.Success, _filePath);
			int _counter = 0;

			if (!string.IsNullOrEmpty(_data)) {
				try{
					JsonConvertor.GetInstance().BuildTableContainer(_table, ref _data, ref _counter);
				}catch(Exception e){
					_result.Code = (int)ResultCode.FailedConvertJson;
					_result.Message = "FailedConvertJson " + e.Message + "/" + e.ToString();
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
		public string ReadAsset(string _filePath){
			#if UNITY_EDITOR
			string _path = Application.dataPath + "/Resources/" + _filePath;

			if (File.Exists(_path))
			{
				return FileStorageUtil.ReadFile(_path);
			}
			else
			{
				Debug.Log("not found json file Resources/" + _filePath);
				return null;
			}
			#else
			if(_filePath.Contains(".")){
				_filePath = _filePath.Split('.')[0];
			}
			
			TextAsset tText = Resources.Load<TextAsset>(_filePath);

			
			return tText.text;
			#endif 
		}

		public async Task<string> ReadAssetAsync(string _filePath){
			#if UNITY_EDITOR
			string _path = Application.dataPath + "/Resources/" + _filePath;

			if (File.Exists(_path))
			{
				var _result = await FileStorageUtil.ReadFileAsync(_path);
				return _result;
			}
			else
			{
				Debug.Log("not found json file Resources/" + _filePath);
				return null;
			}
			#else
			if(_filePath.Contains(".")){
				_filePath = _filePath.Split('.')[0];
			}

			TextAsset tText = Resources.LoadAsync<TextAsset>(_filePath);

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