using System;
using System.Collections.Generic;

namespace BigjamLibrary.BicDB
{
	public interface ITable<T>
	{
		T this [int _index] { get; }
		void AddRow(T _row);
		T FindRow(string _key, string _value);
		T FindRow(string _value);
		void Save(Action<bool> _callaback);
		void Load(Action<bool> _callaback);
		void SetStorage(IStorage _storage);
	}


	public class Table<T> : ITable<T>, IConvertString where T : class, IRow{
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
		public IRow GetRow(int _rowIndex){
			return rows[_rowIndex] as IRow;
		}
		#endregion

		#region ITable

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
			storage.Save(_callaback, this);
		}


		public void Load(Action<bool> _callaback){
			storage.Load(_callaback, this);
		}
		#endregion

	}

}

