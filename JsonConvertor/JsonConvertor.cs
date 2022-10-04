using UnityEngine;
using System.Collections;
using BicDB.Container;
using System;
using System.Linq;
using BicDB.Variable;
using System.Collections.Generic;
using UnityEngine.Assertions;
using BicDB;
using BicDB.Storage;
using System.Text;

namespace BicUtil.Json
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
			StringBuilder _stringBuilder = new StringBuilder();
			BuildFormattedString(_table, _stringBuilder, null);
			return _stringBuilder.ToString();
		}

		public void BuildFormattedString<T>(ITableContainer<T> _table, StringBuilder _stringBuilder, IMutableDictionaryContainer _option = null) where T : IRecordContainer, new(){
			_stringBuilder.Append("{");

			string _dataFieldName = OPTION_DATA_FIELD_NAME_DEFAULT;
			if (_option != null && _option.ContainsKey(OPTION_DATA_FIELD_NAME)) {
				_dataFieldName = _option[OPTION_DATA_FIELD_NAME].AsVariable.AsString;
			}

			var _propertyKeys = _table.Property.Keys.ToArray();
			//header
			for(int i = 0; i < _propertyKeys.Length; i++){
				IDataBase _property = _table.Property[_propertyKeys [i]];

				_stringBuilder.Append('\"');
				_stringBuilder.Append(_propertyKeys[i]);
				_stringBuilder.Append("\":");

				_property.BuildFormattedString(_stringBuilder, this);

				_stringBuilder.Append(',');
			}

			_stringBuilder.Append("\"");
			_stringBuilder.Append(_dataFieldName);
			_stringBuilder.Append("\":[");

			//data
			for (int i = 0; i < _table.Count; i++) {
				BuildFormattedString(_table[i], _stringBuilder);

				if (i != _table.Count - 1) {
					_stringBuilder.Append(',');
				}
			}

			_stringBuilder.Append("]}");
		}

		public void BuildFormattedString<T>(IDataStoreContainer<T> _table, StringBuilder _stringBuilder, IMutableDictionaryContainer _option = null) where T : IRecordContainer, new(){
			_stringBuilder.Append('{');

			string _dataFieldName = OPTION_DATA_FIELD_NAME_DEFAULT;
			if (_option != null && _option.ContainsKey(OPTION_DATA_FIELD_NAME)) {
				_dataFieldName = _option[OPTION_DATA_FIELD_NAME].AsVariable.AsString;
			}

			var _propertyKeys = _table.Property.Keys.ToArray();
			//header
			for(int i = 0; i < _propertyKeys.Length; i++){
				IDataBase _property = _table.Property[_propertyKeys [i]];

				_stringBuilder.AppendFormat("\"{0}\":", _propertyKeys[i]);
				_property.BuildFormattedString(_stringBuilder, this);

				_stringBuilder.Append(",");
			}

			_stringBuilder.AppendFormat("\"{0}\":{", _dataFieldName);

			//data

			var _lastItem = _table.Last();
			foreach (var _item in _table) {
				_stringBuilder.AppendFormat("\"{0}\":", _item.Key);

				BuildFormattedString(_item.Value, _stringBuilder);
				if (_item.Key != _lastItem.Key) {
					_stringBuilder.Append(',');
				}
			}
			_stringBuilder.Append("}}");
		}

		public void BuildFormattedString(IMutableListContainer _list, StringBuilder _stringBuilder){
			_stringBuilder.Append('[');
			int _size = _list.Count;
			for (int i = 0; i < _size; i++) {
				BuildFormattedString(_list[i], _stringBuilder);

				if (i != _size - 1) {
					_stringBuilder.Append(',');
				}
			}
			_stringBuilder.Append(']');
		}


		public void BuildFormattedString<T> (IListContainer<T> _list, StringBuilder _stringBuilder) where T : IDataBase, new(){
			_stringBuilder.Append('[');
			int _size = _list.Count;
			for (int i = 0; i < _size; i++) {
				BuildFormattedString(_list[i], _stringBuilder);

				if (i != _size - 1) {
					_stringBuilder.Append(',');
				}
			}
			_stringBuilder.Append(']');
		}

		public void BuildFormattedString(IMutableDictionaryContainer _dictionary, StringBuilder _stringBuilder){
			var _keys = _dictionary.Keys.ToArray();
			_stringBuilder.Append('{');

			for (int i = 0; i < _keys.Length; i++) {
				_stringBuilder.AppendFormat("\"{0}\":", _keys[i]);
				_dictionary[_keys[i]].BuildFormattedString(_stringBuilder, this);

				if (i != _keys.Length - 1) {
					_stringBuilder.Append(',');
				}
			}

			_stringBuilder.Append('}');
		}

		public void BuildFormattedString<T> (IDictionaryContainer<T> _dictionary, StringBuilder _stringBuilder) where T : IDataBase, new(){
			var _keys = _dictionary.Keys.ToArray();
			_stringBuilder.Append('{');

			for (int i = 0; i < _keys.Length; i++) {
				_stringBuilder.AppendFormat("\"{0}\":", _keys[i]);
				_dictionary[_keys[i]].BuildFormattedString(_stringBuilder, this);

				if (i != _keys.Length - 1) {
					_stringBuilder.Append(',');
				}
			}

			_stringBuilder.Append('}');
		}

		public void BuildFormattedString(IRecordContainer _model, StringBuilder _stringBuilder){
			_stringBuilder.Append('{');
			var _columnKeys = _model.Keys.ToArray();
			for (int j = 0; j < _columnKeys.Length; j++) {
				IDataBase _column = _model[_columnKeys[j]];
				_stringBuilder.AppendFormat("\"{0}\":", _columnKeys[j]);
				_column.BuildFormattedString(_stringBuilder, this);

				if (j != _columnKeys.Length - 1) {
					_stringBuilder.Append(',');
				}
			}

			_stringBuilder.Append('}');
		}

		public void BuildFormattedString(IVariable _variable, StringBuilder _stringBuilder){
			if (_variable.Type == DataType.String || _variable.Type == DataType.Enum || _variable.Type == DataType.Dictionary) {
				_stringBuilder.AppendFormat("\"{0}\"", _variable.AsString.Replace("\"","\\\""));
			} else {
				_stringBuilder.Append(_variable.AsString);
			}
		}


		public void BuildFormattedString(IDataBase _variable, StringBuilder _stringBuilder){
			_variable.BuildFormattedString(_stringBuilder, this);
		}
		#endregion

		#region StringParser

		public void BuildTableContainer<T>(ITableContainer<T> _table, ref string _json, ref int _counter, IMutableDictionaryContainer _option = null) where T : IRecordContainer, new(){
			int _logCount = _counter;
			if (!increaseCounterUntilFoundChar(ref _json, ref _counter, '{')) {
				Debug.Log("_counter = " + _counter.ToString() + "/ scount = " + _logCount.ToString() + "/ jsonLen = " + _json.Length.ToString() + " / json = " + _json);
				Debug.Log("_json = " + _json.Substring(0, 100));
				throw new SystemException("fail find { at BuildTableContainer");
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

				if (_fieldName == _dataFieldName)
                {

                    increaseCounterUntilFoundChar(ref _json, ref _counter, '[');

                    _counter++;

                    increaseCounterUntilNotFoundChars(ref _json, ref _counter, "\n\t ");

                    if (_json[_counter] == ']')
                    {
                        return;
                    }

					if(_json[_counter] == '[')
                    {
                        makeModelByCompressJson(_table, ref _json, ref _counter);
                    }
                    else
                    {
                    	makeModelByRegularJson(_table, ref _json, ref _counter);
					}

                    increaseCounterUntilFoundChar(ref _json, ref _counter, ']');
                    _counter++;
                }
                else {
					_table.Property[_fieldName] = BuildVariable(ref _json, ref _counter);
				}

				if (!increaseCounterUntilFoundCharsWithIgnoreChars(ref _json, ref _counter, ",", "\n\t ")) {
					
					break;
				}

				_counter++;

			}

		}

        private void makeModelByCompressJson<T>(ITableContainer<T> _table, ref string _json, ref int _counter) where T : IRecordContainer, new()
        {

			#if UNITY_EDITOR
			Debug.Log("[BicDB] makeModelByCompressJson " + _table.Name);
			#endif
            ListContainer<StringVariable> fieldNames = new ListContainer<StringVariable>();
            fieldNames.BuildVariable(ref _json, ref _counter, this);
            while (_counter < _json.Length)
            {
                increaseCounterUntilFoundCharsWithIgnoreChars(ref _json, ref _counter, "[]", "\n\t ,");

                if (_json[_counter] == ']')
                {
                    break;
                }

                _counter++;

                T _model = new T();

                for (int i = 0; i < fieldNames.Count; i++)
                {
					var _key = fieldNames[i].AsString;
					
					if(_model.ContainsKey(_key) == true){
						_model[_key].BuildVariable(ref _json, ref _counter, this);
					}else{
						_model[_key] = BuildVariable(ref _json, ref _counter);
					}

                    increaseCounterUntilFoundCharsWithIgnoreChars(ref _json, ref _counter, ",]", "\n\t ");
                    if (_json[_counter] == ',')
                    {
                        _counter++;
                    }
                    else if (_json[_counter] == ']')
                    {
                        _counter++;
                        break;
                    }
                }

                addRowToTable(_table, _model);
				_model.NotifyChanged();
				
            }
        }

        private static void addRowToTable<T>(ITableContainer<T> _table, T _model) where T : IRecordContainer, new()
        {
            object _findRow = null;
            try
            {
                if (!string.IsNullOrEmpty(_table.PrimaryKey))
                {
                    _findRow = (object)_table.FirstOrDefault<T>((T _row) => VariableUtil.IsEqual((_row as IRecordContainer)[_table.PrimaryKey], (_model as IRecordContainer)[_table.PrimaryKey]));
				}
            }
            catch (Exception)
            {
                
            }

            if (_findRow != null)
            {
                (_findRow as IRecordContainer).MergeCopyBy(_model);
            }
            else
            {
                _table.Add(_model);
            }
        }

        private void makeModelByRegularJson<T>(ITableContainer<T> _table, ref string _json, ref int _counter) where T : IRecordContainer, new()
        {
			#if UNITY_EDITOR
			Debug.Log("[BicDB] makeModelByRegularJson " + _table.Name);
			#endif

            while (_counter < _json.Length)
            {

                T _model = new T();
                _model.BuildVariable(ref _json, ref _counter, this);
                
				addRowToTable(_table, _model);
				
                if (!increaseCounterUntilFoundCharsWithIgnoreChars(ref _json, ref _counter, ",", "\n\t "))
                {
                    break;
                }

            }
        }

        public void BuildDataStoreContainer<T>(IDataStoreContainer<T> _table, ref string _json, ref int _counter,  IMutableDictionaryContainer _option = null) where T : IRecordContainer, new(){
			if (!increaseCounterUntilFoundChar(ref _json, ref _counter, '{')) {
				Debug.Log("_counter = " + _counter.ToString() + " / json = " + _json);
				throw new SystemException("fail find { BuildDataStoreContainer");
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

		public IDataBase BuildVariable(string _json){
			var __json = _json;
			int _counter = 0;
			return BuildVariable(ref __json, ref _counter);
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
				MutableListContainer _result = new MutableListContainer();
				_result.BuildVariable(ref _json, ref _counter, this);

				//var _result = BuildMutableListContainer (ref _json, ref _counter);
				return _result;
			} else if (_json[_counter] == '{') {
				MutableDictionaryContainer _result = new MutableDictionaryContainer();
				_result.BuildVariable(ref _json, ref _counter, this);
				return _result;
			} else if (_json[_counter] == 'n'){
				_counter += 4;
				return null;
			}else if(_json[_counter] == 't' || _json[_counter] == 'f' || _json[_counter] == 'T' || _json[_counter] == 'F'){
				BoolVariable _result = new BoolVariable();
				_result.BuildVariable(ref _json, ref _counter, this);
				return _result;
			} else {
				if (IsInt (ref _json, _counter)) {
					IntVariable _result = new IntVariable ();
					_result.BuildVariable (ref _json, ref _counter, this);
					return _result;
				} else {
					FloatVariable _result = new FloatVariable ();
					_result.BuildVariable (ref _json, ref _counter, this);
					return _result;
				}
			}
		}

		public bool IsInt(ref string _json, int _counter){
			while (_counter < _json.Length) {
				if (!"-0123456789\t\n ".Contains (_json [_counter].ToString())) {
					if (_json [_counter] == '.') {
						return false;
					} else if(_json[_counter] != '[') {
						return true;
					}
				}

				_counter++;
			}

			return true;
		}

		public IDataBase BuildListContainer(ref string _json, ref int _counter){
			int _startCounter = _counter;
			if (!increaseCounterUntilFoundChar(ref _json, ref _counter, '[')) {
				throw new SystemException("BuildListContainer fail find [");
			}

			_counter++;


			if (!increaseCounterUntilNotFoundChars(ref _json, ref _counter, " \t\n")) {
				throw new SystemException("not found value");
			}

			if (_json [_counter] == ']') {
				return new MutableListContainer ();
			}

			char _checkString = _json [_counter];
			_counter = _startCounter;


			if (_checkString == '"') {
				var _result = new ListContainer<StringVariable> ();
				BuildListContainer (_result, ref _json, ref _counter);
				return _result;
			} else if (_checkString == '[') {
				var _result = new ListContainer<MutableListContainer> ();
				BuildListContainer (_result, ref _json, ref _counter);
				return _result;

			} else if (_checkString == '{') {
				var _result = new ListContainer<MutableDictionaryContainer> ();
				BuildListContainer (_result, ref _json, ref _counter);
				return _result;
			} else if (_checkString == 'n') {
				_counter += 4;
				return null;
			} else if (_checkString == 't' ||_checkString == 'f' || _checkString == 'T' || _checkString == 'F') {
				var _result = new ListContainer<BoolVariable> ();
				BuildListContainer (_result, ref _json, ref _counter);
				return _result;
			} else {
				if (IsInt (ref _json, _counter)) {
					var _result = new ListContainer<IntVariable> ();
					BuildListContainer (_result, ref _json, ref _counter);
					return _result;
				} else {
					var _result = new ListContainer<FloatVariable> ();
					BuildListContainer (_result, ref _json, ref _counter);
					return _result;
				}
			}
		}
//
//		private DataType getDataType(ref string _json, int _counter){
//			return DataType.Int;
//		}
//
		public void BuildMutableListContainer(IMutableListContainer _list, ref string _json, ref int _counter){
			if (!increaseCounterUntilFoundChar(ref _json, ref _counter, '[')) {
				throw new SystemException("fail find [");
			}

			_list.Clear();
				
			_counter++;


			increaseCounterUntilNotFoundChars(ref _json, ref _counter, "\n\t ");

			if(_json[_counter] == ']'){
				_counter++;
				return;
			}

			while (_counter < _json.Length) {
				_list.Add (BuildVariable (ref _json, ref _counter));
					
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

		public void BuildListContainer<T> (IListContainer<T> _list, ref string _json, ref int _counter) where T : IDataBase, new()
		{
			if (!increaseCounterUntilFoundChar(ref _json, ref _counter, '[')) {
				throw new SystemException("fail find [");
			}

			_list.Clear();

			_counter++;
			
			increaseCounterUntilNotFoundChars(ref _json, ref _counter, "\n\t ");

			if(_json[_counter] == ']'){
				_counter++;
				return;
			}

			while (_counter < _json.Length) {
				var _value = new T();
				_value.BuildVariable (ref _json, ref _counter, this);
				_list.Add(_value);


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

		public void BuildDictionaryContainer<T>(IDictionaryContainer<T> _dictionary, ref string _json, ref int _counter) where T : IDataBase, new(){
			if (!increaseCounterUntilFoundChar(ref _json, ref _counter, '{')) {
				Debug.Log("_counter = " + _counter.ToString() + " / json = " + _json);
				throw new SystemException("fail find { at BuildDictionaryContainer");
			}

			_dictionary.Clear();

			_counter++;

			increaseCounterUntilNotFoundChars(ref _json, ref _counter, "\n\t ");

			if(_json[_counter] == '}'){
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
					var _value = new T ();
					_value.BuildVariable (ref _json, ref _counter, this);
					_dictionary.Add(_fieldName, _value);
				}


				if (!increaseCounterUntilFoundCharsWithIgnoreChars(ref _json, ref _counter, ",", "\n\t ")) {
					_counter++;
					break;
				}

				_counter++;

			}
		}

		public void BuildMutableDictionaryContainer(IMutableDictionaryContainer _dictionary, ref string _json, ref int _counter){
			if (!increaseCounterUntilFoundChar(ref _json, ref _counter, '{')) {
				Debug.Log("_counter = " + _counter.ToString() + " / json = " + _json);
				throw new SystemException("fail find { BuildMutableDictionaryContainer");
			}

			_dictionary.Clear();
			
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
				Debug.Log("_counter = " + _counter.ToString() + " / json = " + _json);
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
			if (!increaseCounterUntilFoundCharsWithIgnoreChars(ref _json, ref _counter, "\"", "\n\t ")) {
				BuildNumberVariable(_variable, ref _json,ref _counter);
				return;
			}

			_counter++;
			string _result = string.Empty;

			while (_counter < _json.Length) {
				if (_json[_counter] == '\\') {
					_counter++;
					if(_json[_counter] == 'n'){
						_result += '\n';
						_counter++;
					}else if(_json[_counter] == 't'){
						_result += "\t";
						_counter++;
					}
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
