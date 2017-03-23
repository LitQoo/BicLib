using System;
using System.Collections.Generic;
using UnityEngine;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using BicDB.Utility;
using BicDB.Container;
using BicDB.Variable;

namespace BicDB.Storage
{

	public class FileStorage : ITableStorage{
		#region Constant
		static public string FILE_NAME_PREFIX = "bdb_"; 
		#endregion

		public enum ResultCode
		{
			Success = 0,
			FailedConvertJson = 1
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
		private string encryptKey = "";
		public void SetEncryptKey(string _key){
			if (_key != string.Empty) {
				encryptKey = _key.PadRight(16, '_');
			}
		}
		#endregion

		#region IStorage
		public void Save<T>(ITableContainer<T> _table, Action<Result> _callback = null, object _parameter = null) where T : IRecordContainer, new() {
			string _json = JsonConvertor.GetInstance().ToFormattedString(_table);

			FileStorage.Write(_json, getFileName(_table.Name), encryptKey);

			if (_callback != null) {
				_callback(new Result((int)ResultCode.Success));
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

		private void loadByFile<T>(ITableContainer<T> _table, Action<Result> _callback, object _parameter) where T : IRecordContainer, new ()
		{
			string _data = FileStorage.Read(getFileName(_table.Name), encryptKey);
			int _counter = 0;

			var _result = new Result ((int)ResultCode.Success);


			if (!string.IsNullOrEmpty (_data)) {
				try {
					JsonConvertor.GetInstance().BuildTableContainer(_table, ref _data, ref _counter);
				} catch (Exception) {
					_result.Code = (int)ResultCode.FailedConvertJson;
					_result.Message = ResultCode.FailedConvertJson.ToString ();
				}
			}

			if (_callback != null) {
				_callback (_result);
			}
		} 

		private string getFileName(string _tableName){
			return FILE_NAME_PREFIX + _tableName;
		}
		#endregion
	

		#region static
		static public void Write(string _data, string _fileName, string _key){
			#if !WEB_BUILD

			string _path = Application.persistentDataPath + "/" + _fileName;
			System.IO.FileStream _file = new System.IO.FileStream (_path, System.IO.FileMode.Create, System.IO.FileAccess.Write);
			System.IO.StreamWriter _streamWriter = new System.IO.StreamWriter(_file);

			if(_key != string.Empty){
				_streamWriter.WriteLine(AESEncrypt256(_data, _key));
			}else{
				_streamWriter.WriteLine(_data);
			}

			_streamWriter.Close();
			_file.Close();


			#else

			throw new System.Exception ("webbuild do not save to file");

			#endif
		}

		static public string Read(string _fileName, string _key){
			#if !WEB_BUILD
			string _path = Application.persistentDataPath + "/" + _fileName;

			if (System.IO.File.Exists(_path))
			{
				System.IO.FileStream _file = new System.IO.FileStream (_path, System.IO.FileMode.Open, System.IO.FileAccess.Read);
				System.IO.StreamReader _stream = new System.IO.StreamReader(_file);
				string _data = null;
				_data = _stream.ReadLine ();
				_stream.Close();
				_file.Close();

				if(_key != string.Empty){
					return AESDecrypt256(_data, _key);
				}else{
					return _data;
				}
			}
			else
			{
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
}

