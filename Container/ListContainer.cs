using System;
using BicDB;
using System.Collections.Generic;
using System.Linq;
using BicDB.Storage;
using BicDB.Container;
using BicDB.Variable;
using BicDB.Utility;
using System.Collections;

namespace BicDB.Container
{
	public class ListContainer<T> : IListContainer<T> where T : class, IDataBase
	{
		private IList<IDataBase> data = new List<IDataBase>();

		#region IListContainer
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

		public IVariable AsVariable{ 
			get{ 
				return null;	
			} 
		}

		public D As<D>() where D : class, IDataBase{
			return this as D;
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

		public void Add(IDataBase _item){
			data.Add (_item);
			OnAddedValueActions (_item as T);
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
			return new ListContainerEnumerator<T> (data.ToArray());
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return data.GetEnumerator();
		}

		public T this[int _index] {
			get {
				return data[_index] as T;
			}
			set {
				data[_index] = value;
				OnChangedElementActions[_index](_index, data[_index] as T);
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

	public class ListContainerEnumerator<T> : IEnumerator<T> where T : class, IDataBase
	{
		public IDataBase[] _data;

		int position = -1;

		public ListContainerEnumerator(IDataBase[] list)
		{
			_data = list;
		}

		public bool MoveNext()
		{
			position++;
			return (position < _data.Length);
		}

		public void Reset()
		{
			position = -1;
		}

		object IEnumerator.Current
		{
			get
			{
				return Current;
			}
		}

		public T Current
		{
			get
			{
				try
				{
					return _data[position] as T;
				}
				catch (IndexOutOfRangeException)
				{
					throw new InvalidOperationException();
				}
			}
		}


		void IDisposable.Dispose() { }
	}
}

