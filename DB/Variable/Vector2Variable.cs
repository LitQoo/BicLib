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
        float X {get;}
        float Y {get;}
        Vector2 AsVector{get;}
    }

    public class Vector2Variable : IVariable, IVector2VariableReadOnly
    {
        private float x = 0;
        private float y = 0;

        public float X {get=>x;}
        public float Y {get=>y;}

        public string AsString { 
            get{
                return string.Format("{0},{1}", this.X, this.Y);
            } 

            set {
                var _strings = value.Split(',');
                
                float _x;
                if(float.TryParse(_strings[0], out _x) == true){
                    this.x = _x;
                }

                float _y;
                if(float.TryParse(_strings[1], out _y) == true){
                    this.y = _y;
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
                return new Vector2 (this.X, this.Y);
            }

            set{ 
                this.x = value.x;
                this.y = value.y;
            }
        }

        public int AsInt { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public float AsFloat { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool AsBool { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public DataType Type => DataType.Vector2;

        public IVariable AsVariable => this;

        public Vector2Variable(float _x, float _y) : base(){
            this.x = _x;
            this.y = _y;
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
            if(_json[_counter] == '"'){
                _parser.BuildStringVariable(this, ref _json, ref _counter);
            }else{
                var _str = "";
                while(true){
                    _str += (_json[_counter]);
                    if(_json[_counter] == '}'){
                        break;
                    }
                    _counter++;
                }

                var _match = System.Text.RegularExpressions.Regex.Matches(_str, @"""([xy])"":([-+]?\d+(.\d+)?)");
                
                if(_match[0].Result("$1") == "x"){
                    this.x = float.Parse(_match[0].Result("$2"));
                    this.y = float.Parse(_match[1].Result("$2"));
                }else{
                    this.x = float.Parse(_match[1].Result("$2"));
                    this.y = float.Parse(_match[0].Result("$2"));
                }
                
                _counter++;
            }
        }

        public bool IsEqual(IVariable _variable)
        {
            throw new NotImplementedException();
        }

        public bool Equals(Vector2 _target)
        {
            return _target.x == this.X && _target.y == this.Y;
        }

        T IDataBase.As<T>()
        {
            throw new NotImplementedException();
        }
    }
}