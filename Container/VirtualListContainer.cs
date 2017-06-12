using System;
using BicDB;
using System.Collections.Generic;
using System.Linq;
using BicDB.Container;
using BicDB.Variable;
using BicDB.Utility;

namespace BicDB.Container
{

	public class VirtualListContainer<T> : VariableBase, IListContainer<T> where T : IDataBase, new()
	{
		public event Action<T> OnAddedValueActions = delegate{};
		public event Action OnClearedValueActions = delegate{};
		public OnChangedElementDelegator<int, T> OnChangedElementActions{ get; set;}

		#region AsValue
		private Func<int, T> data;
		public DataType Type { get { return DataType.List; }}

		#endregion

		#region IListVariable
		public int IndexOf(T item)
		{
			throw new NotImplementedException();
		}

		public void Insert(int index, T item)
		{
			throw new NotImplementedException();
		}

		public void RemoveAt(int index)
		{
			throw new NotImplementedException();
		}

		public void Add(T item)
		{
			throw new NotImplementedException();
		}

//		public void Add(IDataBase item)
//		{
//			throw new NotImplementedException();
//		}

		public void Clear()
		{
			throw new NotImplementedException();
		}

		public bool Contains(T item)
		{
			throw new NotImplementedException();
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		public bool Remove(T item)
		{
			throw new NotImplementedException();
		}

		public IEnumerator<T> GetEnumerator()
		{
			throw new NotImplementedException();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException();
		}

		public T this[int index] {
			get {
				return data(index);
			}
			set {
				throw new NotImplementedException();
			}
		}

		public int Count {
			get {
				throw new NotImplementedException();
			}
		}

		public bool IsReadOnly {
			get {
				throw new NotImplementedException();
			}
		}
		#endregion

		#region Linq
		public IEnumerable<T> Where(Func<T, bool> _func){
			throwSetException();
			return null;
		}

		public T FirstOrDefault(Func<T, bool> _func){
			throwSetException();
			return data(0);
		}

		public IEnumerable<U> Select<U>(Func<T, U> _func){
			throwSetException();
			return null;
		}

		public IOrderedEnumerable<T> OrderBy<U>(Func<T, U> _func){
			throwSetException();
			return null;
		}

		public IOrderedEnumerable<T> OrderByDescending<U>(Func<T, U> _func){
			throwSetException();
			return null;
		}
		#endregion

		#region Logic
		public VirtualListContainer() : base(){
		}

		public VirtualListContainer(Func<int, T> _func) : base(){
			data = _func;
		}
		
		private void throwSetException(){
			throw new SystemException ("this variable not support to write");
		}
		#endregion

		#region IDatabase
		public void BuildVariable(ref string _json, ref int _counter, IStringParser _parser)
		{
			throwSetException ();
		}

		public void BuildFormattedString(ref string _json, IStringFormatter _formatter){
			_formatter.BuildFormattedString(this, ref _json);
		}

		public D As<D>() where D : class, IDataBase{
			return this as D;
		}

		public IVariable AsVariable{ 
			get{ 
				return null;	
			} 
		}
		#endregion
	}
}