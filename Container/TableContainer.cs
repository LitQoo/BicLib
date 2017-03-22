using System;
using System.Collections.Generic;
using System.Linq;
using BicDB.Container;
using BicDB.Variable;
using BicDB.Utility;
using System.Diagnostics;

namespace BicDB.Container
{

	public interface ITableContainer<T> : IDataBase, IList<T>, IModelContainerParent, IStorageSuppoter where T : IModelContainer
	{
		#region event
		Action<T> OnAddedRowActions { get; set; }
		Action<T> OnRemovedRowActions { get; set;}
		#endregion
	}

	public interface IModelContainerParent
	{
		#region get&set
		string Name{ get; set;}
		string PrimaryKey{ get; set; }
		#endregion

		#region Header&Property 
		IModelContainer Header { get; }
		IModelContainer Property { get; }
		#endregion

		int GetIndex(IModelContainer _row);
	}
		
	static public class HeaderKey{
		static public string PrimaryKey = "primaryKey";
	}


	public class TableContainer<T> : ITableContainer<T> where T : class, IModelContainer, new(){
		private IList<T> rows = new List<T>();

		#region IModelContainerParent
		public string Name{ get; set;}

		public string PrimaryKey{ 
			get{ 
				if (Header.ContainsKey(HeaderKey.PrimaryKey)) {
					return (Header[HeaderKey.PrimaryKey] as IVariable).AsString;
				} else {
					return string.Empty;
				}
			} 

			set{ 
				Header [HeaderKey.PrimaryKey] = new StringVariable (value);
			} 
		}

		public IModelContainer header = new ModelContainer();
		public IModelContainer Header {get{ return header;}}

		private IModelContainer property = new ModelContainer();
		public IModelContainer Property{get{ return property;}}

		public int GetIndex(IModelContainer _row){
			return rows.IndexOf(_row as T);
		}
		#endregion


		#region ITableContainer
		public virtual Action<T> OnAddedRowActions { get; set; }
		public virtual Action<T> OnRemovedRowActions { get; set; }
		#endregion

		#region LifeCycle
		public TableContainer(string _name){
			Name = _name;

			OnAddedRowActions = delegate {};
		}
		#endregion

		#region IListVariable
			
		public int IndexOf(T _item)
		{
			return rows.IndexOf(_item);
		}

		public void Insert(int _index, T _item)
		{
			_item.Parent = this;
			rows.Insert(_index, _item);
			OnAddedRowActions (_item);
		}

		public void RemoveAt(int _index)
		{
			rows [_index].Parent = null;
			OnRemovedRowActions (rows [_index]);
			rows.RemoveAt(_index);
		}

		public void Add(T _item)
		{
			_item.Parent = this;
			rows.Add(_item);
			OnAddedRowActions (_item);
		}

		public void Clear()
		{
			foreach (var _item in rows) {
				_item.Parent = null;
				OnRemovedRowActions(_item);
			}

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
				_item.Parent = null;
				OnRemovedRowActions (_item);
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
				if (rows[_index] != null) {
					rows[_index].Parent = null;
				}

				value.Parent = this;
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
			_parser.BuildTableContainer(this, ref _json, ref _counter);
		}

		public void BuildFormattedString(ref string _json, IStringFormatter _formatter)
		{
			_formatter.BuildFormattedString(this, ref _json);
		}
		#endregion


		#region Storage
		private IStorage storage;
		public void SetStorage(IStorage _storage){
			storage = _storage;
		}

		public void Save(Action<Result> _callback = null, object _parameter = null){
			storage.Save(this, _callback, _parameter);
		}

		public void Load(Action<Result> _callback = null, object _parameter = null){
			storage.Load(this, _callback, _parameter);
		}

		public void Pull(Action<Result> _callback = null, object _parameter = null){
			storage.Pull(this, _callback, _parameter);
		}
		#endregion
	}

}

