using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BicDB.Variable;
using System;
using BicDB;
using BicDB.Container;
using BicUtil.ListValueSelector;

namespace BicUtil.MVCSystem
{
	public class ModelBinder
	{
		#region Binding
		private List<Action> bindRemoverList = new List<Action>();

		public void ClearBinding(){
			for (int i = 0; i < bindRemoverList.Count; i++) {
				bindRemoverList [i] ();
			}

			bindRemoverList.Clear ();
		}
		#endregion

		#region Model -> Controller Binding
		public void BindModelToController (VectorVariable _variable, Action<VectorVariable> _func, bool _needFirstCall = false){
			bindRemoverList.Add (()=>{
				_variable.OnChangedValueActions -= _func;
			});

			_variable.OnChangedValueActions += _func;

			if (_needFirstCall == true) {
				_func (_variable);
			}
		}

		public void BindModelToController (VectorIntVariable _variable, Action<VectorIntVariable> _func, bool _needFirstCall = false){
			bindRemoverList.Add (()=>{
				_variable.OnChangedValueActions -= _func;
			});

			_variable.OnChangedValueActions += _func;

			if (_needFirstCall == true) {
				_func (_variable);
			}
		}

		public void BindModelToController (ColorVariable _variable, Action<ColorVariable> _func, bool _needFirstCall = false){
			bindRemoverList.Add (()=>{
				_variable.Unsubscribe(_func);
			});

			_variable.Subscribe(_func, _needFirstCall);
		}

		public void BindModelToController (IVariable _variable, Action<IVariable> _func, bool _needFirstCall = false){
			bindRemoverList.Add (()=>{
				_variable.Unsubscribe(_func);
			});

			_variable.Subscribe(_func, _needFirstCall);
		}

		public void BindModelToController<U> (IEnumVariable<U> _variable, Action<IEnumVariable<U>> _func, bool _needFirstCall = false) where U : struct{
            bindRemoverList.Add (()=>{
				_variable.Unsubscribe(_func);
			});

			_variable.Subscribe(_func, _needFirstCall);
		}

		public void BindModelToController(IVariable _variable, UnityEngine.UI.Text _text, bool _needFirstCall = false){
			Action<IVariable> _func = (__variable)=>{
				_text.text = __variable.AsString;
			};
			
			bindRemoverList.Add (()=>{
				_variable.Unsubscribe(_func);
			});

			_variable.Subscribe(_func);

			if(_needFirstCall == true){
				_text.text = _variable.AsString;
			}
		}

		public void BindModelToController(IVariable _variable, UnityEngine.UI.Text _text, string _format, bool _needFirstCall = false){
			Action<IVariable> _func = (__variable)=>{
				_text.text = __variable.AsString;
			};

			bindRemoverList.Add (()=>{
				_variable.Unsubscribe(_func);
			});

			_variable.Subscribe(_func);

			if(_needFirstCall == true){
				_text.text = string.Format(_format, _variable.AsString);
			}
		}

		public void BindModelToController (IVariable _variable, Action _func, bool _needFirstCall = false){
			BindModelToController (_variable, (IVariable __variable) => _func (), _needFirstCall);
		}

		public void BindModelToController<U> (IObjectContainer<U> _container, Action<IObjectContainer<U>> _func, bool _needFirstCall = false){
			
			bindRemoverList.Add (()=>{
				_container.OnChangedValueActions -= _func;
			});

			_container.OnChangedValueActions += _func;

			if (_needFirstCall == true) {
				_func (_container);
			}
		}

		public void BindModelToController<U> (ListValueSelector<U> _listValueSelector, Action<U> _func, bool _needFirstCall = false)  where U : class{
			bindRemoverList.Add (()=>{
				_listValueSelector.OnChangedValueActions -= _func;
			});
			
			_listValueSelector.OnChangedValueActions += _func;

			if (_needFirstCall == true) {
				_func (_listValueSelector.Current);
			}
		}

