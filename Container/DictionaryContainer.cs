using System;
using BicDB;
using System.Security.Cryptography;
using System.Linq;
using System.Collections.Generic;
using BicDB.Utility;

namespace BicDB.Container
{
	public interface IDictionaryContainer<T> : IDictionary<string, T>, IDataBase where T : IDataBase, new()
	{
		
	}

	public class DictionaryContainer<T> : IDictionaryContainer<T> where T : IDataBase, new()
	{

		private IDictionary<string, T> data = new Dictionary<string, T>();

		#region IDataBase
		public DataType Type { get { return DataType.Dictionary; }}

		public void BuildVariable(ref string _json, ref int _counter, IStringParser _parser)
		{
			_parser.BuildDictionaryContainer(this, ref _json, ref _counter);
		}

		public void BuildFormattedString(ref string _json, IStringFormatter _formatter){
			_formatter.BuildFormattedString(this, ref _json);
		}

		public string GetFormattedString(IStringFormatter _formatter = null)
		{
			if (_formatter == null) {
				_formatter = JsonConvertor.GetInstance();
			}

			string _result = string.Empty;
			_formatter.BuildFormattedString(this, ref _result);
			return _result;
		}
		#endregion

		#region IDictionary
		public void Add(string _key, T _value)
		{
			data.Add(_key, _value);
		}

		public bool ContainsKey(string _key)
		{
			return data.ContainsKey(_key);
		}

		public bool Remove(string _key)
		{
			return data.Remove(_key);
		}

		public bool TryGetValue(string _key, out T _value)
		{
			return data.TryGetValue(_key, out _value);
		}

		public void Add(KeyValuePair<string, T> _item)
		{
			data.Add(_item);
		}

		public void Clear()
		{
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
	}

}
