using UnityEngine;
using System.Collections;
using BicDB.Container;
using System;
using System.Linq;
using BicDB.Variable;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEditor.VersionControl;

namespace BicDB.Utility
{
	public class JsonConvertor : IStringParser, IStringFormatter
	{
		public static string OPTION_DATA_FIELD_NAME = "dataFieldName";
		public static string OPTION_DATA_FIELD_NAME_DEFAULT = "data";

		#region Singleton
		static private JsonConvertor instance = null;
		static public JsonConvertor GetInstance(){
			if (instance == null) {
				instance = new JsonConvertor();
			}

			return instance;
		}
		#endregion

		#region StringFormatter
		public string ToFormattedString<T>(ITableContainer<T> _table) where T : IRecordContainer, new(){
			string _result = string.Empty;
			BuildFormattedString(_table, ref _result, null);
			return _result;
		}

		public void BuildFormattedString<T>(ITableContainer<T> _table, ref string _json, IDictionaryContainer _option = null) where T : IRecordContainer, new(){
			_json += "{";

			string _dataFieldName = OPTION_DATA_FIELD_NAME_DEFAULT;
			if (_option != null && _option.ContainsKey(OPTION_DATA_FIELD_NAME)) {
				_dataFieldName = _option[OPTION_DATA_FIELD_NAME].AsVariable.AsString;
			}

			var _propertyKeys = _table.Property.Keys.ToArray();
			//header
			for(int i = 0; i < _propertyKeys.Length; i++){
				IDataBase _property = _table.Property[_propertyKeys [i]];

				_json += "\"" + _propertyKeys[i] + "\":";
				_property.BuildFormattedString(ref _json, this);

				_json += ",";
			}

			_json += "\"" + _dataFieldName + "\":[";

			//data
			for (int i = 0; i < _table.Count; i++) {
				BuildFormattedString(_table[i], ref _json);

				if (i != _table.Count - 1) {
					_json += ",";
				}
			}

			_json += "]}";
		}

		public void BuildFormattedString<T>(IDataStoreContainer<T> _table, ref string _json, IDictionaryContainer _option = null) where T : IRecordContainer, new(){
			_json += "{";

			string _dataFieldName = OPTION_DATA_FIELD_NAME_DEFAULT;
			if (_option != null && _option.ContainsKey(OPTION_DATA_FIELD_NAME)) {
				_dataFieldName = _option[OPTION_DATA_FIELD_NAME].AsVariable.AsString;
			}

			var _propertyKeys = _table.Property.Keys.ToArray();
			//header
			for(int i = 0; i < _propertyKeys.Length; i++){
				IDataBase _property = _table.Property[_propertyKeys [i]];

				_json += "\"" + _propertyKeys[i] + "\":";
				_property.BuildFormattedString(ref _json, this);

				_json += ",";
			}

			_json += "\"" + _dataFieldName + "\":{";

			//data

			var _lastItem = _table.Last();
			foreach (var _item in _table) {
				_json += "\"" + _item.Key + "\":";
				BuildFormattedString(_item.Value, ref _json);
				if (_item.Key != _lastItem.Key) {
					_json += ",";
				}
			}

			_json += "}}";
		}

		public void BuildFormattedString(IListContainer _list, ref string _json){
			_json += "[";
			int _size = _list.Count;
			for (int i = 0; i < _size; i++) {
				BuildFormattedString(_list[i], ref _json);

				if (i != _size - 1) {
					_json += ",";
				}
			}
			_json += "]";
		}
			
		public void BuildFormattedString(IDictionaryContainer _dictionary, ref string _json){
			var _keys = _dictionary.Keys.ToArray();
			_json += "{";

			for (int i = 0; i < _keys.Length; i++) {
				_json += "\"" + _keys[i] + "\":";
				_dictionary[_keys[i]].BuildFormattedString(ref _json, this);

				if (i != _keys.Length - 1) {
					_json += ",";
				}
			}

			_json += "}";
		}
	
