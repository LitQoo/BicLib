using System;
using System.Collections.Generic;
using BicDB.Variable;

namespace BicDB.Container
{
	public interface IModelContainer :  IDictionary<string, IDataBase>, IDataBase{
		event Action<IModelContainer, string> OnChangedValueActions;
		void NotifyChanged(string _message = "");

		void AddManagedColumn(string _key, IDataBase _value);
	}

	public class ModelContainer : IModelContainer{
		private IDictionary<string, IDataBase> data = new Dictionary<string, IDataBase>();

		#region IModelContainer
		public event Action<IModelContainer, string> OnChangedValueActions;

		public void NotifyChanged(string _message = ""){
			OnChangedValueActions(this, _message);
		}

		public void AddManagedColumn(string _key, IDataBase _value){
			data.Add (_key, _value);
		}
		#endregion

		#region AsValue
		public DataType Type { get { return DataType.Model; }}

		public void BuildVariable(ref string _json, ref int _counter, IStringParser _parser){
			_parser.BuildModelContainer(this, ref _json, ref _counter);
		}

		public void BuildFormattedString(ref string _json, IStringFormatter _formatter){
			_formatter.BuildFormattedString(this, ref _json);
		}
		#endregion

		#region IDictionary
		public void Add(string _key, IDataBase _value)
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

		public bool TryGetValue(string _key, out IDataBase _value)
		{
			return data.TryGetValue(_key, out _value);
		}

		public void Add(KeyValuePair<string, IDataBase> _item)
		{
			data.Add(_item);
		}

		public void Clear()
		{
			data.Clear();
		}

		public bool Contains(KeyValuePair<string, IDataBase> _item)
		{
			return data.Contains(_item);
		}

		public void CopyTo(KeyValuePair<string, IDataBase>[] _array, int _arrayIndex){
			data.CopyTo(_array, _arrayIndex);
		}

		public bool Remove(KeyValuePair<string, IDataBase> _item)
		{
			return data.Remove(_item);
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