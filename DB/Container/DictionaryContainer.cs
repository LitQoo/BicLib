using System;
using BicDB;
using System.Linq;
using System.Collections.Generic;
using BicDB.Variable;
using BicDB.Storage;
using BicUtil.Json;

namespace BicDB.Container
{
	public class DictionaryContainer<T> : IDictionaryContainer<T> where T : IDataBase, new()
	{

		private IDictionary<string, T> data = new Dictionary<string, T>();

		#region IDictionaryContainer
		public Action<string, T> OnAddedRowActions { get; set; }
		public Action<string, T> OnRemovedRowActions { get; set; }
		#endregion

		#region IDataBase
		public DataType Type { get { return DataType.Dictionary; }}

		public void BuildVariable(ref string _json, ref int _counter, IStringParser _parser)
		{
			_parser.BuildDictionaryContainer(this, ref _json, ref _counter);
		}

		public void BuildFormattedString(System.Text.StringBuilder _stringBuilder, IStringFormatter _formatter){
			_formatter.BuildFormattedString(this, _stringBuilder);
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


		#region IDictionary
		public void Add(string _key, T _value)
		{
			data.Add(_key, _value);

			if (OnAddedRowActions != null) {
				OnAddedRowActions(_key, _value);
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

		public override string ToString(){
			System.Text.StringBuilder _stringBuilder = new System.Text.StringBuilder();
			BuildFormattedString (_stringBuilder, JsonConvertor.GetInstance ());
			return _stringBuilder.ToString();
		}

		public bool ParseJson(string _json, bool _merge){
			if(_merge == false){
				this.data.Clear();
			}

			int _count = 0;
			try{
				BuildVariable(ref _json, ref _count, JsonConvertor.GetInstance());
				return true;
			}catch{
				return false;
			}
		}
		#endregion
	}

}
