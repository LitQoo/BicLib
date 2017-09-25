using System;
using BicDB;
using BicDB.Utility;

namespace BicDB.Container
{

	public class ObjectContainer<T> : IObjectContainer<T>
	{
		#region Member
		virtual protected T data{ get; set;}
		#endregion

		#region LifeCycle
		public ObjectContainer(){
		
		}

		public ObjectContainer(T _object){
			data = _object;
		}
		#endregion

		#region IObjectContainer
		public event Action<IObjectContainer<T>, string> OnChangedValueActions = delegate{};

		public T AsObject{
			get{ return data; }
			set{ data = value; NotifyChanged (); }
		}

		public void NotifyChanged(string _message = ""){
			OnChangedValueActions (this, _message);
		}


		public void NotifyChanged(ObjectContainer<T> _objectContainer ,string _message = ""){
			OnChangedValueActions (this, _message);
		}

		public void ClearNotifyAndBinding(){
			OnChangedValueActions = delegate {};
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

	public interface IObjectContainer<T> : IDataBase, IBindRmover
	{
		event Action<IObjectContainer<T>, string> OnChangedValueActions;

		T AsObject{ get; set; }

		void NotifyChanged (string _message = "");
	}

}