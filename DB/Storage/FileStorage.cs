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
		#endregion

		#region IStorage
		public string StorageType{get{ return "FileStorage"; }}

		public void Save<T>(ITableContainer<T> _table, Action<Result> _callback = null, object _parameter = null) where T : IRecordContainer, new() {
			string _json = JsonConvertor.GetInstance().ToFormattedString(_table);
			int _hashCode = _json.GetHashCode();
			var _encryptKey = encryptKey;
			var _fileName = GetFileName(_table.Name);

			if(_json == string.Empty){
				if (_callback != null) {
					_callback(new Result((int)ResultCode.FailedConvertJson, GetPath(_fileName), _hashCode, "string is empty"));
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
					FileStorage.Write(_json, _fileName, _encryptKey);
				}

				if (_callback != null) {
					_callback(new Result((int)ResultCode.Success, GetPath(_fileName), _hashCode));
				}
			}catch{
				if (_callback != null) {
					_callback(new Result((int)ResultCode.FileStream, GetPath(_fileName), _hashCode));
				}
			}

		}

		public void Pull<T>(ITableContainer<T> _table, Action<Result> _callback, object _parameter) where T : IRecordContainer, new ()
		{
			loadByFile(_table, _callback, _parameter);
		}

		public void Load<T>(ITableContainer<T> _table, Action<Result> _callback = null, object _parameter = null) where T : IRecordContainer, new() {
			_table.Clear();
			loadByFile(_table, _callback, _parameter);
		}

        public void Push<T>(ITableContainer<T> _table, Action<Result> _callback) where T : IRecordContainer, new()
        {
			Save(_table, _callback);
        }

		private void loadByFile<T>(ITableContainer<T> _table, Action<Result> _callback, object _parameter) where T : IRecordContainer, new ()
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

#if UNITY_EDITOR
            Debug.Log("[FileStorage] load " + _table.Name + "/" + _encryptKey);