		public void BuildFormattedString(IRecordContainer _model, ref string _json){
			_json += "{";
			var _columnKeys = _model.Keys.ToArray();
			for (int j = 0; j < _columnKeys.Length; j++) {
				IDataBase _column = _model[_columnKeys[j]];

				_json += "\"" + _columnKeys[j] + "\":";
				_column.BuildFormattedString(ref _json, this);

				if (j != _columnKeys.Length - 1) {
					_json += ",";
				}
			}

			_json += "}";

		}

		public void BuildFormattedString(IVariable _variable, ref string _json){
			if (_variable.Type == DataType.String) {
				_json += "\"" + _variable.AsString.Replace("\"","\\\"") + "\"";
			} else {
				_json += _variable.AsString;
			}
		}


		public void BuildFormattedString(IDataBase _variable, ref string _json){
			_variable.BuildFormattedString(ref _json, this);
		}
		#endregion

		#region StringParser

		public void BuildTableContainer<T>(ITableContainer<T> _table, ref string _json, ref int _counter, IDictionaryContainer _option = null) where T : IRecordContainer, new(){
			if (!increaseCounterUntilFoundChar(ref _json, ref _counter, '{')) {
				throw new SystemException("fail find {");
			}


			string _dataFieldName = OPTION_DATA_FIELD_NAME_DEFAULT;
			if (_option != null && _option.ContainsKey(OPTION_DATA_FIELD_NAME)) {
				_dataFieldName = _option[OPTION_DATA_FIELD_NAME].AsVariable.AsString;
			}

			_counter++;


			while(_counter < _json.Length){
				string _fieldName = getNextDictionaryKeyName(ref _json, ref _counter);
				increaseCounterUntilFoundChar(ref _json, ref _counter, ':');
				_counter++;



				if (_fieldName == _dataFieldName) {
					
					increaseCounterUntilFoundChar(ref _json, ref _counter, '[');

					_counter++;

					while (_counter < _json.Length) {


						T _model = new T();
						_model.BuildVariable(ref _json, ref _counter, this);
						object _findRow = null;

						try {
							if(!string.IsNullOrEmpty(_table.PrimaryKey)){
								_findRow = (object)_table.FirstOrDefault<T>((T _row) => VariableUtil.IsEqual((_row as IRecordContainer)[_table.PrimaryKey], (_model as IRecordContainer)[_table.PrimaryKey]));
							}
						} catch (Exception) {
							
						}

						if (_findRow != null) {
							(_findRow as IRecordContainer).CopyBy(_model);
						} else {
							_table.Add(_model);
						}

						if (!increaseCounterUntilFoundCharsWithIgnoreChars(ref _json, ref _counter, ",", "\n\t ")) {
							break;
						}

					}

					increaseCounterUntilFoundChar(ref _json, ref _counter, ']');
					_counter++;
				} else {
					_table.Property[_fieldName] = BuildVariable(ref _json, ref _counter);
				}

				if (!increaseCounterUntilFoundCharsWithIgnoreChars(ref _json, ref _counter, ",", "\n\t ")) {
					
					break;
				}

				_counter++;

			}

		}

		public void BuildDataStoreContainer<T>(IDataStoreContainer<T> _table, ref string _json, ref int _counter,  IDictionaryContainer _option = null) where T : IRecordContainer, new(){
			if (!increaseCounterUntilFoundChar(ref _json, ref _counter, '{')) {
				throw new SystemException("fail find {");
			}


			string _dataFieldName = OPTION_DATA_FIELD_NAME_DEFAULT;
			if (_option != null && _option.ContainsKey(OPTION_DATA_FIELD_NAME)) {
				_dataFieldName = _option[OPTION_DATA_FIELD_NAME].AsVariable.AsString;
			}

			_counter++;


			while(_counter < _json.Length){
				string _fieldName = getNextDictionaryKeyName(ref _json, ref _counter);

				increaseCounterUntilFoundChar(ref _json, ref _counter, ':');
				_counter++;

				if (_fieldName == _dataFieldName) {
					increaseCounterUntilFoundChar(ref _json, ref _counter, '{');
					_counter++;

					while (_counter < _json.Length) {

						string _keyName = getNextDictionaryKeyName(ref _json, ref _counter);

						increaseCounterUntilFoundChar(ref _json, ref _counter, ':');
						_counter++;

						if (_table.ContainsKey(_keyName)) {
							_table[_keyName].BuildVariable(ref _json, ref _counter, this);
						} else {
							T _model = new T();
							_model.BuildVariable(ref _json, ref _counter, this);
							_table.Add(_keyName, _model);
						}

						if (!increaseCounterUntilFoundCharsWithIgnoreChars(ref _json, ref _counter, ",", "\n\t ")) {
							break;
						}

						_counter++;
					}

					increaseCounterUntilFoundChar(ref _json, ref _counter, '}');
					_counter++;
				} else {
					_table.Property[_fieldName] = BuildVariable(ref _json, ref _counter);
				}

				if (!increaseCounterUntilFoundCharsWithIgnoreChars(ref _json, ref _counter, ",", "\n\t ")) {
					break;
				}

				_counter++;

			}

			if (!increaseCounterUntilFoundChar(ref _json, ref _counter, '}')) {
				throw new SystemException("fail find }");
			}


		}

