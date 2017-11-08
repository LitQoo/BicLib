using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BicDB.Variable;
using System;
using BicDB;
using BicDB.Container;

namespace BicUtil.MVCSystem
{
	public class ModelBinder<T>
	{
		#region MVC
		public T Controller{ get; set; }
		#endregion

		#region Binding
		private List<IBindRmover> bindRemoverList = new List<IBindRmover>();

		public void ClearBinding(){
			for (int i = 0; i < bindRemoverList.Count; i++) {
				bindRemoverList [i].ClearNotifyAndBinding ();
			}

			bindRemoverList.Clear ();
		}
		#endregion

		#region Model -> Controller Binding
		public void BindModelToController (VectorVariable _variable, Action<VectorVariable, string> _func, bool _needFirstCall = false){
			bindRemoverList.Add (_variable);
			_variable.OnChangedValueActions += _func;

			if (_needFirstCall == true) {
				_func (_variable, string.Empty);
			}
		}

		public void BindModelToController (IVariable _variable, Action<IVariable, string> _func, bool _needFirstCall = false){
			bindRemoverList.Add (_variable);
			_variable.OnChangedValueActions += _func;

			if (_needFirstCall == true) {
				_func (_variable, string.Empty);
			}
		}

		public void BindModelToController (IVariable _variable, Action _func, bool _needFirstCall = false){
			BindModelToController (_variable, (IVariable __variable, string __msg) => _func (), _needFirstCall);
		}

		public void BindModelToController<U> (IObjectContainer<U> _container, Action<IObjectContainer<U>, string> _func, bool _needFirstCall = false){
			bindRemoverList.Add (_container);
			_container.OnChangedValueActions += _func;

			if (_needFirstCall == true) {
				_func (_container, string.Empty);
			}
		}

		public void BindModelToController (IRecordContainer _model, Action<IRecordContainer, string> _func, bool _needFirstCall = false){
			bindRemoverList.Add (_model);
			_model.OnChangedValueActions += _func;

			if (_needFirstCall == true) {
				_func (_model, string.Empty);
			}
		}
		#endregion

		#region Controller -> Model Binding
		public void BindControllerToModel (VirtualStringVariable _variable, Func<string> _func){
			bindRemoverList.Add (_variable);
			_variable.Getter = _func;
		}

		public void BindControllerToModel (VirtualIntVariable _variable, Func<int> _func){
			bindRemoverList.Add (_variable);
			_variable.Getter = _func;
		}

		public void BindControllerToModel (VirtualBoolVariable _variable, Func<bool> _func){
			bindRemoverList.Add (_variable);
			_variable.Getter = _func;
		}

		public void BindControllerToModel (VirtualFloatVariable _variable, Func<float> _func){
			bindRemoverList.Add (_variable);
			_variable.Getter = _func;
		}

		public void BindControllerToModel<U>(VirtualObjectContainer<U> _variable, Func<U> _func){
			bindRemoverList.Add (_variable);
			_variable.Getter = _func;
		}
		#endregion

		#region Controller <=> Model Binding
		public void BindTwoway(VirtualStringVariable _variable, Func<string> _func, Action<string> _action){
			bindRemoverList.Add (_variable);
			_variable.Getter = _func;
			_variable.Setter = _action;
		}

		public void BindTwoway(VirtualIntVariable _variable, Func<int> _func, Action<int> _action){
			bindRemoverList.Add (_variable);
			_variable.Getter = _func;
			_variable.Setter = _action;
		}

		public void BindTwoway(VirtualBoolVariable _variable, Func<bool> _func, Action<bool> _action){
			bindRemoverList.Add (_variable);
			_variable.Getter = _func;
			_variable.Setter = _action;
		}

		public void BindTwoway(VirtualFloatVariable _variable, Func<float> _func, Action<float> _action){
			bindRemoverList.Add (_variable);
			_variable.Getter = _func;
			_variable.Setter = _action;
		}

		public void BindTwoway<U>(VirtualObjectContainer<U> _variable, Func<U> _func, Action<U> _action){
			bindRemoverList.Add (_variable);
			_variable.Getter = _func;
			_variable.Setter = _action;
		}

		public void BindTwoway<U>(VirtualEnumVariable<U> _variable, Func<U> _func, Action<U> _action) where U : struct{
			bindRemoverList.Add (_variable);
			_variable.Getter = _func;
			_variable.Setter = _action;
		}
		#endregion
	}
}
