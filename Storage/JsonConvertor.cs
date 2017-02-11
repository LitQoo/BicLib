using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using BicDB.Variable;

namespace BicDB.Storage
{
	static public class JsonConvertor {
		#region parse
		static public string ConvertTableToJsonString<T>(ITable<T> _table) where T : IModel {
			string _result = "{";

			var _propertyKeys = _table.Property.Keys.ToArray();
			//header
			for(int i = 0; i < _propertyKeys.Length; i++){
				IVariable _property = _table.Property[_propertyKeys [i]];

				if (_property.Type == VariableType.String) {
					_result += "\"" + _propertyKeys[i] + "\":\"" + _property.AsString.Replace("\"","\\\"") + "\"";
				} else {
					_result += "\"" + _propertyKeys[i] + "\":" + _property.AsString;
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
					setTable (ref _jsonString, _table, ref i);
				} else {
					_table.SetOrChangeProperty (_fieldName, new StringVariable (getValue (ref _jsonString, ref i)));
				}

				if (!increaseCounterUntilFoundCharWithIgnoreChars(ref _jsonString, ref i, ',', "\n\t ")) {
					break;
				}

				i++;

			}
		}

		static public void ConvertJsonDictionaryToTableThenUpdate<T>(string _jsonString, ITable<T> _table) where T : IModel, new(){
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
					updateTable(ref _jsonString, _table, ref i);
				} else {
					_table.SetOrChangeProperty (_fieldName, new StringVariable (getValue (ref _jsonString, ref i)));
				}

				if (!increaseCounterUntilFoundCharWithIgnoreChars(ref _jsonString, ref i, ',', "\n\t ")) {
					break;
				}

