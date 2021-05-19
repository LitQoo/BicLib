using System;
using BicDB;
using System.Linq;
using System.Collections.Generic;
using BicDB.Container;
using UnityEngine;
using BicDB.Storage;

namespace BicDB.Variable
{
	public class VectorIntVariable : DictionaryContainer<IntVariable>, IBindRmover, IDataBase, IVariable
	{
		#region Event
		public event Action<VectorIntVariable> OnChangedValueActions;

		public void Subscribe(Action<VectorIntVariable> _callback, bool _needFirstCall = false){
			OnChangedValueActions += _callback;
			if(_needFirstCall == true){
				_callback(this);
			}
		}

		public void Unsubscribe(Action<VectorIntVariable> _callback){
			OnChangedValueActions -= _callback;
		}
		#endregion

		#region LifeCycle
		public VectorIntVariable(int x, int y) : base(){
			this ["x"] = new IntVariable (x);
			this ["y"] = new IntVariable (y);
		}

        public VectorIntVariable() : this(0, 0){
            
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
        public int X {
			get{ 
				return this ["x"].AsInt;
			}

            set{
                this["x"].AsInt = value;
                NotifyChanged();
            }
		}

		public int Y {
			get{ 
				return this ["y"].AsInt;
			}

            set{
                this["y"].AsInt = value;
                NotifyChanged();
            }
		}

		public Vector2Int AsVectorWithoutNotify{
			get{
				return new Vector2Int (this ["x"].AsInt, this ["y"].AsInt);
			}

			set{ 
				this ["x"].AsInt = value.x;
				this ["y"].AsInt = value.y;
			}
		}

		public Vector2Int AsVector{
			get => AsVectorWithoutNotify;
			set{ 
				AsVectorWithoutNotify = value;
				NotifyChanged ();
			}
		}

		public string AsString { 
			get{
				return string.Format("{0},{1}", this.X, this.Y);
			} 
			
			set {
				var _strings = value.Split(',');
				int _x;
				if(int.TryParse(_strings[0], out _x) == true){
					this.X = _x;
				}

				int _y;
				if(int.TryParse(_strings[1], out _y) == true){
					this.Y = _y;
				}
			} 
		}

		public override string ToString(){
			return this.AsString;
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

        void IVariableReadOnly.Subscribe(Action<IVariableReadOnly> _callback, bool _needFirstCall)
        {
            throw new NotImplementedException();
        }

        void IVariableReadOnly.Unsubscribe(Action<IVariableReadOnly> _callback)
        {
            throw new NotImplementedException();
        }

        void IVariableReadOnly.UnsubscribeAll()
        {
            throw new NotImplementedException();
        }

        void IVariableReadOnly.NotifyChanged(IVariableReadOnly _value)
        {
            throw new NotImplementedException();
        }

        public bool IsEqual(IVariable _variable)
        {
            throw new NotImplementedException();
        }

		public bool Equals(Vector2Int _target)
		{
			return _target.x == this.X && _target.y == this.Y;
		}
        #endregion
    }
}