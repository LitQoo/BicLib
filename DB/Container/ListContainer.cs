using System;
using BicDB;
using System.Collections.Generic;
using System.Linq;
using BicDB.Storage;
using BicDB.Container;
using BicDB.Variable;
using System.Collections;

namespace BicDB.Container
{
	public class ListContainer<T> : IListContainer<T> where T : IDataBase, new(){
		private IList<T> originData = new List<T>();
		virtual protected IList<T> data {
			get{ 
				return originData;
			}
			set{ 
				originData = value;
			}
		}

		#region IListContainer
		public void SubscribeOnAdded(Action<T> _callback){
			onAddedValueActions += _callback;
		} 

		public void UnsubscribeOnAdded(Action<T> _callback){
			onAddedValueActions -= _callback;
		}

		public void SubscribeOnRemoved(Action<T> _callback){
			onRemvoedValueActions += _callback;
		} 

		public void UnsubscribeOnRemoved(Action<T> _callback){
			onRemvoedValueActions -= _callback;
		}
		
		private event Action<T> onAddedValueActions = null;
		private event Action<T> onRemvoedValueActions = null;

		[Obsolete("use SubscribeOnAdded")]
		public event Action<T> OnAddedValueActions {
			add{
				SubscribeOnAdded(value);
			}

			remove{
				UnsubscribeOnAdded(value);
			}
		}
	
		[Obsolete("use SubscribeOnRemoved")]
		public event Action<T> OnRemvoedValueActions {
			add{
				SubscribeOnRemoved(value);
			}

			remove{
				UnsubscribeOnRemoved(value);
			}
		}


		public event Action OnClearedValueActions = null;
		public OnChangedElementDelegator<int, T> OnChangedElementActions{ get; set;}
		#endregion

		#region IDataBase
		public DataType Type { get { return DataType.List; }}

		public void BuildVariable(ref string _json, ref int _counter, IStringParser _parser){
			_parser.BuildListContainer(this, ref _json, ref _counter);
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

		#region IList
		public int IndexOf(T _item)
		{
			return data.IndexOf(_item);
		}

		public void Insert(int _index, T _item)
		{
			data.Insert(_index, _item);
			onAddedValueActions(_item);
		}

		public void RemoveAt(int _index)
		{
			if(onRemvoedValueActions != null){
				onRemvoedValueActions(data[_index]);
			}

			data.RemoveAt(_index);
		}

		public void Add(T _item)
		{
			data.Add(_item);
			if(onAddedValueActions != null){
				onAddedValueActions(_item);
			}
		}

		public void Clear()
		{
			if(OnClearedValueActions != null){
				OnClearedValueActions();
			}

			if(onRemvoedValueActions != null){
				for(int i = 0; i < data.Count; i ++){
					onRemvoedValueActions(data[i]);
				}
			}

			data.Clear();
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
			if(onRemvoedValueActions != null){
				onRemvoedValueActions(_item);
			}
			
			return data.Remove(_item);
		}

		public IEnumerator<T> GetEnumerator()
		{
			return data.GetEnumerator ();
			//return new ListContainerEnumerator<T> (data.ToArray());
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
