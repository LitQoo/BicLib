using UnityEngine;
using System.Collections;
using BicDB.Variable;
using System;
using System.Linq;
using BicDB.Container;

namespace BicDB.Utility
{
	public class JsonConvertor : IStringParser, IStringFormatter
	{
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
		public void BuildFormattedString<T>(ITableContainer<T> _table, ref string _json) where T : IModelContainer, new(){
			_json += "{";

			var _propertyKeys = _table.Property.Keys.ToArray();
			//header
			for(int i = 0; i < _propertyKeys.Length; i++){
				IDataBase _property = _table.Property[_propertyKeys [i]];

				_json += "\"" + _propertyKeys[i] + "\":";
				_property.BuildFormattedString(ref _json, this);

				_json += ",";
			}

			_json += "\"data\":[";

			//data
			for (int i = 0; i < _table.GetSize(); i++) {
				BuildFormattedString(_table[i], ref _json);

				if (i != _table.GetSize() - 1) {
					_json += ",";
				}
			}

			_json += "]}";
		}

		public void BuildFormattedString<T>(IListContainer<T> _list, ref string _json) where T : IDataBase, new(){
			_json += "[";
			int _size = _list.GetSize();
			for (int i = 0; i < _size; i++) {
				BuildFormattedString(_list[i], ref _json);

				if (i != _size - 1) {
					_json += ",";
				}
			}
			_json += "]";
		}
			
		public void BuildFormattedString<T>(IDictionaryContainer<T> _dictionary, ref string _json) where T : IDataBase, new(){
			var _keys = _dictionary.Keys;
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
	
		public void BuildFormattedString(IModelContainer _model, ref string _json){
			_json += "{";
			var _columnKeys = _model.GetColumnNameList();
			for (int j = 0; j < _columnKeys.Count; j++) {
				IDataBase _column = _model[_columnKeys[j]];

				_json += "\"" + _columnKeys[j] + "\":";
				_column.BuildFormattedString(ref _json, this);

				if (j != _columnKeys.Count - 1) {
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
		public void BuildTableVariable<T>(ITableContainer<T> _table, ref string _json, ref int _counter) where T : IModelContainer, new(){
			if (!increaseCounterUntilFoundChar(ref _json, ref _counter, '{')) {
				throw new SystemException("fail find {");
			}

			_counter++;


			while(_counter < _json.Length){
				string _fieldName = getNextDictionaryKeyName(ref _json, ref _counter);

				increaseCounterUntilFoundChar(ref _json, ref _counter, ':');
				_counter++;

				if (_fieldName == "data") {
					increaseCounterUntilFoundChar(ref _json, ref _counter, '[');
					_counter++;

					while (_counter < _json.Length) {
						Console.WriteLine("add row");
						T _model = new T();
						_model.BuildVariable(ref _json, ref _counter, this);
						_table.Add(_model);


						if (!increaseCounterUntilFoundCharsWithIgnoreChars(ref _json, ref _counter, ",", "\n\t ")) {
							Console.WriteLine("break" + _json[_counter].ToString() + " - " + _counter.ToString());
							break;
						}

						_counter++;
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
				ListContainer<StringVariable> _result = new ListContainer<StringVariable>();
				_result.BuildVariable(ref _json, ref _counter, this);
				return _result;
			
			} else if (_json[_counter] == '{') {
				DictionaryContainer<StringVariable> _result = new DictionaryContainer<StringVariable>();
				_result.BuildVariable(ref _json, ref _counter, this);
				return _result;
			} else {
				FloatVariable _result = new FloatVariable();
				_result.BuildVariable(ref _json, ref _counter, this);
				return _result;
			}
		}

		public void BuildListVariable<T>(IListContainer<T> _list, ref string _json, ref int _counter) where T : IDataBase, new(){
			if (!increaseCounterUntilFoundChar(ref _json, ref _counter, '[')) {
				throw new SystemException("fail find [");
			}

			_counter++;

			if (_json [_counter] == ']') {
				_counter++;
				return;
			}

			while (_counter < _json.Length) {
				T _variable = new T();
				_variable.BuildVariable(ref _json, ref _counter, this);
				_list.Add(_variable);

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

		public void BuildDictionaryVariable<T>(IDictionaryContainer<T> _dictionary, ref string _json, ref int _counter) where T : IDataBase, new(){
			if (!increaseCounterUntilFoundChar(ref _json, ref _counter, '{')) {
				throw new SystemException("fail find {");
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

				T _variable = new T();
				_variable.BuildVariable(ref _json, ref _counter, this);
				_dictionary.Add(_fieldName, _variable);


				if (!increaseCounterUntilFoundCharsWithIgnoreChars(ref _json, ref _counter, ",", "\n\t ")) {
					break;
				}

				_counter++;

			}
		}

		public void BuildModelVariable(IModelContainer _model, ref string _json, ref int _counter){
			if (!increaseCounterUntilFoundChar(ref _json, ref _counter, '{')) {
				throw new SystemException("fail find {");
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

				if (_model.Contains(_fieldName)) {
					_model[_fieldName].BuildVariable(ref _json, ref _counter, this);
				} else {
					_model.AddManagedColumn(_fieldName, BuildVariable(ref _json, ref _counter));

				}

				if (!increaseCounterUntilFoundCharsWithIgnoreChars(ref _json, ref _counter, ",", "\n\t ")) {
					break;
				}

				_counter++;

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
