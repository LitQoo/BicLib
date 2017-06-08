using System;
using System.Collections.Generic;
using System.Linq;
using BicDB.Container;
using BicDB.Variable;
using BicDB.Utility;
using System.Diagnostics;

namespace BicDB.Container
{
	public class TableContainer<T> : ITableContainer<T> where T : class, IRecordContainer, new(){
		private IList<T> rows = new List<T>();

		#region IRecordContainerParent
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

		public DictionaryContainer header = new DictionaryContainer();
		public DictionaryContainer Header {get{ return header;}}

		private DictionaryContainer property = new DictionaryContainer();
		public DictionaryContainer Property{get{ return property;}}

		public IVariable GetRecordKey(IRecordContainer _record){
			return new IntVariable(IndexOf(_record as T));
		}
		#endregion


		#region ITableContainer
		public virtual Action<T> OnAddedRowActions { get; set; }
		public virtual Action<T> OnRemovedRowActions { get; set; }
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
			_item.Parent = this;
			rows.Insert(_index, _item);
			if (OnAddedRowActions != null) {
				OnAddedRowActions(_item);
			}
		}

		public void RemoveAt(int _index)
		{
			rows [_index].Parent = null;
			if (OnRemovedRowActions != null) {
				OnRemovedRowActions(rows [_index]);
			}
			rows.RemoveAt(_index);
		}

		public void Add(T _item)
		{
			_item.Parent = this;
			rows.Add(_item);
			if (OnAddedRowActions != null) {
				OnAddedRowActions(_item);
			}
		}

		public void Clear()
		{
			foreach (var _item in rows) {
				_item.Parent = null;
				if (OnRemovedRowActions != null) {
					OnRemovedRowActions(_item);
				}
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
				if (OnRemovedRowActions != null) {
					OnRemovedRowActions(_item);
				}
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
			_formatter.BuildFormattedString(this, ref _json, null);
		}

		public IVariable AsVariable{ 
			get{ 
				return null;	
			} 
		}

		public D As<D>() where D : class, IDataBase{
			return this as D;
		}
		#endregion


		#region Storage
		private ITableStorage storage;
		public void SetStorage(ITableStorage _storage){
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

