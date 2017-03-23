using System;
using System.Collections.Generic;
using System.Linq;
using BicDB.Container;
using BicDB.Variable;
using BicDB.Utility;
using System.Diagnostics;

namespace BicDB.Container
{

	public interface IDataStoreContainer<T> : IDataBase, IDictionary<string, T>, IRecordContainerParent, IDataStoreStorageSuppoter where T : IRecordContainer
	{
		#region event
		Action<string, T> OnAddedRowActions { get; set; }
		Action<string, T> OnRemovedRowActions { get; set;}
		#endregion
	}


	static public class HeaderKey{
		static public string PrimaryKey = "primaryKey";
	}


	public class DataStoreContainer<T> : IDataStoreContainer<T> where T : class, IRecordContainer, new(){
		protected IDictionary<string, T> data = new Dictionary<string, T>();

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

		public IRecordContainer header = new RecordContainer();
		public IRecordContainer Header {get{ return header;}}

		private IRecordContainer property = new RecordContainer();
		public IRecordContainer Property{get{ return property;}}

		public IVariable GetRecordKey(IRecordContainer _record){
			return new StringVariable(this.FirstOrDefault(_item => _item.Value == _record as T).Key);
		}
		#endregion


		#region IDataStoreContainer
		public virtual Action<string, T> OnAddedRowActions { get; set; }
		public virtual Action<string, T> OnRemovedRowActions { get; set; }
		#endregion

		#region LifeCycle
		public DataStoreContainer(string _name){
			Name = _name;
		}
		#endregion

		#region IDictionary
		public void Add(string _key, T _item)
		{
			_item.Parent = this;
			data.Add(_key, _item);
			if (OnAddedRowActions != null) {
				OnAddedRowActions(_key, _item);
			}
		}

		public void Add(KeyValuePair<string, T> _item)
		{
			_item.Value.Parent = this;
			data.Add(_item);

			if (OnAddedRowActions != null) {
				OnAddedRowActions(_item.Key, _item.Value);
			}
		}


		public bool ContainsKey(string _key)
		{
			return data.ContainsKey(_key);
		}

		public bool Remove(string _key)
		{
			if (OnRemovedRowActions != null) {
				OnRemovedRowActions(_key, data[_key]);
			}
			return data.Remove(_key);
		}

		public bool TryGetValue(string _key, out T _value)
		{
			return data.TryGetValue(_key, out _value);
		}
		public void Clear()
		{
			foreach (var _item in data) {
				_item.Value.Parent = null;
				if (OnRemovedRowActions != null) {
					OnRemovedRowActions(_item.Key, _item.Value);
				}
			}

			data.Clear();
		}

		public bool Contains(KeyValuePair<string, T> _item)
		{
			return data.Contains(_item);
		}

		public void CopyTo(KeyValuePair<string, T>[] _array, int _arrayIndex){
			data.CopyTo(_array, _arrayIndex);
		}

		public bool Remove(KeyValuePair<string, T> _item)
		{
			if (OnRemovedRowActions != null) {
				OnRemovedRowActions(_item.Key, _item.Value);
			}

			return data.Remove(_item);
		}

		public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
		{
			return data.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return data.GetEnumerator();
		}

		public T this[string _key] {
			get {
				return data[_key];
			}
			set {
				if (data.ContainsKey(_key)) {
					data[_key].Parent = null;
				}

				value.Parent = this;
				data[_key] = value;
			}
		}

		public ICollection<string> Keys {
			get {
				return data.Keys;
			}
		}

		public ICollection<T> Values {
			get {
				return data.Values;
			}
		}

		public int Count {
			get {
				return data.Count;
			}
		}

		public bool IsReadOnly {
			get {
				return data.IsReadOnly;
			}
		}
		#endregion

		#region IDatabase
		public DataType Type { get { return DataType.DataStore; }}

		public void BuildVariable(ref string _json, ref int _counter, IStringParser _parser)
		{
			_parser.BuildDataStoreContainer(this, ref _json, ref _counter);
		}

		public void BuildFormattedString(ref string _json, IStringFormatter _formatter)
		{
			_formatter.BuildFormattedString(this, ref _json);
		}
		#endregion


		#region Storage
		private IDataStoreStorage storage;
		public void SetStorage(IDataStoreStorage _storage){
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

