using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;

namespace BicjamLib.BicTable
{

	public class FieldInfomation{
		public string Name;
		public bool IsEncrypted;

		public FieldInfomation(string _name, bool _isEncrypted){
			Name = _name;
			IsEncrypted = _isEncrypted;
		}
	}

	public interface IField
	{
		int AsInt{ get; set; }
		string AsString{ get; set; }
		float AsFloat{ get; set; }
		string AsJsonValue{ get; set;}
		void OnChangedValue();
		event Action<IField> NotifyActions;
	}
//
//	public class Variable<T> : IField{
//		#region static
//		public static implicit operator Variable<T>(T _data)
//		{
//			return new Variable<T>(_data);
//		}
//		#endregion
//
//		public T Value {get; set;}
//
//		public Variable(T _data){
//			Value = _data;
//		}
//	}
//
//	public class Function<T> : IField{
//		
//	}

	public class IntVariable : IField{
		#region static
		public static implicit operator IntVariable(int _value)
		{
			return new IntVariable(_value);
		}
		#endregion
		#region AsValue
		private int data;
		public int AsInt{ get{ return data; } set{ data = value; OnChangedValue ();} }
		public string AsString{ get{ return data.ToString (); } set{ data = Int32.Parse (value);  OnChangedValue ();} }
		public float AsFloat{ get{ return (float)data; } set{ data = (int)value;  OnChangedValue ();} }
		public string AsJsonValue{ get{ return data.ToString ();} set{ AsString = value;}}
		#endregion

		public event Action<IField> NotifyActions = delegate{};
		public IntVariable(int _value){
			data = _value;
		}

		public void OnChangedValue(){
			NotifyActions (this);
		}
	}


	public interface IModel
	{
		Dictionary<string, IField> Fields { get; }

		event Action<IModel> NotifyActions;
		void OnChangedField (IField _field);
	}

	public class Model : IModel{
		public event Action<IModel> NotifyActions = delegate{};
		private Dictionary<string, IField> fields = new Dictionary<string, IField>();
		public Dictionary<string, IField> Fields {
			get{ 
				return fields;
			}
		}

		public void AddManagedField(string key, IField _field){
			fields.Add (key, _field);
			_field.NotifyActions += OnChangedField;
		}

		public string GetJsonString(){
			string _jsonString = "{";

			foreach (var _key in fields.Keys) {
				_jsonString += "\"" + _key + "\":" + fields [_key].AsJsonValue;
			}

			_jsonString += "}";

			return _jsonString;
		}

		public void OnChangedField(IField _field){
			NotifyActions (this);
		}
	}

	public class Table{
		//private Dictionary<string, FieldInfomation> fieldInfomations = new Dictionary<string, FieldInfomation>();
		private List<IModel> rows = new List<IModel>();
		private string primaryFieldName = string.Empty;
		public string Name = string.Empty;

//		public void AddField(FieldInfomation _info){
//			fieldInfomations.Add (_info.Name, _info);	
//		} 

		public void SetPrimaryField(string _keyName){
			//fieldInfomations.Add (_info.Name, _info);
			primaryFieldName = _keyName;
		}

		public Table(string _name){
			Name = _name;
		}
			
		public IModel this[int _index]
		{
			get{return rows [_index];}
		}

		public void AddRow(IModel _row){
			rows.Add(_row);
		}

		public IModel FindRow(string _key, string _value){
			foreach (var _item in rows) {
				if (_item.Fields [_key].AsString == _value) {
					return _item;
				}
			}

			return null;
		}

		public IModel FindRow(string _value){
			foreach (var _item in rows) {
				if (_item.Fields [primaryFieldName].AsString == _value) {
					return _item;
				}
			}

			return null;
		}
	}


	public class User : Model{
		#region static
		public static Table Table = null; 
		public static void Init(){
			Table = new Table ("User");
			Table.SetPrimaryField ("no");
			Manager.AddTable (Table); 
		}
		#endregion

		#region Field
		public IntVariable No = 0;
		public IntVariable Age = 0;
		public IntVariable Nick = 0;
		#endregion

		#region IModel
		public User(){
			AddManagedField ("no", No);
			AddManagedField ("age", Age);
			AddManagedField ("nick", Nick);
		}
		#endregion
	}

	public static class Manager{
		static private Dictionary<string, Table> tables = new Dictionary<string, Table> ();
		static public Table GetTable(string _name){
			return tables[_name];
		}

		static public void AddTable(Table _table){
			tables.Add (_table.Name, _table);
		}
	}


	public class test{
		void tt(){
			Table user = User.Table;

			user.AddRow (new User ());

			(user [0] as User).No.AsInt = 1;



		}
	}


	/*
	 * 

	BicTable.AddSaveModule(FileSaver);
	BicTable.AddSaveModule(GPGSSaver);

	class UserTable<UserRow> : Table{
		
	}

	class UserRow : Row{
	  Value<int> name;
	  Value<string> id;
	  Value<float> age;
	  Value<string> email;
	}

	var row = UserTable[0];
	row.addNotifyer(setInfo);
	row.name = "name";   or row.name.value = "name";
	row.commit(); // notify
	UserTable.sync(); //sync server if setting and save

	void setNotifyer(UserInfo _info){

	}

	*/
}