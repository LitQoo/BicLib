using System;
using BicDB;
using System.Linq;
using System.Collections.Generic;
using BicDB.Container;
using UnityEngine;
using BicDB.Storage;

namespace BicDB.Variable
{
	public class VectorVariable : DictionaryContainer<FloatVariable>, IBindRmover, IDataBase, IVariable
	{
		#region Event
		public event Action<VectorVariable> OnChangedValueActions;
		#endregion

		#region LifeCycle
		public VectorVariable(float x, float y) : base(){
			this ["x"] = new FloatVariable (x);
			this ["y"] = new FloatVariable (y);
		}

        event Action<IVariableReadOnly> IVariableReadOnly.OnChangedValueActions
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }
        #endregion

        #region Member
        public FloatVariable X {
			get{ 
				return this ["x"];
			}
		}

		public FloatVariable Y {
			get{ 
				return this ["y"];
			}
		}

		public Vector2 AsVector{
			get{
				return new Vector2 (this ["x"].AsFloat, this ["y"].AsFloat);
			}

			set{ 
				this ["x"].AsFloat = value.x;
				this ["y"].AsFloat = value.y;
				NotifyChanged ();
			}
		}

		public string AsString { 
			get{
				return string.Format("{0},{1}", this.X, this.Y);
			} 

			set {
				var _strings = value.Split(',');
				float _x;
				if(float.TryParse(_strings[0], out _x) == true){
					this["x"].AsFloat = _x;
				}

				float _y;
				if(float.TryParse(_strings[1], out _y) == true){
					this["y"].AsFloat = _y;
				}
			} 
		}

        public int AsInt { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public float AsFloat { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool AsBool { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        #endregion

        #region Logic
		public new void BuildVariable(ref string _json, ref int _counter, IStringParser _parser)
		{
			if(_json[_counter] == '"'){
				_parser.BuildStringVariable(this, ref _json, ref _counter);
			}else{
				_parser.BuildDictionaryContainer(this, ref _json, ref _counter);
			}
		}

		public new void BuildFormattedString(System.Text.StringBuilder _stringBuilder, IStringFormatter _formatter){
			_formatter.BuildFormattedString(this as IVariable, _stringBuilder);
		}
		
        public void NotifyChanged(){
			if (OnChangedValueActions != null) {
				OnChangedValueActions (this);
			}
		}

		public void ClearNotifyAndBinding (){
			OnChangedValueActions = null;
		}

        public void Subscribe(Action<IVariableReadOnly> _callback, bool _needFirstCall = false)
        {
            throw new NotImplementedException();
        }

        public void Unsubscribe(Action<IVariableReadOnly> _callback)
        {
            throw new NotImplementedException();
        }

        public void UnsubscribeAll()
        {
            throw new NotImplementedException();
        }

        public void NotifyChanged(IVariableReadOnly _value)
        {
            throw new NotImplementedException();
        }

        public bool IsEqual(IVariable _variable)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}