using System;
using BicDB;
using System.Linq;
using System.Collections.Generic;
using BicDB.Container;
using UnityEngine;
using BicDB.Storage;
using System.Text;

namespace BicDB.Variable
{
    public interface IVector2VariableReadOnly{
        IVariableReadOnly X {get;}
        IVariableReadOnly Y {get;}
        Vector2 AsVector{get;}
    }

    public class Vector2Variable : IVariable, IVector2VariableReadOnly
    {
        private FloatVariable x = new FloatVariable(0);
        private FloatVariable y = new FloatVariable(0);

        public IVariableReadOnly X {get=>x;}
        public IVariableReadOnly Y {get=>y;}

        public string AsString { 
            get{
                return string.Format("{0},{1}", this.X.AsString, this.Y.AsString);
            } 

            set {
                var _strings = value.Split(',');
                float _x;
                if(float.TryParse(_strings[0], out _x) == true){
                    this.x.AsFloat = _x;
                }

                float _y;
                if(float.TryParse(_strings[1], out _y) == true){
                    this.y.AsFloat = _y;
                }
            } 
        }


        public Vector2 AsVector{
            get => AsVectorWithoutNotify;
            set{ 
                AsVectorWithoutNotify = value;
                NotifyChanged ();
            }
        }

        public Vector2 AsVectorWithoutNotify{
            get{
                return new Vector2 (this.X.AsFloat, this.Y.AsFloat);
            }

            set{ 
                this.x.AsFloat = value.x;
                this.y.AsFloat = value.y;
            }
        }

        public int AsInt { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public float AsFloat { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool AsBool { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public DataType Type => DataType.Vector2;

        public IVariable AsVariable => throw new NotImplementedException();

        public Vector2Variable(float _x, float _y) : base(){
            this.x.AsFloat = _x;
            this.y.AsFloat = _y;
        }

        public Vector2Variable(){

        }

        #region Event
        private event Action<IVector2VariableReadOnly> onChangedValueActions;

        event Action<IVariableReadOnly> IVariableReadOnly.OnChangedValueActions{
            add{throw new NotImplementedException();}
            remove{throw new NotImplementedException();}
        }

        public void Subscribe(Action<IVector2VariableReadOnly> _callback, bool _needFirstCall = false){
            onChangedValueActions += _callback;
            if(_needFirstCall == true){
                _callback(this);
            }
        }

        public void Unsubscribe(Action<IVector2VariableReadOnly> _callback){
            onChangedValueActions -= _callback;
        }

        public void ClearNotifyAndBinding()
        {
            onChangedValueActions = null;
        }

        public void NotifyChanged()
        {
            if (onChangedValueActions != null) {
                onChangedValueActions (this);
            }
        }

        void IVariableReadOnly.NotifyChanged(IVariableReadOnly _value)
        {
            throw new NotImplementedException();
        }

        void IVariableReadOnly.Subscribe(Action<IVariableReadOnly> _callback, bool _needFirstCall = false)
        {
            throw new NotImplementedException();
        }

        void IVariableReadOnly.Unsubscribe(Action<IVariableReadOnly> _callback)
        {
            throw new NotImplementedException();
        }

        public void UnsubscribeAll()
        {
            onChangedValueActions = null;
        }
        #endregion

        public void BuildFormattedString(StringBuilder _stringBuilder, IStringFormatter _formatter)
        {
            _formatter.BuildFormattedString(this as IVariable, _stringBuilder);
        }

        public void BuildVariable(ref string _json, ref int _counter, IStringParser _parser)
        {
            _parser.BuildStringVariable(this, ref _json, ref _counter);
        }

        public bool IsEqual(IVariable _variable)
        {
            throw new NotImplementedException();
        }

        public bool Equals(Vector2 _target)
        {
            return _target.x == this.X.AsFloat && _target.y == this.Y.AsFloat;
        }

        T IDataBase.As<T>()
        {
            throw new NotImplementedException();
        }
    }
}