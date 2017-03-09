using System;
using System.Collections.Generic;
using System.Linq;
using BicDB.Variable;

namespace BicDB
{

	public interface ITable<T> : ILinqSupporter<T>
	{
		#region event
		event Action<T> OnAddedRow;
		event Action<T> OnRemovingRow;
		#endregion

		#region get&set
		string Name{ get; set;}
		string PrimaryKey{ get; set; }
		#endregion

		#region indexer
		T this [int _index] { get; }
		#endregion

		#region method
		void AddRow(T _row);
		void Save(Action<Result> _callaback = null, object _parameter = null);
		void Load(Action<Result> _callaback = null, object _parameter = null);
		void SetStorage(IStorage _storage);
		int GetSize();
		void Clear();
		void RemoveRow (int _index);
		void RemoveRow (T _row);
		void RemoveRow (Func<T, bool> _func);
		#endregion

		#region Header&Property 
		Dictionary<string, IVariable> Header { get; }
		Dictionary<string, IVariable> Property { get; }
		void SetOrChangeProperty (string _key, IVariable _variable);
		void SetOrChangeHeader (string _key, IVariable _variable);
		#endregion

	}

	public class Result{
		public int Code = 0;
		public string Message = "";

		public Result(int _code, string _message = ""){
			Code = _code;
			Message = _message;
		}
	}

	static public class HeaderKey{
		static public string PrimaryKey = "primaryKey";
	}

	public interface ILinqSupporter<T>
	{

		IEnumerable<T> Where(Func<T, bool> _func);
		T FirstOrDefault(Func<T, bool> _func);
		IEnumerable<U> Select<U>(Func<T, U> _func);
		IOrderedEnumerable<T> OrderBy<U>(Func<T, U> _func);
		IOrderedEnumerable<T> OrderByDescending<U>(Func<T, U> _func);
	}


	public class Table<T> : ITable<T> where T : class, IModelVariable, new(){
		private List<T> rows = new List<T>();

		#region event
		public event Action<T> OnAddedRow = delegate {};
		public event Action<T> OnRemovingRow = delegate {};
		#endregion

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
			OnAddedRow (_row);
		}

		public void InsertRow(int _index, T _row){
			rows.Insert (_index, _row);
			OnAddedRow (_row);
		}

		public void Clear(){
			RemoveRow (_row => true);
		}

		public string PrimaryKey{ 
			get{ 
				return Header[HeaderKey.PrimaryKey].AsString;
			} 

			set{ 
				Header [HeaderKey.PrimaryKey] = new StringVariable (value);
			} 
		}

		public void RemoveRow (int _index){
			OnRemovingRow (rows [_index]);
			rows.RemoveAt (_index);
		}

		public void RemoveRow (Func<T, bool> _func){
			for (int i = rows.Count - 1; i >= 0; i--) {
				if (_func (rows [i])) {
					OnRemovingRow (rows [i]);
					rows.RemoveAt (i);
				}
			}
		}

		public void RemoveRow (T _row){
			OnRemovingRow (_row);
			rows.Remove (_row);
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
			return rows.Select(_func);
		}

		public IEnumerable<T> GetChangedRows(){
			return Where ((T _row)=> _row.IsChanged);
		}

		public IOrderedEnumerable<T> OrderBy<U>(Func<T, U> _func){
			return rows.OrderBy(_func);
		}

		public IOrderedEnumerable<T> OrderByDescending<U>(Func<T, U> _func){
			return rows.OrderByDescending(_func);
		}
		#endregion

		#region Storage
		private IStorage storage;
		public void SetStorage(IStorage _storage){
			storage = _storage;
		}

		public void Save(Action<Result> _callaback = null, object _parameter = null){
			storage.Save(this, _callaback, _parameter);
		}


		public void Load(Action<Result> _callaback = null, object _parameter = null){
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

