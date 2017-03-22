using System;
using BicDB;
using System.Collections.Generic;
using System.Linq;
using BicDB.Storage;
using BicDB.Container;
using BicDB.Variable;
using BicDB.Utility;

namespace BicDB.Container
{

	public interface IListContainer<T> : IDataBase, IList<T> where T : IDataBase, new(){
		event Action<T> OnAddedValueActions;
		event Action OnClearedValueActions;
		OnChangedElementDelegator<int, T> OnChangedElementActions { get; set;}
	}


	public class ListContainer<T> : IListContainer<T> where T : IDataBase, new()
	{
		private IList<T> data = new List<T>();

		#region Event
		public event Action<T> OnAddedValueActions = delegate{};
		public event Action OnClearedValueActions = delegate{};
		public OnChangedElementDelegator<int, T> OnChangedElementActions{ get; set;}

		#endregion

		#region IDataBase
		public DataType Type { get { return DataType.List; }}

		public void BuildVariable(ref string _json, ref int _counter, IStringParser _parser){
			_parser.BuildListContainer(this, ref _json, ref _counter);
		}

		public void BuildFormattedString(ref string _json, IStringFormatter _formatter){
			_formatter.BuildFormattedString(this, ref _json);
		}
		#endregion

		#region IList
		public int IndexOf(T _item)
		{
			return data.IndexOf(_item);
		}

		public void Insert(int _index, T _item)
		{
			data.Insert(_index, _item);
			OnAddedValueActions(_item);
		}

		public void RemoveAt(int _index)
		{
			data.RemoveAt(_index);
		}

		public void Add(T _item)
		{
			data.Add(_item);
			OnAddedValueActions(_item);
		}

		public void Clear()
		{
			data.Clear();
			OnClearedValueActions();
		}

		public bool Contains(T _item)
		{
			return data.Contains(_item);
		}

		public void CopyTo(T[] _array, int _arrayIndex)
		{
			data.CopyTo(_array, _arrayIndex);
		}

		public bool Remove(T _item)
		{
			return data.Remove(_item);
		}

		public IEnumerator<T> GetEnumerator()
		{
			return data.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return data.GetEnumerator();
		}

		public T this[int _index] {
			get {
				return data[_index];
			}
			set {
				data[_index] = value;
				OnChangedElementActions[_index](_index, data[_index]);
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

		public ListContainer() : base(){
			OnChangedElementActions = new OnChangedElementDelegator<int, T>();
		}


	}

}

