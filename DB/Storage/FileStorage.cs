using System;
using System.Collections.Generic;
using UnityEngine;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using BicDB.Container;
using BicDB.Variable;
using BicUtil.Json;
using BicDB.Core;
using System.Threading.Tasks;

namespace BicDB.Storage
{
	public class FileStorage : ITableStorage{
		#region Constant
		static public string FILE_NAME_PREFIX = "bdb_"; 
		#endregion

		public enum ResultCode
		{
			Success = 0,
			FailedConvertJson = 1,
			FileStream = 2
		}

		#region Singleton
		static private FileStorage instance = null;
		static public FileStorage GetInstance(){
			if (instance == null) {
				instance = new FileStorage();
			}

			return instance;
		}
		#endregion

		#region EncryptKey
		private Dictionary<string, string> encryptKeys = new Dictionary<string, string>();

		private string encryptKey = "";
		public void SetEncryptKey(string _key){
			#if UNITY_EDITOR
			if(isEncryptOnEditor == false){
				return;
			}
			#endif


			if (_key != string.Empty) {
				encryptKey = _key.PadRight(16, '_');
			}
		}

		public void SetEncryptKey(string _tableName, string _key){
			#if UNITY_EDITOR
			if(isEncryptOnEditor == false){
				return;
			}
			#endif

			if (_key == string.Empty) {
				encryptKeys[_tableName] = _key;
			}else{
				encryptKeys[_tableName] = _key.PadRight(16, '_');
			}
		}

		public string GetEncryptKey(string _tableName){
			#if UNITY_EDITOR
			if(isEncryptOnEditor == false){
				return string.Empty;
			}
			#endif

			if(encryptKeys.ContainsKey(_tableName)){
				return encryptKeys[_tableName];
			}
			
			return string.Empty;
			
		}

		private bool isEncryptOnEditor = false;
		public void SetEncryptOnEditor(bool _isEncryptOnEditor)
		{
			isEncryptOnEditor = _isEncryptOnEditor;
		}

        private string getEncryptKey<T>(ITableContainer<T> _table, object _parameter) where T : IRecordContainer, new()
        {
            var _encryptKey = encryptKey;
            if (encryptKeys.ContainsKey(_table.Name))
            {
                _encryptKey = encryptKeys[_table.Name];
            }
            else if (_parameter != null)
            {
                var _filestorageParameter = _parameter as FileStorageParameter;
                if (_filestorageParameter.EncryptKey != "")
                {
                    SetEncryptKey(_table.Name, _filestorageParameter.EncryptKey);
                    _encryptKey = GetEncryptKey(_table.Name);
                }
            }

            return _encryptKey;
        }
		#endregion

		#region IStorage
		public string StorageType{get{ return "FileStorage"; }}

		public void Save<T>(ITableContainer<T> _table, Action<Result> _callback = null, object _parameter = null) where T : IRecordContainer, new() {
			string _json = JsonConvertor.GetInstance().ToFormattedString(_table);
			int _hashCode = _json.GetHashCode();
			var _encryptKey = encryptKey;
			var _fileName = FileStorageUtil.GetFileName(_table.Name);

			if(_json == string.Empty){
				if (_callback != null) {
					_callback(new Result((int)ResultCode.FailedConvertJson, FileStorageUtil.GetPath(_fileName), _hashCode, "string is empty"));
				}

				return;
			}

			if(encryptKeys.ContainsKey(_table.Name)){
				_encryptKey = encryptKeys[_table.Name];
			}
			
			#if UNITY_EDITOR
			Debug.Log("[FileStorage] write " + _table.Name + "/" + _encryptKey);
			#endif

			try{
				if(string.IsNullOrEmpty(_json) == false){
					FileStorageUtil.Write(_json, _fileName, _encryptKey);
				}

				if (_callback != null) {
					_callback(new Result((int)ResultCode.Success, FileStorageUtil.GetPath(_fileName), _hashCode));
				}
			}catch{
				if (_callback != null) {
					_callback(new Result((int)ResultCode.FileStream, FileStorageUtil.GetPath(_fileName), _hashCode));
				}
			}

		}

		public void Pull<T>(ITableContainer<T> _table, Action<Result> _callback, object _parameter) where T : IRecordContainer, new ()
		{
			FileStorageUtil.LoadByFile(_table, _callback, getEncryptKey(_table, _parameter));
		}

		public void Load<T>(ITableContainer<T> _table, Action<Result> _callback = null, object _parameter = null) where T : IRecordContainer, new() {
			_table.Clear();
			FileStorageUtil.LoadByFile(_table, _callback, getEncryptKey(_table, _parameter));
		}

        public void Push<T>(ITableContainer<T> _table, Action<Result> _callback) where T : IRecordContainer, new()
        {
			Save(_table, _callback);
        }

        public async Task<Result> LoadAsync<T>(ITableContainer<T> _table, object _parameter) where T : IRecordContainer, new()
        {
            _table.Clear();
			var _result = await FileStorageUtil.LoadByFileAsync(_table, getEncryptKey(_table, _parameter));
			return _result;
		}
        #endregion
    }

	public class FileStorageParameter{
		public string EncryptKey;

		public FileStorageParameter(string _encryptKey = ""){
			EncryptKey = _encryptKey;
		}
	}
}

