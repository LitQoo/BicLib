using System;
using System.Collections.Generic;
using BigjamLibrary.BicDB.Column;
using UnityEngine;

namespace BigjamLibrary.BicDB.Storage
{
	public class FileStorage : IStorage{
		#region static
		static private IStorage instance = null;
		static public IStorage GetInstance(){
			if (instance == null) {
				instance = new FileStorage();
			}

			return instance;
		}
		#endregion

		public void Save(Action<bool> _callback, IConvertString _table)
		{
			writeStringToFile(getJsonString(_table), getFileName(_table.Name));
			_callback(true);
		}

		public void Load(Action<bool> _callback, IConvertString _table)
		{
			string _data = readStringFromFile(getFileName(_table.Name));
			_callback(true);
		}

		private string getFileName(string _tableName){
			return "bdb" + _tableName;
		}

		private string getJsonString(IConvertString _table){
			string _result = "[";
			for (int i = 0; i < _table.GetRowSize(); i++) {
				_result += "{";

				var _columnKeys = new List<string>(_table.GetRow(i).Columns.Keys);
				for (int j = 0; j < _columnKeys.Count; j++) {
					IColumn _column = _table.GetRow(i).Columns[_columnKeys[j]];
					if (_column.Type == ColumnType.String) {
						_result += "\"" + _columnKeys[j] + "\":\"" + _column.AsString.Replace("\"","\\\"") + "\"";
					} else {
						_result += "\"" + _columnKeys[j] + "\":" + _column.AsString;
					}

					if (j != _columnKeys.Count - 1) {
						_result += ",";
					}
				}

				_result += "}";

				if (i != _table.GetRowSize() - 1) {
					_result += ",";
				}
			}

			_result += "]";

			return _result;
		}

		private string setData(string _jsonString, IConvertString _table){
			return "";
		}



		private void writeStringToFile(string _data, string _fileName)
		{
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

		private string readStringFromFile(string _fileName)//, int lineIndex )
		{
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
	}
}

