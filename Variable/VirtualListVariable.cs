using System;
using BicDB;
using System.Collections.Generic;
using System.Linq;
using BicDB.Container;

namespace BicDB.Variable
{

	public class VirtualListVariable<T> : DataBase, IListContainer<T> where T : IDataBase, new()
	{
		public new event Action<IListContainer<T>, string> OnChangedValueActions;
		public event Action<T> OnAddedValueActions = delegate{};
		public event Action OnClearedValueActions = delegate{};
		public OnChangedElementDelegator<int, T> OnChangedElementActions{ get; set;}

		#region AsValue
		private Func<int, T> data;
		public int AsInt{ get{ return 0; } set{throwSetException ();} }
		public string AsString{ get{ return string.Empty; } set{ throwSetException ();} }
		public float AsFloat{ get{ return 0; } set{ throwSetException ();} }
		public bool AsBool{ get{ return false; } set{throwSetException ();} }
		public DataType Type { get { return DataType.List; }}
		public string AsFormattedString { get; set; }
		#endregion


		#region IListVariable
		public T this [int _index] { 
			get{ 
				return data(_index);
			} 

			set{ 
				throwSetException ();
			}
		}

		public int GetSize(){
			return 0;
		}

		public void Add(T _value){
			throwSetException ();
		}

		public void Remove(int _index){
			throwSetException ();
		}

		public bool Contains(T _value){
			throwSetException ();

			return false;
		}

		public void Clear(){
			throwSetException ();
		}

		public void RemoveAt(int _index)
		{
			throw new NotImplementedException();
		}

		public void Remove(T _value)
		{
			throw new NotImplementedException();
		}

		public void Remove(Func<T, bool> _func)
		{
			throw new NotImplementedException();
		}

		public void Insert(int _index, T _row)
		{
			throw new NotImplementedException();
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

		public VirtualListVariable() : base(){
		}

		public VirtualListVariable(Func<int, T> _func) : base(){
			data = _func;
		}

		public void BuildVariable(ref string _json, ref int _counter, IStringParser _parser)
		{
			throwSetException ();
		}

		public void BuildFormattedString(ref string _json, IStringFormatter _formatter){
			_formatter.BuildFormattedString(this, ref _json);
		}

		private void throwSetException(){
			throw new SystemException ("this variable not support to write");
		}

		public new void NotifyChanged(string _message = ""){
			IsChanged = true;
			OnChangedValueActions (this, _message);
		}
	}
}