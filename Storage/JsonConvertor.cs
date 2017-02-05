using UnityEngine;
using System.Collections;
using System;

namespace BicDB.Storage
{
	static public class JsonConvertor {
		#region parse
		static public string ConvertTableToJsonString<T>(ITable<T> _table) where T : IModel {
			string _result = "{";

			var _headerKeys = _table.GetHeaderKeyList ();
			//header
			for(int i = 0; i < _headerKeys.Length; i++){
				IVariable _header = _table.GetHeader (_headerKeys [i]);

				if (_header.Type == VariableType.String) {
					_result += "\"" + _headerKeys[i] + "\":\"" + _header.AsString.Replace("\"","\\\"") + "\"";
				} else {
					_result += "\"" + _headerKeys[i] + "\":" + _header.AsString;
				}

				_result += ",";
			}

			_result += "\"data\":[";

			//data
			for (int i = 0; i < _table.GetSize(); i++) {
				_result += "{";
				var _columnKeys = _table[i].GetColumnNameList();
				for (int j = 0; j < _columnKeys.Count; j++) {
					IVariable _column = _table[i][_columnKeys[j]];
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

				if (i != _table.GetSize() - 1) {
					_result += ",";
				}
			}

			_result += "]}";

			return _result;
		}

		static public void ConvertJsonDictionaryToTable<T>(string _jsonString, ITable<T> _table) where T : IModel, new(){
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
					setTable(ref _jsonString, ref _table, ref i);
				} else {
					_table.SetHeader (_fieldName, getValue (ref _jsonString, ref i));
				}

				if (!increaseCounterUntilFoundCharWithSpeicalChar(ref _jsonString, ref i, ',')) {
					break;
				}

				i++;

			}
		}

		static public void ConvertJsonListToTable<T>(ref string _jsonString, ref ITable<T> _table) where T : IModel, new(){
			int _count = 0;
			setTable (ref _jsonString, ref _table, ref _count);
		}

		static private void setTable<T>(ref string _jsonString, ref ITable<T> _table, ref int _counter) where T : IModel, new(){


			increaseCounterUntilFoundChar(ref _jsonString, ref _counter, '[');
			_counter++;

			while (_counter < _jsonString.Length) {
				T _model = MakeModel<T>(ref _jsonString, ref _counter);
				Manager.GetTable<T>().AddRow(_model);

				if (!increaseCounterUntilFoundCharWithSpeicalChar(ref _jsonString, ref _counter, ',')) {
					break;
				}

				_counter++;
			}


			increaseCounterUntilFoundChar(ref _jsonString, ref _counter, ']');
			_counter++;
		}

		static public T MakeModel<T>(ref string _jsonString, ref int _counter) where T : IModel, new(){
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

				if (_model.ContainsKey (_columnName)) {
					_model [_columnName].AsString = _data;
				}

				if (!increaseCounterUntilFoundCharWithSpeicalChar(ref _jsonString, ref  _counter, ',')) {
					break;
				}

				_counter++;
			}

			increaseCounterUntilFoundChar(ref _jsonString, ref _counter, '}');
			_counter++;

			return _model;
		}

		static private bool increaseCounterUntilFoundCharWithSpeicalChar(ref string _jsonString, ref int _counter, char _findChar){
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

		static private bool increaseCounterUntilFoundNormalChar(ref string _jsonString, ref int _counter){
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

		static private bool increaseCounterUntilFoundChar(ref string _jsonString, ref int _counter, char _findChar){
			while(_counter < _jsonString.Length){
				if(_jsonString[_counter] == _findChar){

					return true;
				}

				_counter++;
			}

			return false;
		}

		static private string getName(ref string _jsonString, ref int _startCounter){

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

		static private string getValue(ref string _jsonString, ref int _startCounter){

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
				if(_jsonString[_startCounter] == '\\'){
					_startCounter++;
				}else if ((_isString && _jsonString[_startCounter] == '"') || (!_isString && (_jsonString[_startCounter] == ' ' || _jsonString[_startCounter] == ',' || _jsonString[_startCounter] == '}'))) {
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
	}
}