		public void BindModelToController (IRecordContainer _model, Action<IRecordContainer, string> _func, bool _needFirstCall = false){
			bindRemoverList.Add (()=>{
				_model.OnChangedValueActions -= _func;
			});
			
			_model.OnChangedValueActions += _func;

			if (_needFirstCall == true) {
				_func (_model, string.Empty);
			}
		}

		public void BindModelToControllerOnAdd<T>(ListContainer<T> _list, Action<T> _func, bool _needFirstCall) where T : IDataBase, new(){
			bindRemoverList.Add (()=>{
				_list.UnsubscribeOnAdded(_func);
			});

			_list.SubscribeOnAdded(_func);

			if (_needFirstCall == true) {
				for(int i = 0; i < _list.Count; i++){
					_func (_list[i]);
				}
			}
		}
		#endregion

		#region Controller -> Model Binding
		public void BindControllerToModel (VirtualStringVariable _variable, Func<string> _func){
			bindRemoverList.Add (()=>{
				_variable.Getter = null;
			});
			
			_variable.Getter = _func;
		}

		public void BindControllerToModel (VirtualIntVariable _variable, Func<int> _func){
			bindRemoverList.Add (()=>{
				_variable.Getter = null;
			});
			
			_variable.Getter = _func;
		}

		public void BindControllerToModel (VirtualBoolVariable _variable, Func<bool> _func){
			bindRemoverList.Add (()=>{
				_variable.Getter = null;
			});
			
			_variable.Getter = _func;
		}

		public void BindControllerToModel (VirtualFloatVariable _variable, Func<float> _func){
			bindRemoverList.Add (()=>{
				_variable.Getter = null;
			});
			
			_variable.Getter = _func;
		}

		public void BindControllerToModel<U>(VirtualObjectContainer<U> _variable, Func<U> _func){
			bindRemoverList.Add (()=>{
				_variable.Getter = null;
			});
			
			_variable.Getter = _func;
		}
		#endregion

		#region Controller <=> Model Binding
		public void BindTwoway(VirtualStringVariable _variable, Func<string> _func, Action<string> _action){
			bindRemoverList.Add (()=>{
				_variable.Getter = null;
				_variable.Setter = null;
			});
			
			_variable.Getter = _func;
			_variable.Setter = _action;
		}

		public void BindTwoway(VirtualIntVariable _variable, Func<int> _func, Action<int> _action){
			bindRemoverList.Add (()=>{
				_variable.Getter = null;
				_variable.Setter = null;
			});
			
			_variable.Getter = _func;
			_variable.Setter = _action;
		}

		public void BindTwoway(VirtualBoolVariable _variable, Func<bool> _func, Action<bool> _action){
			bindRemoverList.Add (()=>{
				_variable.Getter = null;
				_variable.Setter = null;
			});
			
			_variable.Getter = _func;
			_variable.Setter = _action;
		}

		public void BindTwoway(VirtualFloatVariable _variable, Func<float> _func, Action<float> _action){
			bindRemoverList.Add (()=>{
				_variable.Getter = null;
				_variable.Setter = null;
			});
			
			_variable.Getter = _func;
			_variable.Setter = _action;
		}

		public void BindTwoway<U>(VirtualObjectContainer<U> _variable, Func<U> _func, Action<U> _action){
			bindRemoverList.Add (()=>{
				_variable.Getter = null;
				_variable.Setter = null;
			});
			
			_variable.Getter = _func;
			_variable.Setter = _action;
		}

		public void BindTwoway<U>(VirtualEnumVariable<U> _variable, Func<U> _func, Action<U> _action) where U : struct{
			bindRemoverList.Add (()=>{
				_variable.Getter = null;
				_variable.Setter = null;
			});
			
			_variable.Getter = _func;
			_variable.Setter = _action;
		}
		#endregion
	}
}
