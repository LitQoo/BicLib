using System;
using BicDB;
using System.Security.Cryptography;
using System.Linq;
using System.Collections.Generic;
using BicDB.Utility;

namespace BicDB.Container
{
	public class MutableDictionaryContainer : IMutableDictionaryContainer
	{

		private IDictionary<string, IDataBase> data = new Dictionary<string, IDataBase>();

		#region IDictionaryContainer
		public Action<string, IDataBase> OnAddedRowActions { get; set; }
		public Action<string, IDataBase> OnRemovedRowActions { get; set; }
		#endregion

		#region IDataBase
		public DataType Type { get { return DataType.Dictionary; }}

		public void BuildVariable(ref string _json, ref int _counter, IStringParser _parser)
		{
			_parser.BuildMutableDictionaryContainer(this, ref _json, ref _counter);
		}

		public void BuildFormattedString(ref string _json, IStringFormatter _formatter){
			_formatter.BuildFormattedString(this, ref _json);
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
		public void Add(string _key, IDataBase _value)
		{
			data.Add(_key, _value);

			if (OnAddedRowActions != null) {
				OnAddedRowActions(_key, _value);
			}
		}

		public void Add(KeyValuePair<string, IDataBase> _item)
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

		public bool Remove(KeyValuePair<string, IDataBase> _item)
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

		public bool TryGetValue(string _key, out IDataBase _value)
		{
			return data.TryGetValue(_key, out _value);
		}

		public bool Contains(KeyValuePair<string, IDataBase> _item)
		{
			return data.Contains(_item);
		}

		public void CopyTo(KeyValuePair<string, IDataBase>[] _array, int _arrayIndex){
			data.CopyTo(_array, _arrayIndex);
		} 

		public IEnumerator<KeyValuePair<string, IDataBase>> GetEnumerator()
		{
			return data.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return data.GetEnumerator();
		}

		public IDataBase this[string _key] {
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

		public ICollection<IDataBase> Values {
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
