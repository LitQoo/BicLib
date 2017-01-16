using System;
using System.Collections.Generic;
using BicDB.Variable;
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

		#region LifeCycle
		public FileStorage(){
			SetFileController(FileController.GetInstance());
		}
		#endregion

		#region IStorage
		public void Save<T>(ITable<T> _table, Action<bool> _callback) where T : IModel, new() {
			fileController.Write(getJsonString(_table), getFileName(_table.Name));
			_callback(true);
		}

		public void Load<T>(ITable<T> _table, Action<bool> _callback) where T : IModel, new() {
			string _data = fileController.Read(getFileName(_table.Name));
			setData(_data , _table);
			_callback(true);
		}
		#endregion

		#region parse
		private string getFileName(string _tableName){
			return FILE_NAME_PREFIX + _tableName;
		}

		private string getJsonString<T>(ITable<T> _table){
			string _result = "{\"version\":0,\"data\":[";
			for (int i = 0; i < _table.GetRowSize(); i++) {
				_result += "{";
				var _columnKeys = _table.GetRow(i).GetColumnNameList();
				for (int j = 0; j < _columnKeys.Count; j++) {
					IVariable _column = _table.GetRow(i)[_columnKeys[j]];
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

			return _result;
		}

		private void setData<T>(string _jsonString, ITable<T> _table) where T : IModel, new(){
			int i = 0;

			if (!increaseCounterUntilFoundChar(ref _jsonString, ref i, '{')) {
				throw new SystemException("fail find {");
			}
				
			i++;


			while(i < _jsonString.Length){
				string _fieldName = getName(ref _jsonString, ref i);

				increaseCounterUntilFoundChar(ref _jsonString, ref i, ':');
				i++;

				if (_fieldName == "data") {
					makeTable(ref _jsonString, ref _table, ref i);
				} else {
					string _data = getValue(ref _jsonString, ref i);
				}

				if (!increaseCounterUntilFoundCharWithSpeicalChar(ref _jsonString, ref i, ',')) {
					break;
				}

				i++;

			}
		}

		private void makeTable<T>(ref string _jsonString, ref ITable<T> _table, ref int _counter) where T : IModel, new(){


			increaseCounterUntilFoundChar(ref _jsonString, ref _counter, '[');
			_counter++;

			while (_counter < _jsonString.Length) {
				T _model = makeModel(ref _jsonString, ref _table, ref _counter);
				Manager.GetTable<T>().AddRow(_model);

				if (!increaseCounterUntilFoundCharWithSpeicalChar(ref _jsonString, ref _counter, ',')) {
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

				_model[_columnName].AsString = _data;

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

		#region fileController
		private IFileController fileController;

		public void SetFileController(IFileController _fileController){
			fileController = _fileController;
		}
		#endregion
	}
}

