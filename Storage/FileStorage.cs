using System;
using System.Collections.Generic;
using BicDB.Variable;
using UnityEngine;

namespace BicDB.Storage
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

		#region IStorage
		public void Save<T>(ITable<T> _table, Action<bool> _callback) where T : IModel, new() {
			writeStringToFile(getJsonString(_table), getFileName(_table.Name));
			_callback(true);


		}

		public void Load<T>(ITable<T> _table, Action<bool> _callback) where T : IModel, new() {
			string _data = readStringFromFile(getFileName(_table.Name));
			setData(_data , _table);
			_callback(true);
		}
		#endregion

		#region parse
		private string getFileName(string _tableName){
			return "bdb" + _tableName;
		}

		private string getJsonString<T>(ITable<T> _table){
			string _result = "{\"version\":0,\"data\":[";
			for (int i = 0; i < _table.GetRowSize(); i++) {
				_result += "{";

				var _columnKeys = new List<string>(_table.GetRow(i).Columns.Keys);
				for (int j = 0; j < _columnKeys.Count; j++) {
					IVariable _column = _table.GetRow(i).Columns[_columnKeys[j]];
					if (_column.Type == VariableType.String) {
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

			_result += "]}";

			Debug.Log(">>>" + _result);

			return _result;
		}

		private void setData<T>(string _jsonString, ITable<T> _table) where T : IModel, new(){
			int i = 0;

			// need reset _tble

			Debug.Log("start parse : " + _jsonString);

			if (!increaseCounterUntilFoundChar(ref _jsonString, ref i, '{')) {
				throw new SystemException("fail find {");
			}

			Debug.Log(" { start : " + i.ToString());

			i++;


			while(i < _jsonString.Length){
				string _fieldName = getName(ref _jsonString, ref i);

				Debug.Log("getname : " + _fieldName);

				increaseCounterUntilFoundChar(ref _jsonString, ref i, ':');
				i++;

				if (_fieldName == "data") {
					Debug.Log("start make table");
					makeTable(ref _jsonString, ref _table, ref i);
				} else {
					string _data = getValue(ref _jsonString, ref i);
					Debug.Log("value : " + _data);
				}

				if (!increaseCounterUntilFoundCharWithSpeicalChar(ref _jsonString, ref i, ',')) {
					Debug.Log("not found , exit");
					break;
				}

				i++;

			}


			Debug.Log("result = " + getJsonString(_table));

//			while(true){
//
//				//findname
//
//				//switch(name)
//				// findvalue
//
//			}

		}

		private void makeTable<T>(ref string _jsonString, ref ITable<T> _table, ref int _counter) where T : IModel, new(){


			increaseCounterUntilFoundChar(ref _jsonString, ref _counter, '[');
			_counter++;

			while (_counter < _jsonString.Length) {
				T _model = makeModel(ref _jsonString, ref _table, ref _counter);
				Manager.GetTable<T>().AddRow(_model);

				if (!increaseCounterUntilFoundCharWithSpeicalChar(ref _jsonString, ref _counter, ',')) {
					Debug.Log("make table exit");
					break;
				}

				_counter++;
			}


			increaseCounterUntilFoundChar(ref _jsonString, ref _counter, ']');
			_counter++;
		}

		private T makeModel<T>(ref string _jsonString, ref ITable<T> _table, ref int _counter) where T : IModel, new(){
			T _model = new T();

			increaseCounterUntilFoundChar(ref _jsonString, ref _counter, '{');
			_counter++;

			while (_counter < _jsonString.Length) {
				//find fieldname
				string _columnName = getName(ref _jsonString, ref _counter);

				increaseCounterUntilFoundChar(ref _jsonString, ref _counter, ':');
				_counter++;

				//set data
				string _data = getValue(ref _jsonString, ref _counter);

				_model.Columns[_columnName].AsString = _data;

				Debug.Log("make model."+_columnName + " = " + _data);

				if (!increaseCounterUntilFoundCharWithSpeicalChar(ref _jsonString, ref  _counter, ',')) {
					break;
				}

				_counter++;
			}

			increaseCounterUntilFoundChar(ref _jsonString, ref _counter, '}');
			_counter++;

			return _model;
		}

		private bool increaseCounterUntilFoundCharWithSpeicalChar(ref string _jsonString, ref int _counter, char _findChar){
			while (_counter < _jsonString.Length) {
				if (_jsonString[_counter] == _findChar) {
					return true;
				} else {
					switch (_jsonString[_counter]) {
						case ' ':
							break;
						case '\\':
							_counter++;
							break;
						default :
							return false;
					}
				}
				_counter++;
			}

			return false;
		}

		private bool increaseCounterUntilFoundNormalChar(ref string _jsonString, ref int _counter){
			while (_counter < _jsonString.Length) {
				switch (_jsonString[_counter]) {
					case ' ':
						break;
					case '\\':
						_counter++;
						break;
					default :
						return true;
				}
				_counter++;
			}

			return false;
		}

		private bool increaseCounterUntilFoundChar(ref string _jsonString, ref int _counter, char _findChar){
			while(_counter < _jsonString.Length){
				if(_jsonString[_counter] == _findChar){
					
					return true;
				}

				_counter++;
			}

			return false;
		}

		private string getName(ref string _jsonString, ref int _startCounter){

			// find start "

			if (!increaseCounterUntilFoundChar(ref _jsonString, ref _startCounter, '"')) {
				throw new SystemException("not found start char");
			}

			_startCounter++;


			// collect sentence
			// find last "
			// return sentence

			string _result = "";
			while (_startCounter < _jsonString.Length) {
				if (_jsonString[_startCounter] == '"') {
					_startCounter++;
					return _result;
				}

				_result += _jsonString[_startCounter];

				_startCounter++;
			}

			throw new SystemException("not found last \"");

		}

		private string getValue(ref string _jsonString, ref int _startCounter){

			// find start point


			if (!increaseCounterUntilFoundNormalChar(ref _jsonString, ref _startCounter)) {
				throw new SystemException("not found start \"");
			}

			bool _isString = false;
			if (_jsonString[_startCounter] == '"') {
				_isString = true;
				_startCounter++;
			}



			// collect sentence
			// find last "
			// return sentence

			string _result = "";
			while (_startCounter < _jsonString.Length) {
				if ((_isString && _jsonString[_startCounter] == '"') || (!_isString && (_jsonString[_startCounter] == ' ' || _jsonString[_startCounter] == ',' || _jsonString[_startCounter] == '}'))) {
					if (!(_jsonString[_startCounter] == ',' || _jsonString[_startCounter] == '}')) {
						_startCounter++;
					}

					return _result;
				}

				_result += _jsonString[_startCounter];

				_startCounter++;
			}

			throw new SystemException("not found last \"");

		}
		#endregion

		#region fileIO
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
		#endregion
	}
}

