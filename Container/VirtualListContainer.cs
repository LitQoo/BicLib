using System;
using BicDB;
using System.Collections.Generic;
using System.Linq;
using BicDB.Container;
using BicDB.Variable;
using BicDB.Utility;

namespace BicDB.Container
{

	public class VirtualListContainer : VariableBase, IMutableListContainer
	{
		public event Action<IDataBase> OnAddedValueActions = delegate{};
		public event Action OnClearedValueActions = delegate{};
		public OnChangedElementDelegator<int, IDataBase> OnChangedElementActions{ get; set;}

		#region AsValue
		private Func<int, IDataBase> data;
		public DataType Type { get { return DataType.List; }}

		#endregion

		#region IListVariable
		public int IndexOf(IDataBase item)
		{
			throw new NotImplementedException();
		}

		public void Insert(int index, IDataBase item)
		{
			throw new NotImplementedException();
		}

		public void RemoveAt(int index)
		{
			throw new NotImplementedException();
		}

		public void Add(IDataBase item)
		{
			throw new NotImplementedException();
		}

		public void Clear()
		{
			throw new NotImplementedException();
		}

		public bool Contains(IDataBase item)
		{
			throw new NotImplementedException();
		}

		public void CopyTo(IDataBase[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		public bool Remove(IDataBase item)
		{
			throw new NotImplementedException();
		}

		public IEnumerator<IDataBase> GetEnumerator()
		{
			throw new NotImplementedException();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException();
		}

		public IDataBase this[int index] {
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

		#region Logic
		public VirtualListContainer() : base(){
		}

		public VirtualListContainer(Func<int, IDataBase> _func) : base(){
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