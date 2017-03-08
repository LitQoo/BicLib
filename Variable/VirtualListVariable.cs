using System;
using BicDB;
using System.Collections.Generic;

namespace BicDB.Variable
{

	public class VirtualListVariable<T> : VariableBase, IListVariable<T> where T : IVariable, new()
	{
		public new event Action<IListVariable<T>, string> OnChangedValueActions;
		public event Action<T> OnAddedValueActions = delegate{};
		public event Action OnClearedValueActions = delegate{};

		#region AsValue
		private Func<int, T> data;
		public int AsInt{ get{ return 0; } set{throwSetException ();} }
		public string AsString{ get{ return string.Empty; } set{ throwSetException ();} }
		public float AsFloat{ get{ return 0; } set{ throwSetException ();} }
		public bool AsBool{ get{ return false; } set{throwSetException ();} }
		public VariableType Type { get { return VariableType.List; }}
		#endregion


		#region IListVariable
		public T this [int _index] { 
			get{ 
				return data(_index);
			} 
		}

		public int GetSize(){
			return 0;
		}

		public void Add(T _value){
			throwSetException ();
		}

		public void RemoveAt(int _index){
			throwSetException ();
		}

		public bool Contains(T _value){
			throwSetException ();

			return false;
		}

		public void Clear(){
			throwSetException ();
		}
		#endregion

		public VirtualListVariable() : base(){
		}

		public VirtualListVariable(Func<int, T> _func) : base(){
			data = _func;
		}

		public void LoadValue(string _value){
			throwSetException ();
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