		public IDataBase BuildVariable(ref string _json, ref int _counter){
			// find start point
			if (!increaseCounterUntilNotFoundChars(ref _json, ref _counter, " \t\n")) {
				throw new SystemException("not found value");
			}

			if (_json[_counter] == '"') {
				StringVariable _result = new StringVariable();
				_result.BuildVariable(ref _json, ref _counter, this);
				return _result;
			} else if (_json[_counter] == '[') {
				ListContainer _result = new ListContainer();
				_result.BuildVariable(ref _json, ref _counter, this);
				return _result;
			} else if (_json[_counter] == '{') {
				DictionaryContainer _result = new DictionaryContainer();
				_result.BuildVariable(ref _json, ref _counter, this);
				return _result;
			} else if (_json[_counter] == 'n'){
				_counter += 4;
				return null;
			}else if(_json[_counter] == 't' || _json[_counter] == 'f' || _json[_counter] == 'T' || _json[_counter] == 'F'){
				BoolVariable _result = new BoolVariable();
				_result.BuildVariable(ref _json, ref _counter, this);
				return _result;
			}else {
				FloatVariable _result = new FloatVariable();
				_result.BuildVariable(ref _json, ref _counter, this);
				return _result;
			}
		}

		public void BuildListContainer(IListContainer _list, ref string _json, ref int _counter){
			Console.WriteLine("BuildListContainer1 at " + _counter.ToString());
			if (!increaseCounterUntilFoundChar(ref _json, ref _counter, '[')) {
				throw new SystemException("fail find [");
			}

			Console.WriteLine("BuildListContainer2 at " + _counter.ToString() + "," + _json[_counter].ToString());

			_counter++;

//			if (_json [_counter] == ']') {
//				_counter++;
//				return;
//			}

			while (_counter < _json.Length) {
				IDataBase _variable = BuildVariable(ref _json, ref _counter);
//				T _variable = new T();
//				_variable.BuildVariable(ref _json, ref _counter, this);
				_list.Add(_variable);

				string _string = "";
				BuildFormattedString(_variable, ref _string);
				Console.WriteLine("add variable " + _string);

				if (!increaseCounterUntilFoundCharsWithIgnoreChars(ref _json, ref _counter, ",]", "\n\t ")) {
					_counter++;
					break;
				} else {
					if (_json[_counter] == ',') {
					} else if (_json[_counter] == ']') {
						_counter++;
						return;
					} else {
						throw new SystemException("JsonToList error");
					}
				}

				_counter++;
			}
		}

		public void BuildDictionaryContainer(IDictionaryContainer _dictionary, ref string _json, ref int _counter){
			if (!increaseCounterUntilFoundChar(ref _json, ref _counter, '{')) {
				throw new SystemException("fail find { at ");
			}

			_counter++;

			if (_json[_counter] == '}') {
				_counter++;
				return;
			}

			while(_counter < _json.Length){
				string _fieldName = getNextDictionaryKeyName(ref _json, ref _counter);

				increaseCounterUntilFoundChar(ref _json, ref _counter, ':');
				_counter++;


				if (_dictionary.ContainsKey(_fieldName)) {
					_dictionary[_fieldName].BuildVariable(ref _json, ref _counter, this);
				} else {
					//T _variable = new T();
					//_variable.BuildVariable(ref _json, ref _counter, this);
					_dictionary.Add(_fieldName, BuildVariable(ref _json, ref _counter));
				}


				if (!increaseCounterUntilFoundCharsWithIgnoreChars(ref _json, ref _counter, ",", "\n\t ")) {
					_counter++;
					break;
				}

				_counter++;

			}

		}

