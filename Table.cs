using System;
using System.Collections.Generic;
using System.Linq;

namespace BicDB
{
	public interface ITable<T> : ILinqSupporter<T>
	{
		string Name{ get; set;}

		T this [int _index] { get; }

		void AddRow(T _row);
		void Save(Action<bool> _callaback);
		void Load(Action<bool> _callaback);
		void SetStorage(IStorage _storage);
		int GetSize();

	}

	public interface ILinqSupporter<T>
	{
		T FindRow(string _key, string _value);
		T FindRow(string _value);
		T FindRow(int _value);
		T FindRow(float _value);

		IEnumerable<T> Where(Func<T, bool> _func);
		T FirstOrDefault(Func<T, bool> _func);
		IEnumerable<U> Select<U>(Func<T, U> _func);
		
	}


	public class Table<T> : ITable<T> where T : class, IModel, new(){
		private List<T> rows = new List<T>();
		private string primaryColumnName = string.Empty;

		#region LifeCycle
		public Table(string _name, string _primaryKeyName = ""){
			Name = _name;
			SetPrimaryColumn (_primaryKeyName);
		}
		#endregion

		#region ITable
		public string Name{ get; set;}

		public int GetSize(){
			return rows.Count;
		}

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

		#endregion

		#region Linq
		public IEnumerable<T> Where(Func<T, bool> _func){
			return rows.Where(_func);
		}

		public T FirstOrDefault(Func<T, bool> _func){
			return rows.FirstOrDefault(_func);
		}

		public IEnumerable<U> Select<U>(Func<T, U> _func){
			return rows.Select<T,U>(_func);
		}

		public T FindRow(string _key, string _value){
			foreach (var _item in rows) {
				if (_item[_key].AsString == _value) {
					return _item as T;
				}
			}

			return null;
		}

		public T FindRow(string _value) {
			return rows.FirstOrDefault(_item => _item[primaryColumnName].AsString == _value);
		}

		public T FindRow(int _value){
			return rows.FirstOrDefault(_item => _item[primaryColumnName].AsInt == _value);
		}

		public T FindRow(float _value){
			return rows.FirstOrDefault(_item => _item[primaryColumnName].AsFloat == _value);
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

