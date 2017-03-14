using System;
using System.Collections.Generic;
using System.Linq;
using BicDB.Variable;
using BicDB.Container;

namespace BicDB.Container
{

	public interface ITableContainer<T> : IDataBase, IList<T>, IStorageSuppoter where T : IModelContainer, new()
	{
		#region event
		event Action<T> OnAddedRowActions;
		event Action<T> OnRemovingRowActions;
		#endregion

		#region get&set
		string Name{ get; set;}
		string PrimaryKey{ get; set; }
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
		private IList<T> rows = new List<T>();

		#region event
		public event Action<T> OnAddedRowActions = delegate {};
		public event Action<T> OnRemovingRowActions = delegate {};
		#endregion

		#region LifeCycle
		public TableContainer(string _name){
			Name = _name;
		}
		#endregion

		#region IListVariable
			
		public int IndexOf(T _item)
		{
			return rows.IndexOf(_item);
		}

		public void Insert(int _index, T _item)
		{
			rows.Insert(_index, _item);
			OnAddedRowActions (_item);
		}

		public void RemoveAt(int _index)
		{
			OnRemovingRowActions (rows [_index]);
			rows.RemoveAt(_index);
		}

		public void Add(T _item)
		{
			rows.Add(_item);
			OnAddedRowActions (_item);
		}

		public void Clear()
		{
			rows.Clear();
		}

		public bool Contains(T _item)
		{
			return rows.Contains(_item);
		}

		public void CopyTo(T[] _array, int _arrayIndex)
		{
			rows.CopyTo(_array, _arrayIndex);
		}

		public bool Remove(T _item)
		{
			
			if (rows.Remove(_item)) {
				OnRemovingRowActions (_item);
				return true;
			}

			return false;
		}

		public IEnumerator<T> GetEnumerator()
		{
			return rows.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return rows.GetEnumerator();
		}

		public T this[int _index] {
			get {
				return rows[_index];
			}
			set {
				rows[_index] = value;
			}
		}

		public int Count {
			get {
				return rows.Count;
			}
		}

		public bool IsReadOnly {
			get {
				return rows.IsReadOnly;
			}
		}

		#endregion

		#region IDatabase
		public DataType Type { get { return DataType.Table; }}

		public void BuildVariable(ref string _json, ref int _counter, IStringParser _parser)
		{
			throw new NotImplementedException();
		}

		public void BuildFormattedString(ref string _json, IStringFormatter _formatter)
		{
			throw new NotImplementedException();
		}
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

