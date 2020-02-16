using System;
using System.Collections.Generic;
using System.Linq;
using BicDB.Container;
using BicDB.Variable;
using System.Security.Cryptography;
using BicDB.Storage;
using System.Threading.Tasks;

namespace BicDB.Container
{
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

		public MutableDictionaryContainer header = new MutableDictionaryContainer();
		public MutableDictionaryContainer Header {get{ return header;}}

		private MutableDictionaryContainer property = new MutableDictionaryContainer();
		public MutableDictionaryContainer Property{get{ return property;}}

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
			if (ContainsKey(_key)) {
				return;
			}

			_item.Parent = this;
			data.Add(_key, _item);
			if (OnAddedRowActions != null) {
				OnAddedRowActions(_key, _item);
			}
		}

		public void Add(KeyValuePair<string, T> _item)
		{
			Add(_item.Key, _item.Value);
		}


		public bool ContainsKey(string _key)
		{
			return data.ContainsKey(_key);
		}

		public bool Remove(string _key)
		{

			if (!ContainsKey(_key)) {
				return false;
			}

			if (OnRemovedRowActions != null) {
				OnRemovedRowActions(_key, data[_key]);
			}

			return data.Remove(_key);
		}


		public bool Remove(KeyValuePair<string, T> _item)
		{

			return Remove(_item.Key);
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


		public bool TryGetValue(string _key, out T _value)
		{
			return data.TryGetValue(_key, out _value);
		}

		public bool Contains(KeyValuePair<string, T> _item)
		{
			return data.Contains(_item);
		}

		public void CopyTo(KeyValuePair<string, T>[] _array, int _arrayIndex){
			data.CopyTo(_array, _arrayIndex);
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

		public void BuildFormattedString(System.Text.StringBuilder _stringBuilder, IStringFormatter _formatter)
		{
			_formatter.BuildFormattedString(this, _stringBuilder, null);
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

		public Task<Result> LoadAsync(object _parameter = null){
			throw new NotImplementedException();
		}

		public void Pull(Action<Result> _callback = null, object _parameter = null){
			storage.Pull(this, _callback, _parameter);
		}

        public void Commit(IRecordContainer _record)
        {
            throw new NotImplementedException();
        }
        #endregion
    }

}

