using System;
using System.Collections.Generic;
using System.Linq;
using BicDB.Variable;
using BicDB.Container;

namespace BicDB.Container
{

	public interface ITableContainer<T> : IDataBase, IListSuppoter<T>, IStorageSuppoter where T : IModelContainer, new()
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
		Dictionary<string, IDataBase> Header { get; }
		Dictionary<string, IDataBase> Property { get; }
		#endregion

	}
		
	static public class HeaderKey{
		static public string PrimaryKey = "primaryKey";
	}


	public class TableContainer<T> : ITableContainer<T> where T : class, IModelContainer, new(){
		private List<T> rows = new List<T>();

		#region event
		public event Action<T> OnAddedRow = delegate {};
		public event Action<T> OnRemovingRow = delegate {};
		#endregion

		#region LifeCycle
		public TableContainer(string _name){
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
		public DataType Type { get { return DataType.Table; }}
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
		private Dictionary<string, IDataBase> header = new Dictionary<string, IDataBase> ();
		public Dictionary<string, IDataBase> Header {get{ return header;}}
		#endregion

		#region Property
		private Dictionary<string, IDataBase> property = new Dictionary<string, IDataBase> ();
		public Dictionary<string, IDataBase> Property{get{ return property;}}
		#endregion
	}

}