				i++;

			}
		}

		static public void ConvertJsonListToTable<T>(string _jsonString, ITable<T> _table) where T : IModel, new(){
			int _count = 0;
			string _jsonList = _jsonString;
			setTable (ref _jsonList, _table, ref _count);
		}

		static public List<IVariable> ConvertJsonListToList<T>(string _jsonString) where T : IVariable, new(){
			List<IVariable> _result = new List<IVariable>();
			int _counter = 0;
			increaseCounterUntilFoundChar(ref _jsonString, ref _counter, '[');
			_counter++;

			if (_jsonString [_counter] == ']') {
				_counter++;
				return _result;
			}

			while (_counter < _jsonString.Length) {
				string _value = getValue(ref _jsonString, ref _counter);
				IVariable _variable = new T();
				_variable.LoadValue(_value);

				_result.Add(_variable);

				if (!increaseCounterUntilFoundCharWithIgnoreChars(ref _jsonString, ref _counter, ',', "\n\t ")) {
					break;
				}


				_counter++;
			}

			return _result; 
		}

		static public string ConvertListToJsonString(List<IVariable> _list){
			string _result = "[";

			for (int i = 0; i < _list.Count; i++) {
				if (_list[i].Type == VariableType.String) {
					_result += "\"" + _list[i].AsString + "\"";
				} else {
					_result += _list[i].AsString;
				}

				if (i != _list.Count - 1) {
					_result += ", ";
				}
			}


			_result += "]";

			return _result;

		}

		static private void setTable<T>(ref string _jsonString, ITable<T> _table, ref int _counter) where T : IModel, new(){


			increaseCounterUntilFoundChar(ref _jsonString, ref _counter, '[');
			_counter++;

			while (_counter < _jsonString.Length) {
				T _model = MakeModel<T>(ref _jsonString, ref _counter);
				_table.AddRow(_model);


				if (!increaseCounterUntilFoundCharWithIgnoreChars(ref _jsonString, ref _counter, ',', "\n\t ")) {
					break;
				}

				_counter++;
			}


			increaseCounterUntilFoundChar(ref _jsonString, ref _counter, ']');
			_counter++;
		}

		static private void updateTable<T>(ref string _jsonString, ITable<T> _table, ref int _counter) where T : IModel, new(){


			increaseCounterUntilFoundChar(ref _jsonString, ref _counter, '[');
			_counter++;

			while (_counter < _jsonString.Length) {
				T _model = MakeModel<T>(ref _jsonString, ref _counter);
				T _finder = _table.FirstOrDefault (_row => _row [_table.PrimaryKey].AsString == _model [_table.PrimaryKey].AsString);

				if (_finder == null) {
					_table.AddRow(_model);
				} else {
					var _columns = _model.GetColumnNameList ();
					foreach (var _item in _columns) {
						if (_finder.ContainsKey (_item)) {
							_finder [_item].LoadValue (_model [_item].AsString);
						} else {
							_finder.AddManagedColumn(_item, new StringVariable (_model [_item].AsString));
						}
					}
				}

				if (!increaseCounterUntilFoundCharWithIgnoreChars(ref _jsonString, ref _counter, ',', "\n\t ")) {
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
					_model [_columnName].LoadValue(_data);
				}else {
					_model.AddManagedColumn(_columnName, new StringVariable (_data));
				}

				if (!increaseCounterUntilFoundCharWithIgnoreChars(ref _jsonString, ref  _counter, ',', "\n\t ")) {
					break;
				}

				_counter++;
			}

			increaseCounterUntilFoundChar(ref _jsonString, ref _counter, '}');
			_counter++;

			return _model;
		}

		static private bool increaseCounterUntilFoundCharWithIgnoreChars(ref string _jsonString, ref int _counter, char _findChar, string _ignoreChars){
			while (_counter < _jsonString.Length) {
				if (_jsonString[_counter] == '\\') {
					_counter++;
				} else if (_ignoreChars.Contains(_jsonString[_counter].ToString())) {
					
				} else if (_jsonString[_counter] == _findChar) {
					return true;
				} else {
					break;
				}

				_counter++;
			}

			return false;
		}

		static private bool increaseCounterUntilNotFoundChars(ref string _jsonString, ref int _counter, string _findChars){
			while (_counter < _jsonString.Length) {
				if (_jsonString[_counter] == '\\') {
					_counter++;
				}else if (!_findChars.Contains(_jsonString[_counter].ToString())) {
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
			if (!increaseCounterUntilNotFoundChars(ref _jsonString, ref _startCounter, " \t\n")) {
				throw new SystemException("not found value");
			}

			char _startChar = ' ';
			char _endChar = ' ';
			int _openerCount = 0;

			if (_jsonString[_startCounter] == '"') {
				_startChar = '"';
				_endChar = '"';
				_openerCount++;
				_startCounter++;
			} else if (_jsonString[_startCounter] == '{') {
				_startChar = '{';
				_endChar = '}';
				_openerCount++;
				_startCounter++;
			} else if (_jsonString[_startCounter] == '[') {
				_startChar = '[';
				_endChar = ']';
				_openerCount++;
				_startCounter++;
			}


			string _result = "";
			while (_startCounter < _jsonString.Length) {
				if (_jsonString[_startCounter] == '\\') {
					_result += _jsonString[_startCounter];
					_startCounter++;
				} else if (_openerCount > 0 && _jsonString[_startCounter] == _endChar) {
					_openerCount--;
					if (_openerCount == 0) {
						_startCounter++;
						if (_startChar == '"') {
							return _result;
						} else {
							return _startChar + _result + _endChar;
						}
					}
				} else if (_openerCount > 0 && _jsonString[_startCounter] == _startChar){
					_openerCount++;
				} else if(_openerCount == 0 && (_jsonString[_startCounter] == ' ' || _jsonString[_startCounter] == ',' || _jsonString[_startCounter] == '}' || _jsonString[_startCounter] == ']')){
					if (!(_jsonString[_startCounter] == ',' || _jsonString[_startCounter] == '}' || _jsonString[_startCounter] == ']')) {
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