		public void BuildModelContainer(IRecordContainer _model, ref string _json, ref int _counter){
			if (!increaseCounterUntilFoundChar(ref _json, ref _counter, '{')) {
				throw new SystemException("fail find { at BuildModelContainer");
			}

			_counter++;

			if (_json[_counter] == '}') {
				_counter++;
				return;
			}

			while(_counter < _json.Length){
				string _fieldName = getNextDictionaryKeyName(ref _json, ref _counter);

				increaseCounterUntilFoundChar(ref _json, ref _counter, ':');
				_counter++;

				if (_model.ContainsKey(_fieldName)) {
					_model[_fieldName].BuildVariable(ref _json, ref _counter, this);
				} else {
					_model.AddManagedColumn(_fieldName, BuildVariable(ref _json, ref _counter));
				}

				if (!increaseCounterUntilFoundCharsWithIgnoreChars(ref _json, ref _counter, ",", "\n\t ")) {
					break;
				}

				_counter++;

			}

			if (!increaseCounterUntilFoundChar(ref _json, ref _counter, '}')) {
				throw new SystemException("fail find } at BuildModelContainer");
			}
			_counter++;
		}

		public void BuildStringVariable(IVariable _variable, ref string _json, ref int _counter){
			if (!increaseCounterUntilFoundChar(ref _json, ref _counter, '"')) {
				throw new SystemException("fail find first \"");
			}

			_counter++;
			string _result = string.Empty;

			while (_counter < _json.Length) {
				if (_json[_counter] == '\\') {
					_counter++;
				} else if (_json[_counter] == '"') {
					_variable.AsString = _result;
					_counter++;
					return;
				}

				_result += _json[_counter];
				_counter++;
			}

			if (!increaseCounterUntilFoundChar(ref _json, ref _counter, '"')) {
				throw new SystemException("fail find last \"");
			}
		}

		public void BuildNumberVariable(IVariable _variable, ref string _json, ref int _counter){
			increaseCounterUntilNotFoundChars(ref _json, ref _counter, " \n\t");

			string _result = string.Empty;

			while (_counter < _json.Length) {
				if (",]}\t\n ".Contains(_json[_counter].ToString())) {
					_variable.AsString = _result;
					return;
				}

				_result += _json[_counter];
				_counter++;
			}

			_variable.AsString = _result;
		}

		private string getNextDictionaryKeyName(ref string _json, ref int _counter){

			// find start "

			if (!increaseCounterUntilFoundChar(ref _json, ref _counter, '"')) {
				throw new SystemException("not found start char");
			}

			_counter++;

			string _result = "";
			while (_counter < _json.Length) {
				if (_json[_counter] == '"') {
					_counter++;
					return _result;
				}

				_result += _json[_counter];

				_counter++;
			}

			throw new SystemException("not found last \"");

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

		private bool increaseCounterUntilNotFoundChars(ref string _json, ref int _counter, string _findChars){
			while (_counter < _json.Length) {
				if (_json[_counter] == '\\') {
					_counter++;
				}else if (!_findChars.Contains(_json[_counter].ToString())) {
					return true;
				}

				_counter++;
			}

			return false;
		}

		private bool increaseCounterUntilFoundCharsWithIgnoreChars(ref string _json, ref int _counter, string _findChars, string _ignoreChars){
			while (_counter < _json.Length) {
				if (_json[_counter] == '\\') {
					_counter++;
				} else if (_ignoreChars.Contains(_json[_counter].ToString())) {

				} else if (_findChars.Contains(_json[_counter].ToString())) {
					return true;
				} else {
					break;
				}

				_counter++;
			}

			return false;
		}
		#endregion
	}
}
