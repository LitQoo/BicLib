using System;
using System.Collections.Generic;
using UnityEngine;

namespace BicDB.Storage
{
	public class FileStorage : IStorage{
		static public string FILE_NAME_PREFIX = "bdb_"; 

		#region static
		static private IStorage instance = null;
		static public IStorage GetInstance(){
			if (instance == null) {
				instance = new FileStorage();
			}

			return instance;
		}
		#endregion

		#region IStorage
		public void Save<T>(ITable<T> _table, Action<bool> _callback = null, object _parameter = null) where T : IModel, new() {
			Write(JsonConvertor.ConvertTableToJsonString(_table), getFileName(_table.Name));
			if (_callback != null) {
				_callback(true);
			}
		}

		public void Load<T>(ITable<T> _table, Action<bool> _callback = null, object _parameter = null) where T : IModel, new() {
			string _data = Read(getFileName(_table.Name));
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
			return FILE_NAME_PREFIX + _tableName;
		}
		#endregion

		#region static
		static public void Write(string _data, string _fileName){
			#if !WEB_BUILD

			string _path = Application.persistentDataPath + "/" + _fileName;
			System.IO.FileStream _file = new System.IO.FileStream (_path, System.IO.FileMode.Create, System.IO.FileAccess.Write);
			System.IO.StreamWriter _streamWriter = new System.IO.StreamWriter(_file);
			_streamWriter.WriteLine(_data);
			_streamWriter.Close();
			_file.Close();


			#else

			throw new System.Exception ("webbuild do not save to file");

			#endif
		}

		static public string Read(string _fileName){
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
				return _data;
			}
			else
			{
				return null;
			}
			#else
			return null;
			#endif 
		}
		#endregion
	}
}

