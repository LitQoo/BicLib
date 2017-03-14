using System;
using BicDB;
using UnityEditor;
using System.Security.Cryptography;
using System.Linq;
using System.Collections.Generic;

namespace BicDB.Container
{
	public interface IDictionaryContainer<T> : IDictionary<string, T>, IDataBase where T : IDataBase, new()
	{
//		T this [string _key] { get; }
//		string[] Keys{ get; }
//
//		int GetSize();
//		void Add(string _key, T _value);
//		void RemoveAt(string _key);
//		bool Contains(string _key);
//		void Clear();
	}

	public class DictionaryContainer<T> : IDictionaryContainer<T> where T : IDataBase, new()
	{

		private IDictionary<string, T> data = new Dictionary<string, T>();

		#region AsValue
		public int AsInt{ get{ return 0; } set{} }
		public string AsString{ get{ return string.Empty; } set{ } }
		public float AsFloat{ get{ return  0; } set{ } }
		public bool AsBool{ get{ return false; } set{ } }
		public DataType Type { get { return DataType.Dictionary; }}
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

		#region LifeCycle
		public DictionaryContainer() : base(){

		}
		#endregion

		#region Logic

		public void BuildVariable(ref string _json, ref int _counter, IStringParser _parser)
		{
			_parser.BuildDictionaryVariable(this, ref _json, ref _counter);
		}

		public void BuildFormattedString(ref string _json, IStringFormatter _formatter){
			_formatter.BuildFormattedString(this, ref _json);
		}
		#endregion
	}

}