#endif

            var _filename = GetFileName(_table.Name);
            int _hashCode = 0;
            int _counter = 0;

            string _data = null; 
			
			try{
				_data = getFileDataWithPathList(_table.Name, _encryptKey);
			}catch(SystemException _e){
				Debug.Log("[Exception] " + _e.Message + "/" + _e.ToString());

				if (_callback != null)
				{
					_callback(new Result((int)ResultCode.FileStream, GetPath(_filename), _hashCode));
				}

				return;
			}

            if (_data != null)
            {
                _hashCode = _data.GetHashCode();
            }

            var _result = new Result((int)ResultCode.Success, GetPath(_filename), _hashCode);

            if (!string.IsNullOrEmpty(_data))
            {
                try
                {
                    JsonConvertor.GetInstance().BuildTableContainer(_table, ref _data, ref _counter);
                }
                catch(Exception _e)
                {
					#if UNITY_EDITOR
					Debug.LogError("[BicDB] Fail load " + _table.Name + "/" + _e.Message + "/" + _e.ToString());
                    #endif
					_result.Code = (int)ResultCode.FailedConvertJson;
                    _result.Message = "FailedConvertJson some Error BuildTableContainer " + _e.Message;
                }
            }

            if (_callback != null)
            {
                _callback(_result);
            }
        }

        private string getFileDataWithPathList(string _tableName, string _encryptKey)
        {
			var _filename = GetFileName(_tableName);
			
            string _data = FileStorage.ReadByPath(GetPath(_filename), _encryptKey);

            if (_data == null || _data == string.Empty)
            {
				if(_tableName == TableService.TABLENAME){
					Debug.Log("[BicDB] bicsystem path by PlayerPrefs.GetString");
					
					if(PlayerPrefs.HasKey(TableService.TABLENAME) == true){
						string _path = PlayerPrefs.GetString(TableService.TABLENAME);
						Debug.Log("PlayerPrefs table path = " + _path);
						
						if(_path != string.Empty){
							_data = FileStorage.ReadByPath(_path, _encryptKey);
						}
					}
				}else{
					Debug.Log("[BicDB] path by bicsystem.path");
					
					var _tableInfo = TableService.GetTableInfo(_tableName, false);
					if (_tableInfo != null)
					{
						for (int i = _tableInfo.PathList.Count - 1; i >= 0; i--)
						{
							_data = FileStorage.ReadByPath(_tableInfo.PathList[i].AsString, _encryptKey);

							if (_data != null && _data != string.Empty)
							{
								break;
							}
						}
					}
				}
            }

            return _data;
        }

        static public string GetFileName(string _tableName){
			return FILE_NAME_PREFIX + _tableName;
		}
		#endregion
	

		#region static
		static public string GetPath(string _fileName){
			return Application.persistentDataPath + "/" + _fileName;
		}

		static public void Write(string _data, string _fileName, string _key){
			#if !WEB_BUILD

			string _path = GetPath(_fileName);
			
			using(System.IO.FileStream _file = new System.IO.FileStream (_path, System.IO.FileMode.Create, System.IO.FileAccess.Write)){
				using(System.IO.StreamWriter _streamWriter = new System.IO.StreamWriter(_file)){
					if(_key != string.Empty){
						_streamWriter.Write(AESEncrypt256(_data, _key));
					}else{
						_streamWriter.Write(_data);
					}

					_streamWriter.Flush();
					_streamWriter.Close();
					_file.Close();
				}
			}


			#else

			throw new System.Exception ("webbuild do not save to file");

			#endif
		}

		static public string ReadByTableName(string _tableName, string _key){
			return ReadByPath(GetPath(GetFileName(_tableName)), _key);
		}

        static public string ReadByPath(string _path, string _key){
			#if !WEB_BUILD
			
			if (System.IO.File.Exists(_path))
			{
				string _data = null;

				using(System.IO.FileStream _file = new System.IO.FileStream (_path, System.IO.FileMode.Open, System.IO.FileAccess.Read)){
					using(System.IO.StreamReader _stream = new System.IO.StreamReader(_file)){
						_data = _stream.ReadToEnd ();
						_stream.Close();
						_file.Close();
					}
				}

				if(_key != string.Empty){
					try{
						var _result = AESDecrypt256(_data, _key);
						return _result;
					}catch{
						return _data;
					}
				}else{
					return _data;
				}
			}
			else
			{
				Debug.LogWarning("file not exists " + _path);
				return null;
			}
			#else
			return null;
			#endif 
		}

		static private string AESDecrypt256(String Input, String key)
		{
			if (string.IsNullOrEmpty(Input)) {
				return string.Empty;
			}

			key = key.PadRight(16, '_');

			RijndaelManaged aes = new RijndaelManaged();
			aes.KeySize = 256;
			aes.BlockSize = 128;
			aes.Mode = CipherMode.CBC;
			aes.Padding = PaddingMode.PKCS7;
			aes.Key = Encoding.UTF8.GetBytes(key);
			aes.IV = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };


			var decrypt = aes.CreateDecryptor();
			byte[] xBuff = null;
			using (var ms = new MemoryStream())
			{
				using (var cs = new CryptoStream(ms, decrypt, CryptoStreamMode.Write))
				{
					byte[] xXml = Convert.FromBase64String(Input);
					cs.Write(xXml, 0, xXml.Length);
				}

				xBuff = ms.ToArray();
			}

			String Output = Encoding.UTF8.GetString(xBuff);
			return Output;
		}

		static private String AESEncrypt256(String Input, String key)
		{
			if (string.IsNullOrEmpty(Input)) {
				return string.Empty;
			}

			key = key.PadRight(16, '_');

			RijndaelManaged aes = new RijndaelManaged();
			aes.KeySize = 256;
			aes.BlockSize = 128;
			aes.Mode = CipherMode.CBC;
			aes.Padding = PaddingMode.PKCS7;
			aes.Key = Encoding.UTF8.GetBytes(key);        
			aes.IV = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

			var encrypt = aes.CreateEncryptor(aes.Key, aes.IV);
			byte[] xBuff = null;
			using (var ms = new MemoryStream())
			{
				using (var cs = new CryptoStream(ms, encrypt, CryptoStreamMode.Write))
				{
					byte[] xXml = Encoding.UTF8.GetBytes(Input);
					cs.Write(xXml, 0, xXml.Length);
				}

				xBuff = ms.ToArray();
			}

			String Output = Convert.ToBase64String(xBuff);
			return Output;
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

