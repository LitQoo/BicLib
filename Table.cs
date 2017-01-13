using System;
using System.Collections.Generic;

namespace BigjamLibrary.BicDB
{
	public interface ITable<T>
	{
		List<T> Rows {get;}
		string Name{ get; set;}

		T this [int _index] { get; }

		void AddRow(T _row);
		T FindRow(string _key, string _value);
		T FindRow(string _value);
		void Save(Action<bool> _callaback);
		void Load(Action<bool> _callaback);
		void SetStorage(IStorage _storage);
		int GetRowSize();
		IModel GetRow(int _rowIndex);
	}


	public class Table<T> : ITable<T> where T : class, IModel, new(){
		private List<T> rows = new List<T>();
		private string primaryColumnName = string.Empty;

		#region LifeCycle
		public Table(string _name, string _primaryKeyName){
			Name = _name;
			SetPrimaryColumn (_primaryKeyName);
		}
		#endregion

		#region IConvertString
		public string Name{ get; set;}

		public int GetRowSize(){
			return rows.Count;
		}

		public IModel GetRow(int _rowIndex){
			return rows[_rowIndex] as IModel;
		}
		#endregion

		#region ITable
		public List<T> Rows {get{ return rows; }}

		public T this[int _index]
		{
			get{return rows [_index] as T;}
		}

		public void SetPrimaryColumn(string _keyName){
			primaryColumnName = _keyName;
		}

		public void AddRow(T _row){
			rows.Add(_row);
		}

		public T FindRow(string _key, string _value){
			foreach (var _item in rows) {
				if (_item.Columns [_key].AsString == _value) {
					return _item as T;
				}
			}

			return null;
		}

		public T FindRow(string _value){
			foreach (var _item in rows) {
				if (_item.Columns [primaryColumnName].AsString == _value) {
					return _item as T;
				}
			}

			return null;
		}

		#endregion

		#region Storage
		private IStorage storage;
		public void SetStorage(IStorage _storage){
			storage = _storage;
		}

		public void Save(Action<bool> _callaback){
			storage.Save(this, _callaback);
		}


		public void Load(Action<bool> _callaback){
			storage.Load(this, _callaback);
		}
		#endregion

	}

}

