using System;
using System.Collections.Generic;
using System.Linq;
using BicDB.Variable;

namespace BicDB
{



	public interface IStorageSuppoter{
		void Save(Action<Result> _callaback = null, object _parameter = null);
		void Load(Action<Result> _callaback = null, object _parameter = null);
		void SetStorage(IStorage _storage);
	}

	public interface ITable<T> : IVariableBase, IListSuppoter<T>, IStorageSuppoter where T : IModelVariable, new()
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
		new T this [int _index] { get; }
		#endregion

		#region Header&Property 
		Dictionary<string, IVariableBase> Header { get; }
		Dictionary<string, IVariableBase> Property { get; }
		void SetOrChangeProperty (string _key, IVariableBase _variable);
		void SetOrChangeHeader (string _key, IVariableBase _variable);
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


	public class Table<T> : VariableBase, ITable<T> where T : class, IModelVariable, new(){
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

		#region IListVariable
			
		public int GetSize(){
			return rows.Count;
		}

		public T this[int _index]
		{
			get{return rows [_index] as T;}
			set{ }
		}

		public void Add(T _row){
			rows.Add(_row);
			OnAddedRow (_row);
		}

		public void Insert(int _index, T _row){
			rows.Insert (_index, _row);
			OnAddedRow (_row);
		}

		public void Clear(){
			Remove (_row => true);
		}

		public void RemoveAt (int _index){
			OnRemovingRow (rows [_index]);
			rows.RemoveAt (_index);
		}

		public void Remove (Func<T, bool> _func){
			for (int i = rows.Count - 1; i >= 0; i--) {
				if (_func (rows [i])) {
					OnRemovingRow (rows [i]);
					rows.RemoveAt (i);
				}
			}
		}

		public void Remove (T _row){
			OnRemovingRow (_row);
			rows.Remove (_row);
		}

		public bool Contains(T _value){
			foreach (var _item in rows) {
				if (_value.AsFormattedString == _item.AsFormattedString) {
					return true;
				}
			}

			return false;
		}


		public void BuildVariable(ref string _json, ref int _counter, IStringParser _parser)
		{
			throw new NotImplementedException();
		}

		public void BuildFormattedString(ref string _json, IStringFormatter _formatter)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region IVariable
		public string AsFormattedString {get;set;}
		public VariableType Type { get { return VariableType.List; }}
		#endregion

		#region ITable
		public string Name{ get; set;}



		public string PrimaryKey{ 
			get{ 
				return (Header[HeaderKey.PrimaryKey] as IVariable).AsString;
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
			return rows.Select(_func);
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
		private Dictionary<string, IVariableBase> header = new Dictionary<string, IVariableBase> ();
		public Dictionary<string, IVariableBase> Header {get{ return header; }}
		public void SetOrChangeHeader(string _key, IVariableBase _variable){
			if (header.ContainsKey (_key)) {
				header [_key].AsFormattedString = _variable.AsFormattedString;
			} else {
				header [_key] = _variable;
			}
		}
		#endregion

		#region Property
		private Dictionary<string, IVariableBase> property = new Dictionary<string, IVariableBase> ();
		public Dictionary<string, IVariableBase> Property {get{ return property; }}
		public void SetOrChangeProperty(string _key, IVariableBase _variable){
			if (property.ContainsKey (_key)) {
				property [_key].AsFormattedString = _variable.AsFormattedString;
			} else {
				property [_key] = _variable;
			}
		}
		#endregion
	}

}

