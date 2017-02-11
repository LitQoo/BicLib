using System;
using System.Collections.Generic;
using System.Linq;
using BicDB.Variable;

namespace BicDB
{

	public interface ITable<T> : ILinqSupporter<T>
	{
		string Name{ get; set;}
		string PrimaryKey{ get; set; }

		T this [int _index] { get; }

		void AddRow(T _row);
		void Save(Action<bool> _callaback = null, object _parameter = null);
		void Load(Action<bool> _callaback = null, object _parameter = null);
		void SetStorage(IStorage _storage);
		int GetSize();
		void Clear();

		#region Header&Property
		Dictionary<string, IVariable> Header { get; }
		Dictionary<string, IVariable> Property { get; }
		void SetOrChangeProperty (string _key, IVariable _variable);
		void SetOrChangeHeader (string _key, IVariable _variable);
		#endregion

	}

	static public class HeaderKey{
		static public string PrimaryKey = "primaryKey";
	}

	public interface ILinqSupporter<T>
	{

		IEnumerable<T> Where(Func<T, bool> _func);
		T FirstOrDefault(Func<T, bool> _func);
		IEnumerable<U> Select<U>(Func<T, U> _func);
	}


	public class Table<T> : ITable<T> where T : class, IModel, new(){
		private List<T> rows = new List<T>();

		#region LifeCycle
		public Table(string _name){
			Name = _name;
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

		public void AddRow(T _row){
			rows.Add(_row);
		}

		public void Clear(){
			rows.Clear();
		}

		public string PrimaryKey{ 
			get{ 
				return Header[HeaderKey.PrimaryKey].AsString;
			} 

			set{ 
				Header [HeaderKey.PrimaryKey] = new StringVariable (value);
			} 
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

		public IEnumerable<T> GetChangedRows(){
			return Where ((T _row)=> _row.IsChanged());
		}
		#endregion

		#region Storage
		private IStorage storage;
		public void SetStorage(IStorage _storage){
			storage = _storage;
		}

		public void Save(Action<bool> _callaback = null, object _parameter = null){
			storage.Save(this, _callaback, _parameter);
		}


		public void Load(Action<bool> _callaback = null, object _parameter = null){
			storage.Load(this, _callaback, _parameter);
		}
		#endregion

		#region Header
		private Dictionary<string, IVariable> header = new Dictionary<string, IVariable> ();
		public Dictionary<string, IVariable> Header {get{ return header; }}
		public void SetOrChangeHeader(string _key, IVariable _variable){
			if (header.ContainsKey (_key)) {
				header [_key].AsString = _variable.AsString;
			} else {
				header [_key] = _variable;
			}
		}
		#endregion

		#region Property
		private Dictionary<string, IVariable> property = new Dictionary<string, IVariable> ();
		public Dictionary<string, IVariable> Property {get{ return property; }}
		public void SetOrChangeProperty(string _key, IVariable _variable){
			if (property.ContainsKey (_key)) {
				property [_key].AsString = _variable.AsString;
			} else {
				property [_key] = _variable;
			}
		}
		#endregion
	}

}

