using System;
using BicDB;
using BicDB.Utility;

namespace BicDB.Container
{

	public class VirtualObjectContainer<T> : IObjectContainer<T>{

		#region Member
		private Func<T> getterFunc = null;
		private Action<T> setterAction = null;
		private T obj;
		private bool isFuncType = true;
		#endregion

		#region LifeCycle
		public VirtualObjectContainer(Func<T> _getter, Action<T> _setter){
			getterFunc = _getter;
			setterAction = _setter;
			isFuncType = true;
		}

		public VirtualObjectContainer(T _obj){
			obj = _obj;
			isFuncType = false;
		}
		#endregion

		#region IObjectContainer
		public event Action<IObjectContainer<T>, string> OnChangedValueActions = delegate{};

		public T AsObject{
			get{ 
				if (isFuncType) {
					return getterFunc ();
				} else {
					return obj;
				}
			}
			set{ 
				if (isFuncType) {
					setterAction (value);
				} else {
					obj = value;
				} 

				NotifyChanged (); 
			}
		}

		public void NotifyChanged(string _message = ""){
			OnChangedValueActions (this, _message);
		}
		#endregion

		#region IDataBase
		public DataType Type { get { return DataType.Object; }}

		public void BuildVariable(ref string _json, ref int _counter, IStringParser _parser)
		{
			//_parser.BuildStringVariable(this, ref _json, ref _counter);
		}

		public void BuildFormattedString(ref string _json, IStringFormatter _formatter){
			//_formatter.BuildFormattedString(this, ref _json);
		}

		public U As<U> () where U : class, IDataBase
		{
			return this as U;
		}
		public IVariable AsVariable {
			get {
				return null;
			}
		}
		#endregion
	}

}