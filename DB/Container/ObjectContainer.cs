using System;
using BicDB;
using BicDB.Variable;
using BicDB.Storage;

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
		private event Action<IObjectContainer<T>> onChangedValueActions = delegate{};

		[Obsolete("use Subscribe")]
		public event Action<IObjectContainer<T>> OnChangedValueActions{
			add{
				onChangedValueActions += value;
			}

			remove{
				onChangedValueActions -= value;
			}
		}

		public T AsObject{
			get{ return data; }
			set{ data = value; NotifyChanged (); }
		}

		public void Subscribe(Action<IObjectContainer<T>> _callback, bool _needFirstCall = false){
			onChangedValueActions += _callback;
			if(_needFirstCall == true){
				_callback(this as IObjectContainer<T>);
			}
		}

		public void Unsubscribe(Action<IObjectContainer<T>> _callback){
			onChangedValueActions -= _callback;
		}

		public void UnsubscribeAll(){
			onChangedValueActions = delegate{};
		}

		public void NotifyChanged(){
			onChangedValueActions (this);
		}


		public void NotifyChanged(ObjectContainer<T> _objectContainer){
			onChangedValueActions (this);
		}

		public void ClearNotifyAndBinding(){
			onChangedValueActions = delegate {};
		}
		#endregion

		#region IDataBase
		public DataType Type { get { return DataType.Object; }}

		public void BuildVariable(ref string _json, ref int _counter, IStringParser _parser)
		{
			//_parser.BuildStringVariable(this, ref _json, ref _counter);
		}

		public void BuildFormattedString(System.Text.StringBuilder _stringBuilder, IStringFormatter _formatter){
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
		event Action<IObjectContainer<T>> OnChangedValueActions;

		T AsObject{ get; set; }

		void NotifyChanged ();
	}
}